using Godot;
using System;
using System.Security.Cryptography;

// When player is close enough, sled will hover, using the hover mechanic without the effects
public partial class EmeraldRock : RockBase
{
    public override void _Ready()
    {
        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (ds != null)
        {
            if (_isPlayerInDistance)
                ds.Hover();
            else
                ds.HoverEnd();
        }
    }
}
