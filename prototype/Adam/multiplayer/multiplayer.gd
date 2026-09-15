extends CanvasLayer

# Input singleton to be removed later

const PACKET_READ_LIMIT: int = 32

var is_host: bool = false
var lobby_id: int = 0
var lobby_members: Array = []
var lobby_members_max: int = 4

var AppID: String = "480"
var global_steam_id: int = 0
var global_steam_username: String = ""

@onready var global_lobby_id = $LobbyID

@export var host: Button
@export var join: Button

func _init():
	OS.set_environment("SteamAppID", AppID)
	OS.set_environment("SteamGameID", AppID)

func _ready():
	if host:
		host.pressed.connect(_on_host_pressed)
	if join:
		join.pressed.connect(_on_join_pressed)
	
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

# Button callbacks
func _on_host_pressed():
	create_lobby()

func _on_join_pressed():
	var id: int = int(global_lobby_id.text)
	join_lobby(id)

# Lobby functions
func create_lobby():
	if lobby_id == 0:
		Steam.createLobby(Steam.LOBBY_TYPE_PUBLIC, lobby_members_max)
	else:
		print("Already in a lobby, skipping creation")

func _on_lobby_created(result: int, this_lobby_id: int):
	if result == 1:
		lobby_id = this_lobby_id
		Steam.setLobbyData(lobby_id, "name", "Adams Lobby")
		Steam.allowP2PPacketRelay(true)
		print("Lobby successfully created! ID:", lobby_id)
	else:
		push_error("Failed to create lobby! Result code: " + str(result))

func join_lobby(this_lobby_id: int):
	print("Joining lobby with ID:", this_lobby_id)
	Steam.joinLobby(this_lobby_id)

func _on_lobby_joined(this_lobby_id: int, _permissions: int, _locked: bool, response: int):
	if response == Steam.CHAT_ROOM_ENTER_RESPONSE_SUCCESS:
		lobby_id = this_lobby_id
		print("Joined lobby:", lobby_id)
		get_lobby_members()
		make_p2p_handshake()
	else:
		push_error("Failed to join lobby! Response code: " + str(response))

func get_lobby_members():
	lobby_members.clear()
	var num_members = Steam.getNumLobbyMembers(lobby_id)
	for i in range(num_members):
		var member_id = Steam.getLobbyMemberByIndex(lobby_id, i)
		var member_name = Steam.getFriendPersonaName(member_id)
		lobby_members.append({"steam_id": member_id, "steam_name": member_name})

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
				Steam.sendP2PPacket(member["steam_id"], data, send_type, channel)
	else:
		Steam.sendP2PPacket(target_id, data, send_type, channel)

func _on_p2p_session_request(remote_id: int):
	Steam.acceptP2PSessionWithUser(remote_id)

func read_all_p2p_packets(read_count: int = 0):
	if read_count >= PACKET_READ_LIMIT:
		return
	while Steam.getAvailableP2PPacketSize(0) > 0:
		read_p2p_packet()
		read_count += 1

func read_p2p_packet():
	var size = Steam.getAvailableP2PPacketSize(0)
	if size > 0:
		var packet = Steam.readP2PPacket(size, 0)
		var sender = packet["remote_steam_id"]
		var data = bytes_to_var(packet["data"])
		if data.has("message"):
			match data["message"]:
				"handshake":
					print("PLAYER:", data["username"], "has joined!", "Data: ", data)
					get_lobby_members()

				"player_jump":
					print("Player of ID", sender, "jumped!")

func send_jump():
	print("Sending jump package")
	send_p2p_packet(0, {
		"message": "player_jump",
		"steam_id": global_steam_id
	}, 0)
