using Godot;
using System;


[GlobalClass]
public partial class InputSystemPreset : Resource
{
    [ExportGroup("Default inputs")]
    [Export]
    public InputEvent _eventActivate;
    [Export]
    public InputEvent _eventDeactivate;

    [ExportGroup("Input event groups")]
    [Export]
    public InputEvent[] _inputsPlayerMovemenet;
    [Export]
    public InputEvent[] _inputsRythmEvent;
    [Export]
    public InputEvent[] _inputsMenu;
    [Export]
    public InputEvent[] _inputsUniversal;
}
