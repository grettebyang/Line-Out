using Godot;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public partial class SliderButton : CursorButton
{
    [Export] TextureRect _slider;
    [Export] ColorRect _valueBar;
    [Export] ColorRect _bar;
    [Export] ColorRect _hoverBar;
    [Export] public float MinValue;
    [Export] public float MaxValue;
    PlayerCurser _controllingCursor;
    [Export] private float Value;

    [Signal] public delegate void OnValueChangedEventHandler(float value);

    public override void _Ready()
    {
        base._Ready();
        _valueBar.Size = new Vector2((Value - MinValue) * _bar.Size.X / (MaxValue - MinValue), _valueBar.Size.Y);
    }

    public override void _Process(double delta)
    {
        var prevVal = Value;
        if(_controllingCursor != null)
        {
            float length = float.Clamp(_controllingCursor.GlobalPosition.X - _valueBar.GlobalPosition.X, 0.0f, _bar.Size.X);
            _valueBar.Size = new Vector2(length, _valueBar.Size.Y);
            Value = length * (MaxValue - MinValue) / _bar.Size.X + MinValue;
            if(prevVal != Value)
            {
                GD.Print("New Value: " + Value);
                EmitSignal(nameof(OnValueChanged), Value);
            }
        }
    }

    public void SetValue(float value)
    {
        Value = value;
        _valueBar.Size = new Vector2((Value - MinValue) * _bar.Size.X / (MaxValue - MinValue), _valueBar.Size.Y);
    }


    public override void ResizeCollisionShape()
    {
        RectangleShape2D shape = new RectangleShape2D();
        shape.Size = new Vector2(Size.X, _slider.Size.Y);
        _collisionShape.Shape = shape;
        _collisionShape.GlobalPosition = GlobalPosition + Size/2;
        GD.Print(shape.Size);        
    }

    protected override void OnPressed(PlayerCurser cursor)
    {
        if(_disabled || _controllingCursor != null) // only one cursor can move slider at a time
            return;
            
        if(_staySelected)
            _selected = true;

        cursor.ButtonReleased += OnReleased;
        
        _controllingCursor = cursor;
        _audioPlayer.Stream = _soundClick;
        _audioPlayer.Play();
        EmitSignal(nameof(Pressed), cursor, _buttonId);        
    }

    protected virtual void OnReleased(PlayerCurser cursor)
    {
        _controllingCursor = null;
        cursor.ButtonReleased -= OnReleased;
        if(_cursorsHovering == 0)
        {
            UnHover();
        }
    }

    public override void Hover()
    {
        _hoverBar.Visible = true;  
        base.Hover(); 
    }

    public override void UnHover()
    {
        if(_controllingCursor != null)
            return;
        
        _hoverBar.Visible = false;
        base.UnHover();
    }



}
