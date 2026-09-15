using Godot;
using Godot.Collections;
using System;
using System.Linq;

public partial class ControllersActivatorUI : Control
{
    private const string TEXT_HINT_CONFIRMED = "Waiting for others";
    private const string TEXT_HINT_NOT_CONFIRMED = "Press to confirm";

    [Export] private bool _preventClosing = false;
    [Export] private bool _debug = false;

    [ExportGroup("UI refs")]
    [Export] private Control _tilesHolder;
    [Export] private Label _lblJoiningState;
    [Export] private Label _lblTimer;

    [ExportGroup("Audio")]
    [Export] private AudioStreamPlayer _tickPlayer;
    [Export] private AudioStreamPlayer _confirmAudioPlayer;
    [Export] private AudioStreamPlayer _activateAudioPlayer;
    [Export] private AudioStreamPlayer _declineAudioPlayer;

    private InputSystem _inputSystem;
    private Array<ControllersActivatorTileUI> _tiles = new Array<ControllersActivatorTileUI>();

    //------------------------------------------
    // Override methods
    //------------------------------------------

    public override void _Ready()
    {
        if(_debug)
        {
            PlayerCurserContainer.GetInstance().HideCursors();    
        }

        Visible = true;

        // Tiles setup
        Array<Node> tileNodes = _tilesHolder.GetChildren();
        int i = 0;

        foreach (Node node in tileNodes)
        {
            ControllersActivatorTileUI tile = node as ControllersActivatorTileUI;
            _tiles.Add(tile);
            tile.SetPlayerId(i);
            i++;
        }

        // Input system setup
        _inputSystem = InputSystem.GetInstance();

        _inputSystem.OnControllerActivationAllowed += OnControllersActivationsAllowed;
        _inputSystem.OnControllerActivate += OnControllerActivate;
        _inputSystem.OnControllerDeactivate += OnControllerDeactivate;
        _inputSystem.OnNewControllerConnect += OnControllerConnect;
        _inputSystem.OnControllerDisconnect += OnControllerDisconnect;

        GD.Print("Activation hooked");

        // Check connected controllers
        for(int id = 0; id < _tiles.Count; id++)
        {
            InputSystemController controller = _inputSystem.GetConnectedControllerSlot(id);
            if (controller != null)
            {
                _tiles[id].SetState(EControllerActivationState.JOIN);
            }      
            else
            {
                _tiles[id].SetState(EControllerActivationState.DISCONNECTED);
                continue;
            }  
            // Check active controllers  
            controller = _inputSystem.GetActiveControllerSlot(id);
            if (controller != null)
            {
                _tiles[id].SetState(EControllerActivationState.ACTIVE);
            }
        }

        PlayerCurserContainer.GetInstance().OnPlayerJoined += OnPlayerJoined;
    }

    public override void _ExitTree(){
        _inputSystem.OnControllerActivationAllowed -= OnControllersActivationsAllowed;
        _inputSystem.OnControllerActivate -= OnControllerActivate;
        _inputSystem.OnControllerDeactivate -= OnControllerDeactivate;
        _inputSystem.OnNewControllerConnect -= OnControllerConnect;
        _inputSystem.OnControllerDisconnect -= OnControllerDisconnect;

        PlayerCurserContainer.GetInstance().OnPlayerJoined -= OnPlayerJoined;
    }

    public override void _Input(InputEvent @event){
        if (!Visible || _preventClosing || !_debug)
            return;

        if (@event.IsActionPressed(UIConstants.ACTION_CANCEL))
        {
            if (_inputSystem.ActiveControllersCount() != 0)
                _inputSystem.AllowControllerActivation(false);
        }
    }

    //------------------------------------------
    // Callbacks
    //------------------------------------------

    private void OnControllerConnect(InputSystemController controller)
    {
        for(int i = 0; i < _tiles.Count; i++)
        {
            if(_tiles[i].GetState() == EControllerActivationState.DISCONNECTED)
            {
                _tiles[i].SetState(EControllerActivationState.JOIN);
                break;
            }
        }
    }

    private void OnControllerDisconnect(InputSystemController controller)
    {
        // Reorder connected players
        int connectedConsCount = _inputSystem.GetNumberOfConnectedControllers() - 1;
        GD.Print("Connected controllers: " + connectedConsCount);
        for(int id = 0; id < _tiles.Count; id++)
        {
            //InputSystemController con = _inputSystem.GetConnectedControllerSlot(id);
            // if(con == controller)
            // {
            //     _tiles[id].SetState(EControllerActivationState.DISCONNECTED);
            //     continue;
            // }
            // if (con != null)
            // {
            //     _tiles[id].SetState(EControllerActivationState.JOIN);
            // }      
            // else
            // {
            //     _tiles[id].SetState(EControllerActivationState.DISCONNECTED);
            //     continue;
            // }  
            // Check active controllers  
            InputSystemController con = _inputSystem.GetActiveControllerSlot(id);
            if (con != null)
            {
                _tiles[id].SetState(EControllerActivationState.ACTIVE);
            }
            else if(id < connectedConsCount)
            {
                _tiles[id].SetState(EControllerActivationState.JOIN);
            }
            else
            {
                _tiles[id].SetState(EControllerActivationState.DISCONNECTED);
            }
        }      
    }

    private void OnControllerActivate(InputSystemController controller, int slotId)
    {
        _tiles[slotId].SetState(EControllerActivationState.ACTIVE);
        _confirmAudioPlayer.Play();
    }

    private void OnControllerDeactivate(InputSystemController controller, int slotId)
    {
        _tiles[slotId].SetState(EControllerActivationState.JOIN);
        _declineAudioPlayer.Play();
    }

    public void OnControllersActivationsAllowed(bool allowed)
    {
        if (!allowed)
        {
            GD.Print("Close OnControllersActivationsAllowed");
            Visible = false;
            return;
        }

        Visible = true;
        GD.Print("OnControllersActivationsAllowed!");

        GD.Print("Connected controllers: " + _inputSystem.GetConnectedControllers().Count);
        int connectedConsCount = _inputSystem.GetNumberOfConnectedControllers();
        for (int i = 0; i < _tiles.Count; i++)
        {
            InputSystemController con = _inputSystem.GetActiveControllerSlot(i);
            if (con != null)
            {
                _tiles[i].SetState(EControllerActivationState.ACTIVE);
            }
            else if(i < connectedConsCount)
            {
                _tiles[i].SetState(EControllerActivationState.JOIN);
            }
            else
            {
                _tiles[i].SetState(EControllerActivationState.DISCONNECTED);
            }
        }
    }

    public ControllersActivatorTileUI GetTile(int id)
    {
        return _tiles[id];
    }

    public void OnPlayerJoined(PlayerCurser cursor)
    {
        cursor.Position = _tiles[cursor.Id].GlobalPosition;
    }
}
