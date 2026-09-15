using Godot;
using System;
using Godot.Collections;

[Tool]
public partial class SpeedWind : Node3D
{
    [Export] float _power = 50;
    [Export] Area3D area;
    Timer _exitTimer;
    [Export] private float _exitTime = 3f;
    Array<Node3D> _affectedObjects = new Array<Node3D>();

    public override void _Ready()
    {
        area.BodyEntered += OnEnter;
        area.BodyExited += OnExit;

        _exitTimer.Timeout += ExitTimerFinished;
    }

    public override void _Process(double delta)
    {
        // Render
        DebugDraw3D.DrawBox(GlobalPosition, GlobalBasis.GetRotationQuaternion(), new Vector3(2, 2, 2), Colors.SkyBlue);
        DebugDraw3D.DrawArrow(GlobalPosition, GlobalPosition + GlobalBasis.Z * 5, Colors.SkyBlue, 0.5f, true);
    }


    public override void _PhysicsProcess(double delta)
    {
        // Affect
        foreach (Node3D body in _affectedObjects)
        {
            if (body is DogController dog)
            {
                dog.Velocity += Basis.Z * _power * (float)delta * 5000; // Velocity is not really working
                dog.GlobalPosition += GlobalBasis.Z * _power * (float)delta * 0.2f;
            }

            if (body is DogSled sled)
            {
                sled.LinearVelocity += GlobalBasis.Z * _power * (float)delta;
            }

            DebugDraw3D.DrawSphere(body.GlobalPosition, 0.5f, Colors.Red);
        }
    }

    void OnEnter(Node3D body)
    {
        _exitTimer.Start();

        if (body is DogController dogCollider)
        {
            _affectedObjects.Add(body);
        }

        if (body is DogSled sled)
        {
            _affectedObjects.Add(sled);
        }
    }

    void OnExit(Node3D body)
    {
        _exitTimer.Start();
    }

    void ExitTimerFinished()
    {
        foreach (Node3D body in _affectedObjects)
        {
            if(_exitTimer.IsStopped() && _affectedObjects.Count > 0)
            {
                if (_affectedObjects.Contains(body))
                {
                    _affectedObjects.Remove(body);
                }
            }
        }
    }
}
