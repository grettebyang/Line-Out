using Godot;
using System;
using Godot.Collections;

public partial class BlockerObstacle : Node
{
    [Export] float _forceMutliplier = 1;
    [Export] float _forceCap = 10;
    [Export] Area3D _area;
    Array<RigidBody3D> _blocks = new Array<RigidBody3D>();

    public override void _Ready()
    {
        foreach (Node node in GetChildren())
        {
            if (node is RigidBody3D rb)
                _blocks.Add(rb);
        }

        _area.BodyEntered += OnEnter;
    }

    void OnEnter(Node body)
    {
        if (body is DogController dog)
        {
            Vector3 dir = new Vector3(0, 0, 0);

            foreach (RigidBody3D rb in _blocks)
            {
                dir = dog.GlobalPosition - rb.GlobalPosition;
                float vel = Mathf.Clamp(dog.Velocity.Length(), 1, _forceCap);
                rb.LinearVelocity -= dir * vel * _forceMutliplier;
            }

            SlowDown();

            _area.BodyEntered -= OnEnter;
        }

        if (body is DogSled sled)
        {
            Vector3 dir = new Vector3(0, 0, 0);

            foreach (RigidBody3D rb in _blocks)
            {
                dir = sled.GlobalPosition - rb.GlobalPosition;
                float vel = Mathf.Clamp(sled.LinearVelocity.Length(), 1, _forceCap);
                rb.LinearVelocity -= dir * vel * _forceMutliplier;
            }

            SlowDown();

            _area.BodyEntered -= OnEnter;
        }
    }

    void SlowDown()
    {
        DogSled sled = GameManager.GetInstance().CurrentLevel._sled;

        // Dogs
        foreach (DogController dog in sled.DogSpawner.DogList)
        {
            dog.Velocity *= 0.5f;
        }

        sled.LinearVelocity *= 0.5f;
    }
}
