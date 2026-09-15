using Godot;
using System;
using Godot.Collections;
using System.Collections.Generic;

public partial class RollingSnowball : RigidbodyObstacle
{
    [Export] float _trapTimer = 10;
    [Export] Vector3 _direction;
    [Export] float _power;
    [Export] AudioStreamPlayer3D _rollingSound;
    [Export] AudioStreamPlayer3D _hitSound;

    // How long to wait before snowball starts rolling (good way to make snowball start rolling behind instead of infront of sled)
    [Export] private float _activationTimer = 2f;
    // Area 3d is just buggy to work with so we use this instead
    [Export] private float _activationRange = 5f;
    private bool _isRolling = false;
    private bool _sledCaught = false;
    private Array<Node3D> _trapped = new Array<Node3D>();
    private List<Vector3> _impactOffset = new List<Vector3>();
    private Vector3 _camOffset;
    DogSled sled;
    private BasicTimeManager _timeManager;

    // reduce this value over time to make snowball smaller
    float timeBeforePoof = 30.0f;
    // For pausing
    private bool _paused = false;
    private Vector3 _savedLinVel = Vector3.Zero;
    private Vector3 _savedAngularVel = Vector3.Zero;

    public override void _Ready()
    {
        base._Ready();
        Freeze = true;
        _direction = _direction.Normalized();
        this.BodyEntered += OnBodyEntered;
        _timeManager = BasicTimeManager.GetInstance();
    }

    public override void _Process(double delta)
    {
        checkIfSledInRange((float)delta);
        if (Engine.IsEditorHint())
        {
            DebugDraw3D.DrawArrow(GlobalPosition, GlobalPosition + _direction * 5, Colors.Red, 0.25f, true);
        }
    }

    // Signal
    private void checkIfSledInRange(float delta)
    {
        _activationTimer -= delta;

        if (sled == null)
        {
            sled = GameManager.GetInstance().CurrentLevel._sled;
            if (sled == null) 
                return;
        }

        if(!_isRolling && (GlobalPosition - sled.GlobalPosition).Length() < _activationRange * 10 && _activationTimer <= 0)
        {
            Freeze = false;
            _direction = sled.GlobalPosition - GlobalPosition;
            _isRolling = true;
            ApplyForce(_direction * _power);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        delta = delta * _timeManager.GameSpeed;
        base._PhysicsProcess(delta);

        bool inMenu = _timeManager.State == BasicTimeManager.TIMESTATE.MENU;
        if(inMenu && _paused == false)
        {
            _paused = true;
            Freeze = true;
            _savedLinVel = LinearVelocity;
            _savedAngularVel = AngularVelocity;
        }
        else if(!inMenu && _paused == true)
        {
            _paused = false;
            Freeze = false;
            LinearVelocity = _savedLinVel;
            AngularVelocity = _savedAngularVel;
        }

        OverrideCamera(delta);
        if(_isRolling)
        {
            timeBeforePoof -= (float)delta;

            if (timeBeforePoof <= 0f && _trapped.Count == 0) 
                QueueFree();

            if(LinearVelocity.Length() > 0)
            {
                if(!_rollingSound.IsPlaying())
                    _rollingSound.Play();
            }
            else
            {
                _rollingSound.Stop();
            }

            // Trapped
            for(int i = 0; i < _trapped.Count; i++)
            {
                _trapped[i].GlobalPosition = GlobalPosition + GlobalBasis * _impactOffset[i] * 2;
                _trapped[i].GlobalRotation = this.GlobalRotation;
                
            }

            if (_trapped.Count > 0)
            {
                _trapTimer -= (float)delta;
                if (_trapTimer <= 0)
                {
                    _sledCaught = false;
                    foreach(Node3D body in _trapped)
                    {
                        body.RemoveMeta("snowball_trapped");
                        body.ProcessMode = ProcessModeEnum.Always;
                    }

                    _impactOffset.Clear();
                    _trapped.Clear();
                    QueueFree();
                }
            }
        }
    }

    // Todo: Refactor duplication
    void OnBodyEntered(Node body)
    {
        if(LinearVelocity.Length() > 3 && body is Node3D node && !_trapped.Contains(node))
        {
            if (node.HasMeta("snowball_trapped"))
                return;
            
            if (body is DogSled sled)
            {
                node.SetMeta("snowball_trapped", true);
                _sledCaught = true;
                DogSled dogSled = GameManager.GetInstance().CurrentLevel._sled;
                _trapped.Add(dogSled);
                _impactOffset.Add(GlobalBasis.Inverse() * GlobalPosition.DirectionTo((body as Node3D).GlobalPosition));
                _hitSound.Play();
            }
            if (body is DogController dog)
            {
                node.SetMeta("snowball_trapped", true);
                _trapped.Add(dog);
                _impactOffset.Add(GlobalBasis.Inverse() * GlobalPosition.DirectionTo((body as Node3D).GlobalPosition));
                _hitSound.Play();
                body.ProcessMode = ProcessModeEnum.Disabled;
            }

        }
    }

    // If sled is disabled we just want the camera to Lookat snowball
    private void OverrideCamera(double delta)
    {
        if (_sledCaught)
        {
            Camera3D cam = GameManager.GetInstance().CurrentLevel._sled._camera._sledCamera;
            _camOffset = (cam.GlobalPosition - GlobalPosition).Normalized() * 12f + Vector3.Up * 4f;

            cam.TopLevel = true;
            Vector3 target = GlobalPosition + _camOffset;
            cam.GlobalPosition = cam.GlobalPosition.Lerp(target, (float)delta * 2f);
            cam.LookAt(GlobalPosition);
        }
    }
}
