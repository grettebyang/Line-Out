using Godot;
using System;

public partial class IKTarget : Marker3D
{
    const float RETURN_SPEED = 50;

    [Export] Node3D _stepTarget;
    [Export] public float _stepLenght = 2;

    [Export] public IKTarget _adjacentLeg;
    [Export] Node3D _originalPosNode;

    public bool _isMoving = false;

    public float _velocity;

    Tween _moveTween;

    bool _legFixed = false;
    bool _stopped = false;

    public override void _Process(double delta)
    {
        Vector3 flatPos = new Vector3(GlobalPosition.X, 0, GlobalPosition.Z);
        Vector3 flatTargPos = new Vector3(_stepTarget.GlobalPosition.X, 0, _stepTarget.GlobalPosition.Z);

        // Adjust step speed
        if (_velocity > 0.1f)
        {
            _moveTween?.SetSpeedScale(_velocity);
            _stopped = false;
        }
        else
        {
            // Returning to default speed
            //_moveTween?.SetSpeedScale(1);
            GlobalPosition = GlobalPosition.Lerp(_originalPosNode.GlobalPosition, (float)delta * RETURN_SPEED);

            if (!_stopped)
            {
                _moveTween?.Kill();
                //GlobalPosition = GlobalPosition.Lerp(_originalPosNode.GlobalPosition, 1);
                _stopped = true;
            }
        }

        if (!_legFixed)
        {
            GlobalPosition = _originalPosNode.GlobalPosition;
            _legFixed = true;
        }

        if (flatPos.DistanceTo(flatTargPos) > _stepLenght && _velocity != 0 && !_isMoving && !_adjacentLeg._isMoving)
        {
            Step();
        }
        
        /*
        DebugDraw3D.DrawSphere(_originalPosNode.GlobalPosition, 0.1f, Colors.Green);
        DebugDraw3D.DrawSphere(_stepTarget.GlobalPosition, 0.1f, Colors.Red);
        DebugDraw3D.DrawSphere(GlobalPosition, 0.1f, Colors.Blue);
        */
    }

    public void Step()
    {
        _isMoving = true;
        Vector3 targetPos = _stepTarget.GlobalPosition;
        Vector3 halfWay = (GlobalPosition + targetPos) / 2;

        //DebugDraw3D.DrawSphere(halfWay, 0.1f, Colors.Green);
        //DebugDraw3D.DrawSphere(GlobalPosition, 0.1f, Colors.Blue);
        //DebugDraw3D.DrawSphere(_stepTarget.GlobalPosition, 0.1f, Colors.Red);

        _moveTween = GetTree().CreateTween();
        float time = _stepLenght * 0.125f;
 
        _moveTween.TweenProperty(this, "global_position", halfWay, time);
        _moveTween.TweenProperty(this, "global_position", targetPos, time);
        _moveTween.TweenCallback(Callable.From(OnStepDone)).SetDelay(time * 2);
    }

    void OnStepDone()
    {
        _isMoving = false;
    }

    public void RestartPosition()
    {
        //_moveTween.Dispose();
        _isMoving = false;
        GlobalPosition = _originalPosNode.GlobalPosition;
    }
}
