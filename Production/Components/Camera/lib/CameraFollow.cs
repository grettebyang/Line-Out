using Godot;
using System;

// Just a quick script to follow the sled, replace this later
// Also, camera should be instantiated by the sled, rather than sit in the level
public partial class CameraFollow : Node3D {
    [Export] public DogSled _targetFollow;
    private float _heightOffset = 3.5f;
    private float _behindDistance = -5f;

    public override void _Process(double delta) {
        if (_targetFollow == null) return;

        Vector3 targetForward = -_targetFollow.GlobalTransform.Basis.Z.Normalized();
        Vector3 desiredPosition = _targetFollow.GlobalPosition - (targetForward * _behindDistance) + (Vector3.Up * _heightOffset);

        GlobalPosition = desiredPosition;

        Vector3 lookAtPoint = _targetFollow.GlobalPosition + (targetForward * -10);
        LookAt(lookAtPoint, Vector3.Up);
    }
}
