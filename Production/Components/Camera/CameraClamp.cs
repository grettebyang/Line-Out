using Godot;
using System;
[Tool]
public partial class CameraClamp : Node3D
{
    [Export] float _clampAngle;
    [Export] Node3D _left, _right;
    [Export] bool disabled;
    public override void _PhysicsProcess(double delta)
    {
        if (disabled)
            return;
        _left.RotationDegrees = new Vector3(0, _clampAngle /2 - 90, 0);
        DebugDraw3D.DrawLine(_left.GlobalPosition, _left.GlobalPosition + _left.GlobalBasis.X * -5);
        _right.RotationDegrees = new Vector3(0, -_clampAngle /2 - 90, 0);
        DebugDraw3D.DrawLine(_right.GlobalPosition, _right.GlobalPosition + _right.GlobalBasis.X * -5);

    }

















}
