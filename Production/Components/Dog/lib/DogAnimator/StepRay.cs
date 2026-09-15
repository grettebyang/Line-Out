using Godot;
using System;

public partial class StepRay : RayCast3D
{
    [Export] Node3D _stepTarget;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (IsColliding())
        {
            _stepTarget.GlobalPosition = GetCollisionPoint();
            //DebugDraw3D.DrawSphere(_stepTarget.GlobalPosition, 0.1f, Colors.Red);
        }
    }

}
