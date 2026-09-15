using Godot;
using System;

[Tool]
[GlobalClass]
public partial class LevelResource : Resource
{
    private string _idName;

    [Export]
    public string IdName
    {
        get => _idName;
        set
        {
            _idName = value;
            this.ResourceName = value;
            EmitChanged(); // Refresh the inspector
        }
    }

    [Export] public PackedScene _level;

    [ExportCategory("Flavor text")]
    [Export] public string DisplayName { get; set; }

    [Export(PropertyHint.MultilineText)]
    public string Decription { get; set; }

    [Export(PropertyHint.MultilineText)]
    public string Goals { get; set; }

    [Export] public float[] TimesToRating { get; set; } // The bigger rating, the less time (e.g. 1 star = 2:00, 2 star = 1:30, etc.)
}
