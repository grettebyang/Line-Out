using Godot;
using System;

public partial class TestDogAnimator : Node3D
{
    [Export] CharacterBody3D _player;
    [Export] Node3D _bodyIKTarget;
    [Export] Node3D _tailIKTarget;
    [Export] float _runSpeedTarget = 3;
    [Export] Curve _bodyRotateCurve;

    float runAnimTime = 1;
    float _rotateTime;

    [ExportGroup("Legs")]
    [Export] IKTarget _frontLeftIKTarget;
    [Export] IKTarget _frontRightIKTarget;
    [Export] IKTarget _backLeftIKTarget;
    [Export] IKTarget _backRightIKTarget;

    float moveTime = 0;

    Vector3 _bodyStartPos;
    float _bodyY = 0;

    Vector3 _tailStartPos;
    float _tailX = 0;

    bool _isRunning = false;

    public override void _Ready()
    {
        _bodyStartPos = _bodyIKTarget.Position;
        _tailStartPos = _tailIKTarget.Position;
    }


    public override void _Process(double delta)
    {
        // Handle run animation
        moveTime += (float)delta;

        if (moveTime > runAnimTime)
        {
            moveTime = 0;
        }

        float runSpeed = _player.Velocity.Length();

        // Tail
        float tailSpeed = runSpeed;
        if (tailSpeed == 0)
            tailSpeed = 1;

        _tailX += (float)delta * 2 * tailSpeed;
        if (_tailX > Mathf.Pi * 2)
        {
            _tailX = _tailX - (Mathf.Pi * 2);
        }

        Vector3 swing = _tailStartPos + _tailIKTarget.Basis.X * (float)Math.Sin(_tailX) * 4f;
        _tailIKTarget.Position = swing;

        // Check is moving
        if (runSpeed == 0)
        {
            moveTime = 0;

            // Head and tail return
            _bodyIKTarget.Position = _bodyIKTarget.Position.Lerp(_bodyStartPos, (float)delta * 10);
            _tailIKTarget.Position = _tailIKTarget.Position.Lerp(_tailStartPos, (float)delta * 10);

            return;
        }

        // Legs 
        _frontLeftIKTarget._velocity = runSpeed;
        _frontRightIKTarget._velocity = runSpeed;
        _backLeftIKTarget._velocity = runSpeed;
        _backRightIKTarget._velocity = runSpeed;

        //SetupRun();

        // Setups
        bool runState = _isRunning;

        if (runSpeed < _runSpeedTarget)
        {
            _isRunning = false;
        }
        else
        {
            _isRunning = true;
        }

        if (runState != _isRunning)
        {
            // Reset legs
            _frontLeftIKTarget.RestartPosition();
            _frontRightIKTarget.RestartPosition();

            _backLeftIKTarget.RestartPosition();
            _backRightIKTarget.RestartPosition();

            if (runSpeed < _runSpeedTarget)
                SetupWalk();
            else
                SetupRun();
        }

        // Body
        _bodyY += (float)delta * 4 * runSpeed;
        if (_bodyY > Mathf.Pi * 2)
        {
            _bodyY = _bodyY - (Mathf.Pi * 2);
        }

        Vector3 bob = _bodyStartPos + _bodyIKTarget.Basis.Y * (float)Math.Sin(_bodyY) * 0.25f;
        _bodyIKTarget.Position = new Vector3(_bodyIKTarget.Position.X, bob.Y, _bodyIKTarget.Position.Z);

        // Rotate forward
        float rotatePower = 1;
        if (!_isRunning)
            rotatePower = 0.25f;

        _rotateTime += (float)delta * runSpeed;
        if (_rotateTime > _bodyRotateCurve.MaxDomain)
        {
            _rotateTime = _rotateTime - _bodyRotateCurve.MaxDomain;
        }

        Rotation = new Vector3(_bodyRotateCurve.Sample(_rotateTime) * rotatePower,Rotation.Y, Rotation.Z);
    }

    void SetupWalk()
    {
        // Side
        _frontLeftIKTarget._adjacentLeg = _frontRightIKTarget;
        _frontRightIKTarget._adjacentLeg = _frontLeftIKTarget;

        _backLeftIKTarget._adjacentLeg = _backRightIKTarget;
        _backRightIKTarget._adjacentLeg = _backLeftIKTarget;
    }

    void SetupRun()
    {
        // In line
        _frontLeftIKTarget._adjacentLeg = _backLeftIKTarget;
        _backLeftIKTarget._adjacentLeg = _frontLeftIKTarget;

        _frontRightIKTarget._adjacentLeg = _backRightIKTarget;
        _backRightIKTarget._adjacentLeg = _frontRightIKTarget;
    }
}
