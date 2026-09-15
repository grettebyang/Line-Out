using Godot;
using System;
using System.Numerics;

public partial class Clickable : CursorButton
{
    private float sine = 0.0f;
    private float mult = 0.0f;
    [Export] private float _bounceSpeed = 25f;
    [Export] private float _taperSpeed = .3f;
    [Export] private float _scaleAmount = .2f;


    protected override void OnFocus(Node2D body){
        if(_disabled)
            return;
            
        if(body.GetType() == typeof(PlayerCurser)){
            SetFocused((body as PlayerCurser));
            EmitSignal(nameof(Hovered), (body as PlayerCurser), _buttonId);
        }
    }

    public override void SetFocused(PlayerCurser cursor)
    {
        if(_disabled)
            return;
        
        _cursorsHovering++;
        cursor.ButtonPressed += OnPressed;
    }

    protected override void ExitFocus(Node2D body){
        if(_disabled)
            return;

        if(body.GetType() == typeof(PlayerCurser))
        {
            (body as PlayerCurser).ButtonPressed -= OnPressed;
            _cursorsHovering--;

            if(!_selected && _cursorsHovering == 0)
            {
                EmitSignal(nameof(UnHovered), (body as PlayerCurser), _buttonId);
            }
        }
    }

    protected void ClickedOn(PlayerCurser cursor, int buttonId)
    {
        mult = _scaleAmount;
        sine = 0.0f;
        Scale = Godot.Vector2.One + Godot.Vector2.One * mult * (float)Math.Cos(sine);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if(mult > 0.0f)
        {
            sine = sine + _bounceSpeed * (float)delta;
            mult = Math.Max(mult - (float)delta * _taperSpeed, 0.0f);
            Scale = Godot.Vector2.One + Godot.Vector2.One * mult * (float)Math.Cos(sine);
        }
        
    }

}
