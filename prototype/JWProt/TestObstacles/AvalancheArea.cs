using Godot;
using Godot.Collections;
using System;

public partial class AvalancheArea : Node
{
    [Export] float _trapDelay;
    [Export] GpuParticles3D _anticipationFx;
    [Export] Area3D _trigger;
    [Export] Area3D _trapArea;
    [Export] Node3D _snow;

    Array<DogController> _trappedDogs = new Array<DogController>();
    float _trapTimer;
    bool _triggered = false;
    bool _done = false;

    Camera _sledCam;

    public override void _Ready()
    {
        _sledCam = GameManager.GetInstance().CurrentLevel._sled._camera;
        _anticipationFx.Emitting = false;
        _snow.Visible = false;

        _trigger.BodyEntered += OnTriggerEnter;
        _trapArea.BodyEntered += OnTrapAreaEnter;
        _trapArea.BodyExited += OnTrapAreaExit;
    }

    public override void _Process(double delta)
    {
        if (_done)
            return;

        // Triggered
        if (_triggered)
        {
            _sledCam.ActivateCamShake(20);

            if (_trapTimer > 0)
            {
                _trapTimer -= (float)delta;
            }
            else
            {
                // Trap dogs
                _snow.Visible = true;
                _done = true;
                _anticipationFx.Emitting = false;

                foreach (DogController dog in _trappedDogs)
                {
                    dog.MaxSpeed = 0.1f;
                }

                // Clearup
                _trappedDogs.Clear();
                _trigger.BodyEntered -= OnTriggerEnter;
                _trapArea.BodyEntered -= OnTrapAreaEnter;
                _trapArea.BodyExited -= OnTrapAreaExit;
            }
        }
    }


    void OnTriggerEnter(Node body)
    {
        if (body is DogController dog)
        {
            Trigger();
        }

        if (body is DogSled sled)
        {
            Trigger();
        }
    }

    public void Trigger()
    {
        _triggered = true;
        _trapTimer = _trapDelay;
        _anticipationFx.Emitting = true;
    }

    void OnTrapAreaEnter(Node body)
    {
        if (body is DogController dog)
        {
            _trappedDogs.Add(dog);
        }
    }

    void OnTrapAreaExit(Node body)
    {
        if (body is DogController dog)
        {
            _trappedDogs.Remove(dog);
        }
    }
}
