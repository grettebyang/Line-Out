using Godot;
using System;
using System.Collections.Generic;


// Usage: Inherit and instantiate this Ropenode in a class such as dog or sled
// Connect the two endpoints which are made up of two nodes: _endPoint1 and _endPoint2 which will connect the rope

// Usage: Inherit and instantiate this Ropenode in a class such as dog or sled
// Connect the two endpoints which are made up of two nodes: EndPointA and EndPointB which will connect the rope
public partial class RopeNode : Node3D
{
    // I didn't feel like using exports method via sled doing this
    [Export] public float MaxRopeLength = 4f;
    public int NumOfRopeSegments = 75;
    [Export] public int RopeSegmentsToIgnore = 0;
    [Export] public float RopeKnotRadius = 0.12f;
    public Node3D EndPointA;
    public Node3D EndPointB;
    public readonly List<RigidBody3D> RopeSegmentList = new();
    PackedScene _ropeSegmentScene = GD.Load<PackedScene>("res://Production//Components//Rope//lib//RopeSegment.tscn");
    PackedScene _ropeKnotScene = GD.Load<PackedScene>("res://Production/Components/Rope/lib/RopeKnot.tscn");

    // Mainrope specific variables:
    float _t = 0;
    float _stiffness = 1f;
    float _maxForce = 250f;
    float _currentRopeLength;
    float _ropeSegmentGravFactor = 0;
    float _ropeHangFactor = 0;
    float _distanceErrorFactor = 0;
    float _pullStrength = 0;
    float _basePull;
    float _distanceToTarget;
    float _tension;
    float _steps;
    Vector3 _baselinePos;
    Vector3 _targetPosition;
    Vector3 _moveDirection;
    Vector3 _targetVelocity;
    Vector3 _velocityError;
    Vector3 _springForce;
    Vector3 _dampingForce;
    Vector3 _totalForce;
    Vector3 _toTarget;
    Vector3 _ropeDirection;
    Vector3 _avgDirection;

    public void instantiateRope(Node3D _startPoint, Node3D _endPoint)
    {
        EndPointA = _startPoint;
        EndPointB = _endPoint;

        for (int i = 0; i < NumOfRopeSegments; i++)
        {
            RigidBody3D _ropeSegment = _ropeSegmentScene.Instantiate<RigidBody3D>();
            _ropeSegment.Scale = new Vector3(3.0f, 3.0f, 3.0f);
            AddChild(_ropeSegment);
            RopeSegmentList.Add(_ropeSegment);
        }
    }

    public Vector3 getRopeAverageDirection(int _sledSteeringReponsivenessFactor = 16)
    {
        _avgDirection = Vector3.Zero;
        // Note: if i starts at 0, it will use the first rope segments as the direction which ends up going everywhere
        for (int i = 10; i < NumOfRopeSegments / _sledSteeringReponsivenessFactor; i++)
        {
            if (i > NumOfRopeSegments)
                break;
            _avgDirection += RopeSegmentList[i].GlobalPosition - RopeSegmentList[i + 5].GlobalPosition;
            _avgDirection = _avgDirection.Normalized();
        }
        return _avgDirection;
    }

    private void maintainRope(double delta)
    {
        _distanceToTarget = EndPointA.GlobalPosition.DistanceTo(EndPointB.GlobalPosition);
        _tension = Mathf.Clamp((_distanceToTarget - Mathf.Max(MaxRopeLength, 4f) * 0.8f) / (Mathf.Max(MaxRopeLength, 4f) * 0.8f), 0.3f, 1f);
        _steps = _distanceToTarget / NumOfRopeSegments;
        _distanceToTarget = 0;

        _ropeDirection = (EndPointB.GlobalPosition - EndPointA.GlobalPosition).Normalized();
        _basePull = 12f + _tension * 2f;

        // Execute segments call
        for (int i = 0; i < NumOfRopeSegments; i++)
        {
            _t = (float)i / NumOfRopeSegments;

            _ropeSegmentGravFactor = 2f * Mathf.Pow(_t - 0.5f, 2) * (1f - _tension);
            _ropeHangFactor = Mathf.Cosh(_ropeSegmentGravFactor * 1.3f);

            _baselinePos = EndPointA.GlobalPosition + _ropeDirection * (_steps * i);

            _targetPosition = new Vector3(
                _baselinePos.X,
                _baselinePos.Y + _ropeHangFactor - 1.1f,
                _baselinePos.Z
            );

            _toTarget = _targetPosition - RopeSegmentList[i].GlobalPosition;

            if (_toTarget.Length() >= 5.0f || i < 10)
            {
                RopeSegmentList[i].GlobalPosition = _targetPosition;
            }
            else
            {
                // Stretch
                _distanceToTarget = _toTarget.Length();

                // Some rope segments sometimes go too far away, and that's to some extend intended for its physics, but it can
                // Also go crazy and cause rope to completely lose its structure if it's left unchecked
                _distanceErrorFactor = Mathf.Clamp(Mathf.Exp(_distanceToTarget - 1f) - 1f, -10, 10);
                _pullStrength = _basePull * (0.6f + _distanceErrorFactor) - 0.2f;
                _moveDirection = _toTarget.Normalized();

                _targetVelocity = _moveDirection * _pullStrength * (i / 10f);
                _velocityError = _targetVelocity - RopeSegmentList[i].LinearVelocity / 1.1f;

                _springForce = _velocityError * _stiffness;
                if(i > 40)
                    _dampingForce = -RopeSegmentList[i].LinearVelocity / 5000;
                else 
                    _dampingForce = -RopeSegmentList[i].LinearVelocity / 50;

                _totalForce = _springForce + _dampingForce;

                _totalForce = _totalForce.LimitLength(_maxForce);
                RopeSegmentList[i].ApplyForce(_totalForce);
            }
            
            if(i < NumOfRopeSegments - 12)
                DebugDraw3D.DrawLine(RopeSegmentList[i + 10].GlobalPosition, RopeSegmentList[i + 12].GlobalPosition * 1.000000001f, Colors.Black);
            if(i < 1)
            {
                DebugDraw3D.DrawLine(RopeSegmentList[i + 10].GlobalPosition * 1.000000001f, EndPointA.GlobalPosition, Colors.Black);

                DebugDraw3D.DrawSphere(RopeSegmentList[i + 10].GlobalPosition, 0.005f, Colors.Black);
            }
        }
        DebugDraw3D.DrawLine(RopeSegmentList[NumOfRopeSegments - 2].GlobalPosition, EndPointB.GlobalPosition, Colors.Black);
    }

    // Could be used to render the rope ends if the existing solution doesn't hold up:
    // private Vector3 getRopeRenderPosition(int i)
    // {
    //     float _renderT = (float)i / (RopeSegmentList.Count - 1);
    //     Vector3 _startCorrection = EndPointA.GlobalPosition - RopeSegmentList[0].GlobalPosition;
    //     Vector3 _endCorrection = EndPointB.GlobalPosition - RopeSegmentList[RopeSegmentList.Count - 1].GlobalPosition;

    //     return RopeSegmentList[i].GlobalPosition + _startCorrection.Lerp(_endCorrection, _renderT);
    // }

    public override void _PhysicsProcess(double delta)
    {
        maintainRope(delta);
    }
}