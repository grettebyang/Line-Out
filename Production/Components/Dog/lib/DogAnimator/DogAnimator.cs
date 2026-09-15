using Godot;
using Godot.Collections;
using System;

public partial class DogAnimator : Node3D
{
    [Export] DogController _player;
    [Export] float _runSpeedTarget = 3;

    [ExportGroup("Animations")]
    [Export] ProcAnimation _idleAnimation;
    [Export] ProcAnimation _moveAnimation;
    [Export] ProcAnimation _gestureTestAnimation; // TODO: Remove once not needed

    [ExportGroup("Legs")]
    [Export] IKTarget _frontLeftIKTarget;
    [Export] IKTarget _frontRightIKTarget;
    [Export] IKTarget _backLeftIKTarget;
    [Export] IKTarget _backRightIKTarget;

    Array<ProcAnimation> _animationQueue = new Array<ProcAnimation>();

    bool _moving = false;
    bool _isRunning = false;

    float _queueTimer = 0;

    public override void _Ready()
    {
        _animationQueue.Add(_idleAnimation);
    }

    public override void _Process(double delta)
    {
        ProcessAnimations((float)delta);

        Vector2 moveInput = _player.MoveInput;
        float runSpeed = _player.Velocity.Length();

        // Flying 
        if (!_player.IsOnFloor())
        {
            _frontLeftIKTarget.ProcessMode = ProcessModeEnum.Disabled;
            _frontRightIKTarget.ProcessMode = ProcessModeEnum.Disabled;
            _backLeftIKTarget.ProcessMode = ProcessModeEnum.Disabled;
            _backRightIKTarget.ProcessMode = ProcessModeEnum.Disabled;
        }
        else
        {
            _frontLeftIKTarget.ProcessMode = ProcessModeEnum.Always;
            _frontRightIKTarget.ProcessMode = ProcessModeEnum.Always;
            _backLeftIKTarget.ProcessMode = ProcessModeEnum.Always;
            _backRightIKTarget.ProcessMode = ProcessModeEnum.Always;
        }

        // Check is moving
        if (moveInput.X == 0 && moveInput.Y == 0)
        {
            // Cancel moving
            if (_moving && !_animationQueue.Contains(_idleAnimation))
            {
                _moving = false;
                //_moveAnimation.CancelAnimation();
                _animationQueue.Add(_idleAnimation);

                _frontLeftIKTarget.RestartPosition();
                _frontRightIKTarget.RestartPosition();
                _backLeftIKTarget.RestartPosition();
                _backRightIKTarget.RestartPosition();
            }
            else
            {
                // Play idle
                //_idleAnimation.ProgressAnimation((float)delta);
            }

            return;
        }
        else
        {
            // Cancel idle 
            if (!_moving && !_animationQueue.Contains(_moveAnimation))
            {
                _moving = true;
                //_idleAnimation.CancelAnimation();
                _animationQueue.Add(_moveAnimation);
            }
        }

        // Update animations
        _moveAnimation.Duration = 1 / runSpeed;
        //_moveAnimation.ProgressAnimation((float)delta);

        // Legs 
        _frontLeftIKTarget._velocity = runSpeed;
        _frontRightIKTarget._velocity = runSpeed;
        _backLeftIKTarget._velocity = runSpeed;
        _backRightIKTarget._velocity = runSpeed;

        // Update walk/run state
        bool runState = _isRunning;

        if (runSpeed < _runSpeedTarget)
        {
            _isRunning = false;
        }
        else
        {
            _isRunning = true;
        }

        // Apply walk/run state
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
    }
    
    void ProcessAnimations(float delta)
    {
        if (_animationQueue.Count == 1)
        {
            // Process single animation
            _animationQueue[0].ProgressAnimation(delta);
        }
        else
        {
            // Switch animation
            if (_queueTimer < _animationQueue[1].TransitionTime)
            {
                // Blend
                //DebugDraw2D.SetText("Blend animation------------------!");
                _animationQueue[1].ProgressAnimation(delta);
                _animationQueue[1].IsLerping = true;
                _queueTimer += delta;
            }
            else
            {
                //DebugDraw2D.SetText("Stop lerping!");
                // Switch
                _animationQueue.RemoveAt(0);
                _animationQueue[0].IsLerping = false;
                _queueTimer = 0;
            }
        }
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

    // TODO: Smoothing of run/walk transition
}
