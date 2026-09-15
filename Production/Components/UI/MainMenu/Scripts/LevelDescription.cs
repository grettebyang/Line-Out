using Godot;
using System;

[Tool]
public partial class LevelDescription : Control
{
    [Export] private string _name;

    [Export(PropertyHint.MultilineText)]
    private string _decription;

    [Export(PropertyHint.MultilineText)]
    private string _goals;

    [Export(PropertyHint.MultilineText)]
    private string _packageName = "Saft och Bullar";

    [Export(PropertyHint.MultilineText)]
    private string _flavorDescription = "Konkuransen är hård i kalla Lappland, se till så att Herr Svensson får sitt fika innan solens nedgång!";

    [ExportGroup("References")]
    [Export] private Label _lblName;
    [Export] private Label _lblDescription;
    [Export] private Label _lblGoals;
    [Export] private Label _lblPackageName;
    [Export] private Label _lblFlavorDescription;

    public override void _Ready()
    {
        _lblName.Text = _name;
        _lblDescription.Text = _decription;
        _lblGoals.Text = _goals;
        _lblPackageName.Text = _packageName;
        _lblFlavorDescription.Text = _flavorDescription;
    }


    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
        {
            if (_lblName == null || _lblDescription == null || _lblGoals == null || _lblFlavorDescription == null)
                return;

            _lblName.Text = _name;
            _lblDescription.Text = _decription;
            _lblGoals.Text = _goals;
            _lblFlavorDescription.Text = _flavorDescription;
        }
    }

}
