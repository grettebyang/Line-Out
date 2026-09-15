using Godot;
using System;

public partial class ConfusionHalo : Node3D
{
    private Vector3 _rotationVec = new Vector3(0.0f, 1.0f, 0.0f);
    public override void _Process(double delta)
    {
        Rotation += _rotationVec * 3f * (float)delta;
    }
}
