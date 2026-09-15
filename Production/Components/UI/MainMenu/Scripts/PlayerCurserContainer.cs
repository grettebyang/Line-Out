using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;

public partial class PlayerCurserContainer : Control
{
    private ControllersActivatorUI _controllerActivator;
    private Control _characterTiles;
    public List<PlayerCurser> CurserList = new List<PlayerCurser>();
    private InputSystem _inputSystem = InputSystem.GetInstance();
    private float _delayTimer = 0.0f;
    private float _delaytime = 0.3f;
    public bool _restrictMenuNav;

    [Signal] public delegate void OnPlayersReadyEventHandler(bool ready);    
    [Signal] public delegate void OnPlayerJoinedEventHandler(PlayerCurser cursor);    
    [Signal] public delegate void OnCharacterPlayerChangedEventHandler(int playerId, int characterId);  
    
    
    //------------------------------------------
    // Singleton
    //------------------------------------------

    private static PlayerCurserContainer _instance;

    private PlayerCurserContainer()
	{
		if (_instance != null)
            return;

		_instance = this;
	}

    public static PlayerCurserContainer GetInstance()
	{
		return _instance;
	}



    public override void _Ready()
    {
        // Listen to controller activation
        SpawnCursors();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if(_delayTimer > 0)
        {
            _delayTimer -= (float)delta;
            if(_delayTimer <= 0)
            {
                foreach(PlayerCurser cursor in CurserList)
                {
                    cursor.ProcessMode = ProcessModeEnum.Inherit;
                }
            }
        }
    }

    public void SetDelay()
    {
        _delayTimer = _delaytime;
    }


    public void SpawnCursors()
    {
        _inputSystem.OnControllerActivate += AddPlayerCurser;
        _inputSystem.OnControllerActivate += CancelReadyBanner;
        _inputSystem.OnControllerDeactivate += RemovePlayerCurser;
        _inputSystem.OnControllerDisconnect += ReorderPlayerCursors;
        InputSystemController[] controllers = _inputSystem.GetActiveControllersSlots();
        for (int i = 0; i < controllers.Length; i++){
            if (controllers[i] == null)
                continue;

            // Spawn
            AddPlayerCurser(controllers[i], i);
        }
    }

    public void DespawnCursors()
    {
        _inputSystem.OnControllerActivate -= AddPlayerCurser;
        _inputSystem.OnControllerActivate -= CancelReadyBanner;
        _inputSystem.OnControllerDeactivate -= RemovePlayerCurser;
        _inputSystem.OnControllerDisconnect -= ReorderPlayerCursors;
        foreach(PlayerCurser cursor in CurserList)
        {
            cursor.QueueFree();   
        }
        CurserList.Clear();
    }

    public void HideCursors()
    {
        SetCursorsActive(false);
        Visible = false;
    }

    public void ShowCursors()
    {
        SetCursorsActive(true);
        Visible = true;
    }

    public void SetCursorsAtCollisionLayer(uint layer)
    {
        foreach(PlayerCurser cursor in CurserList)
        {
            cursor.SetCollisionLayerValue(1, true);
        }
    }

    public void CheckAllPlayersReady()
    {
        foreach(PlayerCurser cursor in CurserList)
        {
            if(!cursor.HasSelection())
            {
                return;
            }
        }
        // Bring up confirmation screen/message
        //_inputSystem.RemoveActiveInputMode(EInputSystemMode.PLAYER_MOVEMENT);
        EmitSignal(nameof(OnPlayersReady), true);
    }

    public void SetCursorsActive(bool active)
    {
        foreach(PlayerCurser curser in CurserList)
        {
            curser.SetActive(active);
            curser._justOpened = true; // this is so that menu input will not affect selection input, temporary solution
        }
    }

    public void SetCursorsFrozen(bool active)
    {
        foreach(PlayerCurser curser in CurserList)
        {
            curser.Freeze(active);
        }
    }

    public void SetCursorsAtTiles(List<CharacterButton> tiles)
    {
        foreach(PlayerCurser cursor in CurserList)
        {
            int selectedChar = GameManager.GetInstance().PlayerCharacters[cursor.Id];
            if(selectedChar != -1)
            {
                CharacterButton cbtn = tiles[selectedChar];
                cursor.Position = cbtn.GlobalPosition + cbtn.Size/2;       
                cursor.Freeze(true);
                cbtn.CallDeferred("SetCursorToButton", cursor);
            }
        }
    }

    public void AddPlayerCurser(InputSystemController controller, int id)
    {
        PlayerCurser curser = ResourceLoader.Load<PackedScene>("res://Production/Components/UI/ControllersActivator/Layouts/PlayerCurser.tscn").Instantiate() as PlayerCurser;
        CurserList.Add(curser);
        curser.SetPlayerId(id);
        curser.OnCharacterSelected += CheckAllPlayersReady;

        // If the character is already set, set tiles that are already selected
        int selectedChar = GameManager.GetInstance().PlayerCharacters[id];
        // GD.Print("player " + id + " character selected: " + selectedChar);

        // Set position and color to corresponding activator tile
        curser.SetReady(false);
        //curser.Position = _controllerActivator.GetTile(id).GlobalPosition;
        curser.Position = new Vector2(GetWindow().GetScreenTransform().X.X/2, GetWindow().GetScreenTransform().Y.Y/2);
                
        // GD.Print("tile position: " + _controllerActivator.GetTile(id).GlobalPosition);
        if(id > 0 && _restrictMenuNav)
        {
            curser.DisableCursorNavigation();
        }

        curser._justOpened = true;
        curser.SetInputController(controller, id);
        curser.SetActive(true);
        AddChild(curser);

        EmitSignal(nameof(OnPlayerJoined), curser);
    }

    public void RemovePlayerCurser(InputSystemController controller, int id)
    {
        foreach (PlayerCurser curser in CurserList) {
            if (curser == null)
                continue;
            if (curser is PlayerCurser playerCurser) {
                if (playerCurser.Id == id) {
                    // Remove character selection
                    playerCurser.DeselectCharacter();

                    playerCurser.QueueFree();
                    CurserList.Remove(curser);

                    CheckAllPlayersReady();

                    break;
                }
            }
        }
    }

    public void ReorderPlayerCursors(InputSystemController controller)
    {
        // Reorder player colors
        for(int i = 0; i < CurserList.Count; i++)
        {
            CurserList[i].SetPlayerId(i);
            // Change the selected character button's hover color and _selected to i
            EmitSignal(nameof(OnCharacterPlayerChanged), CurserList[i].Id, CurserList[i].GetCharacterSelected());
        }
    }

    public void CancelReadyBanner(InputSystemController controller, int id)
    {
        // When a new player joins
        //_inputSystem.AddActiveInputMode(EInputSystemMode.PLAYER_MOVEMENT);
        EmitSignal(nameof(OnPlayersReady), false);
    }

    public void RestrictMenuNavigation()
    {
        for(int i = 1; i < CurserList.Count; i++)
        {
            // Disable cursor
            CurserList[i].DisableCursorNavigation();
        }

    }

    public void EnableMenuNavigationAll()
    {
        for(int i = 1; i < CurserList.Count; i++)
        {
            // Enable cursor
            CurserList[i].EnableCursorNavigation();
        }
    }

    public void SetMenuNavRestriction(bool allow)
    {
        _restrictMenuNav = allow;
        if(_restrictMenuNav)
        {
            RestrictMenuNavigation();
        }
        else
        {
            EnableMenuNavigationAll();
        }
    }
}
