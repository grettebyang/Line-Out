using Godot;
using System;

public partial class MudPuddle : Node3D
{
    [Export] Area3D _area;
    [Export] AudioStreamPlayer _sfx;
    [Export] AudioStream _mudRunningSound;
    [Export] public float _targetSpeed = 2.5f;
    [Export] private float _respawnTimer = 30;
    DogSled _sled;
    float _originalSpeed = -1;
	BasicTimeManager _timeManager;
    private float _currentTimer;

    DogController _slowedDog;
    public override void _Ready()
    {
		_timeManager = BasicTimeManager.GetInstance();
        _sled = GameManager.GetInstance().CurrentLevel._sled;
        _area.BodyEntered += OnEnter;
        _area.BodyExited += OnLeave;
    }

    public override void _PhysicsProcess(double delta)
    {
        delta *= _timeManager.GameSpeed;

        if(_currentTimer > 0)
        {
            _currentTimer -= (float)delta;
            Visible = false;
        }
        else
            Visible = true;
    }

    void OnEnter(Node body)
    {
        if(_currentTimer > 0)
            return;
        // Dog 
        if (body is DogController dog)
        {
            if(dog._invincible)
                return;
            
            if (_originalSpeed == -1)
            {
                _originalSpeed = dog.MaxSpeed;
            }

            _slowedDog = dog;
            _currentTimer = _respawnTimer;

            dog.MaxSpeed = _targetSpeed;
            dog._runningAudio.Stream = _mudRunningSound;

            _sfx.Play();
            _sled.EmitSignal(nameof(_sled.OnMudHit));
        }

    }

    void OnLeave(Node body)
    {
        if (body is DogController dog && body == _slowedDog)
        {
            dog.MaxSpeed = dog.BaseMaxSpeed;
            dog._runningAudio.Stream = dog._runningSound;
        }
    }
}
