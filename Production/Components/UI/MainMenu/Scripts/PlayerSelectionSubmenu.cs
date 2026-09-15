using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerSelectionSubmenu : SubmenuBase
{
    public PlayerCurserContainer _playerCurserContainer;
    [Export]
    public TextureRect _confirmationBanner;
    [Export]
    public GridContainer _characterTiles;
    private List<CharacterButton> _tiles = new List<CharacterButton>();
    private InputSystem _inputSystem;
    private bool _playersReady = false;
    [Signal] public delegate void OnCharacterSelectionCompleteEventHandler();

    public override void _Ready()
    {
        base._Ready();
        _playerCurserContainer = PlayerCurserContainer.GetInstance();
        _inputSystem = InputSystem.GetInstance();
        //_playerCurserContainer.SetCursorsActive(false);
        _playerCurserContainer.OnPlayersReady += OnPlayersReady;
        _playerCurserContainer.OnCharacterPlayerChanged += OnCharacterPlayerChanged;

        var tiles = _characterTiles.GetChildren();
        for(int i = 0; i < tiles.Count; i++)
        {
            CharacterButton cbtn = (CharacterButton)tiles[i];
            _tiles.Add(cbtn);
        }   
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        _playerCurserContainer.OnPlayersReady -= OnPlayersReady;
        _playerCurserContainer.OnCharacterPlayerChanged -= OnCharacterPlayerChanged;
    }


    public void OnCharacterPlayerChanged(int playerId, int characterId)
    {
        if(characterId == -1)
            return;
            
        _tiles[characterId].SetPlayerToButton(playerId);
    }

    public void SetCursorsAtTiles()
    {     
        foreach(PlayerCurser cursor in _playerCurserContainer.CurserList)
        {
            int selectedChar = GameManager.GetInstance().PlayerCharacters[cursor.Id];
            if(selectedChar != -1)
            {       
                cursor.Position = _tiles[selectedChar].GlobalPosition + _tiles[selectedChar].Size/2;
                //_tiles[selectedChar].SetFocused(cursor);
                cursor._justOpened = true;
            }
        }
    }

    public void SetupCursors()
    {
        foreach(PlayerCurser cursor in _playerCurserContainer.CurserList)
        {
            int selectedChar = GameManager.GetInstance().PlayerCharacters[cursor.Id];
            if(selectedChar != -1)
            {       
                _tiles[selectedChar].SetCursorOnOpen(cursor);
            }
            if(_playersReady)
            {
                cursor.ButtonPressed += ReadyConfirm;
            }
        }
        
    }

    public void ResetCursors()
    {
        foreach(PlayerCurser cursor in _playerCurserContainer.CurserList)
        {
            int selectedChar = GameManager.GetInstance().PlayerCharacters[cursor.Id];
            if(selectedChar != -1)
            {       
                _tiles[selectedChar].ResetCursorOnClose(cursor);
            }
            if(_playersReady)
            {
                cursor.ButtonPressed -= ReadyConfirm;
            }
        }
        
    }

    public override void Close()
    {
        base.Close();
        ResetCursors();
        _inputSystem.AllowControllerActivation(false);
        if(_playerCurserContainer._restrictMenuNav)
        {
            _playerCurserContainer.RestrictMenuNavigation();
        }
        
        //_playerCurserContainer.SetCursorsActive(false);
        //_playerCurserContainer.DespawnCursors();

        //_inputSystem.RemoveActiveInputMode(EInputSystemMode.PLAYER_MOVEMENT);
        //_inputSystem.AddActiveInputMode(EInputSystemMode.MENU);
    }
    public override void Open()
    {
        _inputSystem.AllowControllerActivation(true);
        //SetCursorsAtTiles();
        SetupCursors();
        if(_playerCurserContainer._restrictMenuNav)
        {
            _playerCurserContainer.EnableMenuNavigationAll();
        }
        base.Open();
        //_playerCurserContainer.CallDeferred("SpawnCursors");

        //_playerCurserContainer.SetCursorsActive(true);
        //_inputSystem.AddActiveInputMode(EInputSystemMode.PLAYER_MOVEMENT);
        //_inputSystem.RemoveActiveInputMode(EInputSystemMode.MENU);
    }

    public override void Confirm()
    {
        if(_playersReady)
        {
            EmitSignal(nameof(OnCharacterSelectionComplete));
        }
    }

    public void ReadyConfirm(PlayerCurser cursor)
    {
        EmitSignal(nameof(OnCharacterSelectionComplete));
    }

    public override bool Back()
    {
        if(_playersReady)
        {
            _playersReady = false;
            _confirmationBanner.Visible = false;
            //_inputSystem.AddActiveInputMode(EInputSystemMode.PLAYER_MOVEMENT);
            return false;
        }
        return true;
    }

    public void OnPlayersReady(bool ready)
    {
        if(!_playersReady && ready)
        {
            _characterTiles.ProcessMode = ProcessModeEnum.Disabled;
            foreach(PlayerCurser cursor in _playerCurserContainer.CurserList)
            {
                cursor.ButtonPressed += ReadyConfirm;
            }            
        }
        else if(_playersReady && !ready)
        {
            _characterTiles.ProcessMode = ProcessModeEnum.Inherit;
            foreach(PlayerCurser cursor in _playerCurserContainer.CurserList)
            {
                cursor.ButtonPressed -= ReadyConfirm;
            }            
        }
        _confirmationBanner.Visible = ready;
        _playersReady = ready;
    }

    public void SelectCharacter(PlayerCurser cursor, int characterId)
    {
        int assignedCharacter = GameManager.GetInstance().GetPlayerCharacter(cursor.Id);
        if(assignedCharacter != -1) // if character is already assigned
        {
            // deselected the already assigned character
            _tiles[assignedCharacter].DeselectCharacterButton();
        }
        cursor.SetCharacterSelected(characterId);
        cursor.SetReady(true);
		GameManager.GetInstance().SetPlayerCharacter(cursor.Id, characterId);

        _playerCurserContainer.CheckAllPlayersReady();
        //cursor.Freeze(true);
    }

    public void DeselectCharacter(PlayerCurser cursor, int characterId)
    {
        cursor.SetCharacterSelected(-1);
        cursor.SetReady(false);
		GameManager.GetInstance().SetPlayerCharacter(cursor.Id, -1);

        OnPlayersReady(false);
        //cursor.Freeze(false);
    }
}
