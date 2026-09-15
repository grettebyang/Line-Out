using Godot;
using Godot.Collections;
using System;

public partial class BreakableObstacle : Node3D
{
    [Export] public float _breakSpeed;
    [Export] public float _breakSpeedCountAdjust; // How much must be speed higher per extra dogs

    [Export] public float _targetSpeed = 2.5f;
    [Export] public float _slowDownTime = 2;
    [Export] float _disposeTime = 3;
    [Export] Area3D _area;
    [Export] Node3D _model;

    bool _hit = false;
    float _originalSpeed;
    float _slowDownTimeCounter;

    DogSled sled;

    public override void _Ready()
    {
        sled = GameManager.GetInstance().CurrentLevel._sled;

        GameManager.GetInstance().CurrentLevel.OnStart += Setup;
        _area.BodyEntered += OnEnter;
    }

    public override void _Process(double delta)
    {
        // Activate slowdown
        if (_slowDownTimeCounter > 0)
        {
            _slowDownTimeCounter -= (float)delta;

            if (_slowDownTimeCounter <= 0)
            {
                // Cancel slowdown
                DogSled sled = GameManager.GetInstance().CurrentLevel._sled;

                foreach (DogController dog in sled.DogSpawner.DogList)
                {
                    dog.MaxSpeed = _originalSpeed;
                }
            }
        }

        // Dispose object
        if (_hit)
        {
            _disposeTime -= (float)delta;

            if (_disposeTime <= 0)
            {
                this.QueueFree();
            }
        }
    }

    void Setup()
    {
        // Adjust break speed
        int dogsCount = GameManager.GetInstance().CurrentLevel._sled.DogSpawner.DogList.Count;
        for (int i = 1; i < dogsCount; i++)
        {
            _breakSpeed += _breakSpeedCountAdjust;
        }

        DebugDraw2D.SetText("_breakSpeed: " + _breakSpeed, null, 0, Colors.Yellow, 1);
        GameManager.GetInstance().CurrentLevel.OnStart -= Setup;
    }

    bool _hitBySled = false;

    void OnEnter(Node body)
    {
        // Dog 
        if (body is DogController dog)
        {
            SlowDown();
        }

        // Sled 
        if (body is DogSled sled)
        {
            _hitBySled = true;
            SlowDown();
        }
    }

    void SlowDown()
    {
        _hit = true;
        _area.BodyEntered -= OnEnter;

        // Pick fastest object in direction (dogs only now)
        float fastestSpeed = 0;
        Vector3 recognizeDir = new Vector3(0, 0, 0);

        if (_hitBySled)
        {
            fastestSpeed = sled.LinearVelocity.Length();
            recognizeDir = sled.GlobalBasis.Z;
        }
        else
        {
            foreach (DogController dog in sled.DogSpawner.DogList)
            {
                Vector3 dogDir = -dog.GlobalBasis.Z;
                Vector3 diff = GlobalPosition - dog.GlobalPosition;
                float speed = dogDir.Dot(diff) * dog.Velocity.Length();

                if (speed > fastestSpeed)
                {
                    fastestSpeed = speed;
                    recognizeDir = diff.Normalized();
                }
            }
        }

        // Ignore slowdown?
            if (fastestSpeed >= _breakSpeed)
            {
                DebugDraw2D.SetText("Broken fastestSpeed: " + fastestSpeed, null, 0, Colors.Green, 1);
                Break(recognizeDir, 25);
                sled._camera.ActivateCamShake(20);
                return;
            }

        // Apply slowdown
        foreach (DogController dog in sled.DogSpawner.DogList)
        {
            _originalSpeed = dog.MaxSpeed;
            dog.MaxSpeed = _targetSpeed;
        }

        _slowDownTimeCounter = _slowDownTime;
        Break(recognizeDir, 8);
        sled._camera.ActivateCamShake(10);

        DebugDraw2D.SetText("Failed fastestSpeed: " + fastestSpeed, null, 0, Colors.OrangeRed, 1);
    }

    void Break(Vector3 dir, float force)
    {
        foreach (RigidBody3D rb in _model.GetChildren())
        {
            rb.Freeze = false;
            rb.ApplyImpulse(dir * force);
        }
    }
}
