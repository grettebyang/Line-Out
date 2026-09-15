using Godot;
using Godot.Collections;
using System;
using System.IO;

public partial class SubmenuBase : Control
{
    [Export] protected Control _mainButtonsContainer;
    [Export] public bool _holdToGoBack;
    [Export] public bool _confirmHintButtonVisible;
    [Export] public bool _showControllerActivator;

    public Array<CursorButton> _mainButtons = new Array<CursorButton>();
    public CursorButton _selectedButton;

    protected float _openDelayTimer = 0.0f;
    protected float _delayTime = .3f;

    public delegate void GoBackToPreviousMenu(bool goback);
    public GoBackToPreviousMenu _goBackToPreviousMenu;

    public bool _canControl = true;

    public override void _Ready(){
        if (_mainButtonsContainer is null) {
            return;
        }

        foreach (Node node in _mainButtonsContainer.GetChildren()){
            CursorButton btn = node as CursorButton;
            if (btn == null)
                continue;

            _mainButtons.Add(btn);
        }
    }

    public override void _Process(double delta)
    {
        if(_openDelayTimer > 0)
        {
            _openDelayTimer -= (float)delta;
            if(_openDelayTimer <= 0)
            {
                foreach(CursorButton button in _mainButtons)
                {
                    button.ProcessMode = ProcessModeEnum.Inherit;
                }
            }
        }        
    }

    public override void _Input(InputEvent @event){
        // Check hover
        // foreach (CursorButton btn in _mainButtons){
        //     if (btn.IsHovered())
        //     {
        //         //GD.Print("Bnt hovered: " + btn.Name);
        //         btn.GrabFocus();
        //     }

        //     if (Input.IsMouseButtonPressed(MouseButton.Left) && btn.IsHovered())
        //         Confirm();
        // }
    }

    protected virtual void OnFocusChange(Control node){
        // CursorButton button = node as CursorButton;
        // if (button != null)
        //     _selectedButton = button;
    }

    public virtual void Open(){
        Visible = true;
        _canControl = true;
        _openDelayTimer = _delayTime;
        ProcessMode = ProcessModeEnum.Inherit;
        GetViewport().GuiFocusChanged += OnFocusChange;
    }

    public virtual void Close(){
        Visible = false;
        _canControl = false;
        foreach(CursorButton button in _mainButtons)
        {
            button.ProcessMode = ProcessModeEnum.Disabled;
        }
        ProcessMode = ProcessModeEnum.Disabled;
        GetViewport().GuiFocusChanged -= OnFocusChange;
    }

    public virtual void Confirm(){
        if (_selectedButton == null)
            return;
            
        //_selectedButton.EmitSignal("pressed");
    }

    public virtual bool Back()
    {
        return true;
    }


}
