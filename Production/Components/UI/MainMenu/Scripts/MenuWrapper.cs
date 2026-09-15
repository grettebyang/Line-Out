using Godot;
using Godot.Collections;
using System;
using System.Resources;

public partial class MenuWrapper : Control
{
    [Export] protected SubmenuBase _mainSelection; // Default selection of the menu

    [Export] protected Control _backButton;
    [Export] protected HBackHold _backButtonHold;
    [Export] protected Control _confirmButton;

    protected Array<SubmenuBase> _menuHistory = new Array<SubmenuBase>();
    protected SubmenuBase _activeSubmenu;
    protected bool _holdToGoBack = false;
    protected float _backButtonHoldTime = 0.0f;
    protected float _maxHoldTime = 1.0f;
    protected bool _backHeld = false;

    private PlayerCurserContainer _playerCursorContainer;

    [Signal] public delegate void OnCloseEventHandler();

    //------------------------------------------
    // Override
    //------------------------------------------

    public override void _Ready(){
        OpenSubmenu(_mainSelection, true);
        _playerCursorContainer = PlayerCurserContainer.GetInstance();
        foreach(PlayerCurser cursor in _playerCursorContainer.CurserList)
        {
            CursorInputHookup(cursor);   
        }
        _playerCursorContainer.OnPlayerJoined += CursorInputHookup;
    }

    public void CursorInputHookup(PlayerCurser cursor)
    {
        cursor.BackPressed += OnReturnButtonPressed;
    }

    public override void _ExitTree()
    {
        _playerCursorContainer.OnPlayerJoined -= CursorInputHookup;
        foreach(PlayerCurser cursor in _playerCursorContainer.CurserList)
        {
            cursor.BackPressed -= OnReturnButtonPressed;
            //cursor.BackReleased -= OnReturnButtonReleased;       
        }
    }

    public override void _Process(double delta){
        if(_holdToGoBack && _backHeld)
        {
            _backButtonHoldTime += (float)delta;
            if(_backButtonHoldTime >= .15f)
            {
                _backButtonHold._buttonSymbol.Scale = new Vector2(1.2f, 1.2f);
                if(_backButtonHoldTime >= _maxHoldTime)
                {
                    _backHeld = false;
                    _backButtonHoldTime = 0.0f;
                    _backButtonHold._buttonSymbol.Scale = new Vector2(1f, 1f);
                    OnBackPressed();
                }
                _backButtonHold._holdTimePie.Value = _backButtonHoldTime;
            }
        }
    }

    public void OnReturnButtonPressed(PlayerCurser cursor)
    {
        GD.Print("On return button pressed");
        if(!_activeSubmenu._canControl)
            return;

        if(!_holdToGoBack)
        {
            OnBackPressed();
        }
        else if(!_backHeld)
        {
            _backHeld = true;
            cursor.BackReleased += OnReturnButtonReleased;
        }
    }

    public void OnReturnButtonReleased(PlayerCurser cursor)
    {    
        GD.Print("On return button released");
        _backHeld = false;
        _backButtonHoldTime = 0.0f;
        _backButtonHold._buttonSymbol.Scale = new Vector2(1f, 1f);
        _backButtonHold._holdTimePie.Value = _backButtonHoldTime;  
        cursor.BackReleased -= OnReturnButtonReleased;  
    }

    //------------------------------------------
    // Custom
    //------------------------------------------

    // Hide previous menu and display selected menu
    protected virtual void OpenSubmenu(SubmenuBase submenu, bool resetFocus = false, bool savePrevious = false){
        if (_activeSubmenu != null)
            _activeSubmenu.Close();

        // Add to history
        if (savePrevious)
            _menuHistory.Add(_activeSubmenu);

        // Update menu
        _activeSubmenu = submenu;
        //GD.Print("Last _activeSubmenu: " + _activeSubmenu);
        _activeSubmenu.Open();
        _holdToGoBack = _activeSubmenu._holdToGoBack;

        InputSystem.GetInstance().ClearFrameInput();

        // Activate button
        // if (resetFocus){
        //     CursorButton selected = null;
        //     if (submenu._mainButtons.Count > 0)
        //         selected = submenu._mainButtons[0];

        //     if (selected != null)
        //         selected.GrabFocus();
        // }
        // else
        // {
        //     submenu._selectedButton.GrabFocus();
        // }
    }

    protected virtual void OnConfirmPressed(){
        //GD.Print("_activeSubmenu: " + _activeSubmenu);
        _activeSubmenu.Confirm();
    }

    protected virtual void OnBackPressed(){
        int last = _menuHistory.Count - 1;
        if (last == -1)
            return;

        // Back and remove from history
        if(_activeSubmenu.Back())
        {
            OpenSubmenu(_menuHistory[last]);
            _menuHistory.RemoveAt(last);
        }
        else
        {
            _activeSubmenu._goBackToPreviousMenu += DeferMenuBackPressed; // Not sure how safe this is to do in various different circumstances
            return;
        }
    }

    protected void DeferMenuBackPressed(bool goback)
    {
        _activeSubmenu._goBackToPreviousMenu -= DeferMenuBackPressed;
        if(goback)
            OnBackPressed();
    }
}
