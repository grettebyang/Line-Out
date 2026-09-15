using Godot;
using System;

public partial class ToggleButton : CursorButton
{
    [Export] protected ColorRect _switchedLeft;
    [Export] protected ColorRect _switchedRight;

    protected bool _toggled;
    
    [Signal] public delegate void ToggledEventHandler(bool toggled);

    protected override void OnPressed(PlayerCurser cursor){
        if(_disabled)
            return;
            
        if(_staySelected)
            _selected = true;
        _toggled = !_toggled;
        _switchedLeft.Visible = !_toggled;
        _switchedRight.Visible = _toggled;
        _audioPlayer.Stream = _soundClick;
        _audioPlayer.Play();
        EmitSignal(nameof(Toggled), _toggled);
    }

    public void SetToggle(bool toggled)
    {
        _toggled = toggled;
        _switchedLeft.Visible = !_toggled;
        _switchedRight.Visible = _toggled;
    }
}
