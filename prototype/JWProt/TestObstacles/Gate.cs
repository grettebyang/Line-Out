using Godot;
using System;

public partial class Gate : Node3D
{
    [Export] Area3D _barkArea;
    [Export] float _minTarget = 50;
    [Export] float _failOpenDelay = 2;
    [Export] MovingMechanism _door;
    [Export] Area3D _doorArea;

    RhythmManager _rhythmManager;
    bool barkEvent = false;
    bool _barkEventDone = false;
    bool _failed = false;

    DogSled _sled;

    public override void _Ready()
    {
        _sled = GameManager.GetInstance().CurrentLevel._sled;

        _barkArea.BodyEntered += OnEnter;
        _doorArea.BodyEntered += OnDoorEnter;
        _rhythmManager = RhythmManager.GetInstance();
    }

    public override void _Process(double delta)
    {
        if (barkEvent)
        {
            if (!_rhythmManager._sequenceGo)
            {
                GD.Print("_rhythmManager._accuracy: " + _rhythmManager._accuracy);

                // Slow opening if fail
                if (_rhythmManager._accuracy < _minTarget)
                {
                    _failed = true;
                }
                else
                {
                    _door.isPlaying = true;
                    //_doorArea.BodyEntered -= OnDoorEnter;
                }

                _barkArea.Monitorable = false;
                _barkArea.Monitoring = false;

                barkEvent = false;
                _barkEventDone = true;
            }
        }

        if (_barkEventDone)
        {
            if (_failed)
            {
                if (_failOpenDelay > 0)
                {
                    _failOpenDelay -= (float)delta;
                }
                else if (_failOpenDelay <= 0 && !_door.isPlaying)
                {
                    _door.isPlaying = true;
                    _doorArea.BodyEntered -= OnDoorEnter;
                    _barkEventDone = false;
                }
            }
        }

        if (_door.isPlaying)
        {
            _sled._camera.ActivateCamShake(10);
        }
    }


    void OnEnter(Node body)
    {
        if (body is DogController dog)
        {
            _rhythmManager.StartRhythmEvent();
            barkEvent = true;

            _barkArea.BodyEntered -= OnEnter;
        }
    }

    void OnDoorEnter(Node body)
    {
        if (body is DogController dog)
        {
            Vector3 force = dog.GlobalPosition - this.GlobalPosition;
            force = force.Normalized() * 20;
            GD.Print("Apply force: " + force);
            dog.ApplyExternalForce(force);
        }
    }
}
