using Godot;
using System;

public partial class SettingsLeftRightOption : Control
{
    [Export] private CursorButton _leftButton;
    [Export] private CursorButton _rightButton;
    [Export] private Label _optionLabel;
    [Export] public int _optionsCount;
    public int _optionSelected = 0;

    [Signal] public delegate void OnOptionChangedEventHandler(Label optionLabel, int changeDirection);

    public void OnRightPressed(PlayerCurser cursor, int buttonId)
    {
        //_optionSelected = (_optionSelected + 1) % _optionsCount;
        EmitSignal(nameof(OnOptionChanged), _optionLabel, 1);
    }

    public void OnLeftPressed(PlayerCurser cursor, int buttonId)
    {
        //_optionSelected = (_optionSelected + _optionsCount - 1) % _optionsCount;
        EmitSignal(nameof(OnOptionChanged), _optionLabel, -1);
    }

    public void Select(string option)
    {
        _optionLabel.Text = option;
    }
}
