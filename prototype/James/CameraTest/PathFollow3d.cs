using Godot;
using System;
[Tool]
public partial class PathFollow3d : PathFollow3D
{
    public override void _Process(double delta)
    {
        Progress += 0.1f;
    }



}
