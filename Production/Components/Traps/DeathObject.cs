using Godot;
using System;

public partial class DeathObject : StaticBody3D
{
    [Export]
    public AudioStreamPlayer _deathSFX;
    public override void _Ready() {
        base._Ready();
    }
}
