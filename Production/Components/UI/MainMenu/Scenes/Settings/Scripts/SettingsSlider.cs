using Godot;
using System;

public partial class SettingsSlider : HSlider
{
    [Export] float valueChangePerc = 0.1f;
    [Export] Button parentButton;
    float valueChange;

    public override void _Ready()
    {
        base._Ready();
        valueChange = valueChangePerc * (float)(MaxValue - MinValue);
    }


    public override void _Input(InputEvent @event){
        if (!parentButton.HasFocus())
            return;

        if (@event.IsActionPressed("MenuRight"))
        {
            Value += valueChange;
        }
        
        if (@event.IsActionPressed("MenuLeft"))
        {
            Value -= valueChange;
        }
    }
}
