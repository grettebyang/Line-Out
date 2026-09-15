extends Node

const PACKET_READ_LIMIT: int = 32
# 480 is to be changed later when we have real steam ID
const AppID: String = "480"
const lobby_members_max: int = 4

var is_host: bool = false
var lobby_id: int = 0
var lobby_members: Array = []

var global_steam_id: int = 0
var global_steam_username: String = ""

func _init():
	OS.set_environment("SteamAppID", AppID)
	OS.set_environment("SteamGameID", AppID)

func _ready():
	if not Steam.steamInit():
		push_error("Steam failed to initialize!")
		return

	global_steam_id = Steam.getSteamID()
	print("Steam initialized! Steam ID:", global_steam_id)
	
	# Connect signals
	Steam.lobby_created.connect(_on_lobby_created)
	Steam.lobby_joined.connect(_on_lobby_joined)
	Steam.p2p_session_request.connect(_on_p2p_session_request)

func _process(_delta):
	Steam.run_callbacks()
	
	if lobby_id > 0:
		read_all_p2p_packets()

func initialize_steam() -> void:
	var initialize_response: Dictionary = Steam.steamInitEx()
	print("Did Steam initialize?: %s" % initialize_response)

	if initialize_response['status'] > Steam.STEAM_API_INIT_RESULT_OK:
		print("Failed to initialize Steam, shutting down: %s" % initialize_response)
		# Show quit prompt here potentially (Via C#)
		get_tree().quit()

# Lobby functions
func change_lobby_id(new_text: String):
	lobby_id = int(new_text)

func _create_host_lobby():
	create_lobby()

func _join_hosted_lobby():
	Steam.joinLobby(lobby_id)

func create_lobby():
	if lobby_id == 0:
		Steam.createLobby(Steam.LOBBY_TYPE_PUBLIC, lobby_members_max)
	else:
		print("Already in a lobby, skipping creation")

func _on_lobby_created(result: int, this_lobby_id: int):
	if result == 1:
		lobby_id = this_lobby_id
		# This should allow overlay joining but I haven't tested yet
		Steam.setLobbyJoinable(lobby_id, true)
		Steam.setLobbyData(lobby_id, "name", "Adams Lobby")
		Steam.allowP2PPacketRelay(true)
		print("Lobby successfully created! ID:", lobby_id)
		is_host = true
	else:
		push_error("Failed to create lobby! Result code: " + str(result))

func join_lobby(this_lobby_id: int):
	print("Joining lobby with ID:", this_lobby_id)
	Steam.joinLobby(this_lobby_id)

func _on_lobby_joined(this_lobby_id: int, _permissions: int, _locked: bool, response: int):
	if response == Steam.CHAT_ROOM_ENTER_RESPONSE_SUCCESS:
		lobby_id = this_lobby_id
		print("Joined lobby:", lobby_id)
		update_lobby_members()
		make_p2p_handshake()
	else:
		push_error("Failed to join lobby! Response code: " + str(response))

func leave_lobby() -> void:
	if lobby_id != 0:
		Steam.leaveLobby(lobby_id)
		for this_member in lobby_members:
			if this_member['steam_id'] != global_steam_id:
				Steam.closeP2PSessionWithUser(this_member['steam_id'])
		lobby_members.clear()
		lobby_id = 0
		is_host = false

func update_lobby_members():
	lobby_members.clear()
	var num_members = Steam.getNumLobbyMembers(lobby_id)
	for i in range(num_members):
		var member_id = Steam.getLobbyMemberByIndex(lobby_id, i)
		var member_name = Steam.getFriendPersonaName(member_id)
		lobby_members.append({"steam_id": member_id, "steam_name": member_name})

func get_lobby_members():
	return lobby_members

func is_this_pc_host() -> bool:
	return is_host

func is_lobby_empty() -> bool:
	return lobby_members.size() == 0

func get_lobby_id() -> int:
	return lobby_id

# P2P functions
func make_p2p_handshake():
	send_p2p_packet(0, {"message": "handshake", "steam_id": global_steam_id, "username": Steam.getPersonaName(), }, 0)

func send_p2p_packet(target_id: int, packet_data: Dictionary, send_type: int):
	var channel: int = 0
	var data: PackedByteArray
	data.append_array(var_to_bytes(packet_data))
	if target_id == 0:
		for member in lobby_members:
			if member["steam_id"] != global_steam_id:
				var result = Steam.sendP2PPacket(member["steam_id"], data, send_type, channel)
				print("Sent to ", member["steam_id"], " - Result: ", result)
	else:
		Steam.sendP2PPacket(target_id, data, send_type, channel)

func _on_p2p_session_request(remote_id: int):
	Steam.acceptP2PSessionWithUser(remote_id)

func _on_lobby_chat_update(lobby_id: int, changed_id: int, making_change_id: int, chat_state: int):
	if chat_state == Steam.CHAT_MEMBER_STATE_CHANGE_LEFT:
		update_lobby_members()	

func read_all_p2p_packets(read_count: int = 0):
	if read_count >= PACKET_READ_LIMIT:
		return
	while Steam.getAvailableP2PPacketSize(0) > 0:
		read_p2p_packet()
		read_count += 1

func read_p2p_packet():
	var size = Steam.getAvailableP2PPacketSize(0)
	if size > 0:
		var raw = Steam.readP2PPacket(size, 0)
		var sender = raw["remote_steam_id"]
		var data = bytes_to_var(raw["data"])
		
		if data is Dictionary and data.has("PeerId") and data.has("Inputs"):
			print("Received input packet from SteamID:", data["PeerId"])
			receive_inputs(data)
		# Legacy handshake to be removed in the future
		elif data is Dictionary and data.has("message") and data["message"] == "handshake":
			print("PLAYER:", data["username"], "has joined! Steam ID:", data["steam_id"])
			update_lobby_members()


func receive_inputs(packet: Dictionary):
	var peer_id = packet["PeerId"]
	
	# This is the Array sent from C#
	var inputs = packet["Inputs"]  
	print("Received input packet from peer:", peer_id)
	print("Inputs count:", inputs.size())

	for i in range(inputs.size()):
		print("  Input[", i, "]: ", inputs[i])

	NetworkManager.Instance.receiveOnlineInputCallback(packet)

func send_inputs(_input: Array):
	var packet := {
		"PeerId": global_steam_id,
		"Inputs": _input
	}
	send_p2p_packet(0, packet, 0)
