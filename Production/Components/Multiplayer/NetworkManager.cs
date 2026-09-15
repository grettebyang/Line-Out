using Godot;
using System;
using Godot.Collections;
using System.Linq;

// C# interface
public partial class NetworkManager : Node
{
    const int MAXIMUM_CONTROLLER_COUNT = 4;
    [Export] private Node _core;
    private InputSystem _localInputSystem;
    // If we are in a lobby we want to default to reading from the _OnlineInputSystem
    // which is shared accross computers, But still be sending input from _localInputSystem
    // Probably better to move _OnlineInputSystem to input system code instead
    public InputSystem _OnlineInputSystem { get; set; }
    // Singleton instance
    public static NetworkManager MultiplayerInstance;
    // Should be null if you're not in a lobby:
    private Dictionary<int, Godot.Collections.Array> _onlineControllers = new Dictionary<int, Godot.Collections.Array>();

    // Maps each connection (Godot Multiplayer peer ID) to an internal player index.
    // host = always index 0.
    private Dictionary<long, int> _peerToPlayerIndex = new Dictionary<long, int>();
    private Dictionary<int, long> _playerIndexToPeer = new Dictionary<int, long>();

    // Excludes host because we wanna use this for sending to clients
    private int _connectedPlayersToHost = 0;

    private NetworkManager()
    {
        if (MultiplayerInstance != null)
            return;

        MultiplayerInstance = this;
    }

    // Singleton Instance
    public static NetworkManager GetInstance()
    {
        return MultiplayerInstance;
    }
    public override void _Ready()
    {
        // Sanity checks
        if (MultiplayerInstance != null)
            return;
        if (_core == null)
        {
            return;
        }

        if (IsThisPcHost())
        {
            long hostPeer = Multiplayer.GetUniqueId();
            _peerToPlayerIndex[hostPeer] = 0;
            _playerIndexToPeer[0] = hostPeer;
        }
    }

    // If host, 
    public void receiveOnlineInputCallback(Dictionary packet)
    {
        long senderPeer = (long)packet["PeerId"];
        if (!_peerToPlayerIndex.ContainsKey(senderPeer))
        {
            GD.PrintErr($"receiveOnlineInputCallback: Unknown peer {senderPeer}");
            return;
        }
        int senderIndex = _peerToPlayerIndex[senderPeer];
        
        var incomingInputs = (Godot.Collections.Array)packet["Inputs"];
        concatLocalWithOnlineList(senderIndex, incomingInputs);

        // Host also inserts its own input into slot 0
        if (IsThisPcHost())
        {
            var hostInputs = gatherInputs();
            _onlineControllers[0] = hostInputs;

            // TODO: rebroadcast to clients
        }
    }

    // Update our onlineinput
    public void concatLocalWithOnlineList(int playerIndex, Godot.Collections.Array inputs)
    {
        _onlineControllers[playerIndex] = inputs;
    }

    public override void _PhysicsProcess(double delta)
    {
        // sendInputsToHost();
    }

    public void RegisterNewPeer(long peerId)
    {
        if (_peerToPlayerIndex.ContainsKey(peerId))
            return;

        int newIndex = _peerToPlayerIndex.Count; 
        _peerToPlayerIndex[peerId] = newIndex;
        _playerIndexToPeer[newIndex] = peerId;

        GD.Print($"Assigned peer {peerId} → slot {newIndex}");
    }

    // Local variable allocations are necessary
    private Godot.Collections.Array gatherInputs()
    {
        if (_localInputSystem == null)
            _localInputSystem = InputSystem.GetInstance();

        if (_core == null || !IsInstanceValid(_core))
        {
            return null;
        }
        Array<InputSystemController> activeControllers;
        activeControllers = _localInputSystem.GetOnlyActiveControllers();

        var inputsArray = new Godot.Collections.Array();

        for (int i = 0; i < activeControllers.Count; i++)
        {
            InputSystemController controller = activeControllers[i];
            if (controller == null)
                continue;

            float xInput = controller.GetInputAxis("left", "right");
            float zInput = controller.GetInputAxis("back", "front");

            // Only send data if it's non-zero
            if (Mathf.Abs(xInput) > 0.01f || Mathf.Abs(zInput) > 0.01f)
            {
                var inputDict = new Dictionary();
                inputDict["ControllerSlot"] = i;
                inputDict["X"] = xInput;
                inputDict["Z"] = zInput;

                inputsArray.Add(inputDict);
            }
        }
        return inputsArray.Count > 0 ? inputsArray : null;
    }
    
    // If sender is host, then we want to update
    // Functions commented away purpose not done because we probably want to 
    // Update InputSystemController in the input system instead of here
    // So this is just how it could work-ish
    private void updateOnlineControllerData(Godot.Collections.Array hostArray)
    {
        if (IsThisPcHost())
        {
            // Concatenate _OnlineInputSystem by from various _localInputSystems
            // Note: The host needs to take its own _localInputSystem as a special case
        }
        else if (!IsThisPcHost())
        {
            _OnlineInputSystem = GetOnlineInputSystem();
        }
        else
        {
            // Used for future debugging, remove later
            GD.PrintErr("Error with updateOnlineControllerData()");
        }
    }
    
    private void sendInputsToHost()
    {
        if (IsThisPcHost())
            return;

        var inputPacket = gatherInputs();
        if (inputPacket != null)
        {
            var packet = new Dictionary();
            packet["PeerId"] = Multiplayer.GetUniqueId();
            packet["Inputs"] = inputPacket;

            _core.Call("send_inputs", packet);
        }
    }

    private void onLineEdit(String text)
    {
        _core.Call("change_lobby_id", text);
    }

    // ------------------------------- //
    //          Public API             //
    // ------------------------------- //

    public InputSystem GetOnlineInputSystem()
    {
        if (_OnlineInputSystem != null)
            return _OnlineInputSystem;
        else
            return null;
    }
    public void CreateHostedLobby()
    {
        _core.Call("_create_host_lobby");
    }

    // Id is fetched somewhat hard-coded from core, maybe change that later
    public void JoinLobby(int lobbyId)
    {
        _core.Call("join_lobby", lobbyId);
    }
    public long GetLobbyID()
    {
        return (long)_core.Call("get_lobby_id");
    }

    public int GetLobbyMembers()
    {
        return (int)_core.Call("get_lobby_members");
    }

    public void LeaveLobby()
    {
        _core.Call("leave_lobby");
    }

    public bool IsLobbyEmpty()
    {
        return (bool)_core.Call("is_lobby_empty");
    }

    public bool IsThisPcHost()
    {
        if (_core != null && IsInstanceValid(_core))
            return (bool)_core.Call("is_this_pc_host");
        // GD.PrintErr("Multiplayer _core is not available in network manager");
        return false;
    }
}
