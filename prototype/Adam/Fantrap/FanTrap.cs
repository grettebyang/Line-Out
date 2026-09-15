using Godot;
using System;

public partial class FanTrap : Node3D
{
    [Export] private Node3D _rotationPivot;
    [Export] private float _rotationSpeed = 5f;
    [Export] private float _fanForce = 5000f;
    [Export] private float _fanFOV = 60f;
    [Export] private float _maxDistance = 20f;
    
    DogSled ds;

    public override void _Ready()
    {
        ds = GameManager.GetInstance().CurrentLevel._sled;
    }

    public override void _PhysicsProcess(double delta) 
    {
        if (ds == null) 
        {
            ds = GameManager.GetInstance().CurrentLevel._sled;
        }

        _rotationPivot.RotateZ(_rotationSpeed * (float)delta);
        
        if (IsPlayerInFront()) 
        {
            Vector3 pushDirection = GetFanForward();
            DebugDraw3D.DrawSphere(_rotationPivot.GlobalPosition, 1f, Colors.Green);
            
            ds.ApplyCentralForce(pushDirection * _fanForce);

            DebugDraw3D.DrawLine(_rotationPivot.GlobalPosition, _rotationPivot.GlobalPosition + (pushDirection * 5f), Colors.Red);
            DebugDraw3D.DrawSphere(ds.GlobalPosition, 0.5f, Colors.Yellow);
        }
    }

    bool IsPlayerInFront() 
    {
        Vector3 directionToPlayer = (ds.GlobalPosition - _rotationPivot.GlobalPosition).Normalized();
        Vector3 fanForward = GetFanForward();
        
        float dotProduct = fanForward.Dot(directionToPlayer);
        float angleThreshold = Mathf.Cos(Mathf.DegToRad(_fanFOV));
        
        float distance = _rotationPivot.GlobalPosition.DistanceTo(ds.GlobalPosition);
        
        return (dotProduct >= angleThreshold) && (distance <= _maxDistance);
    }

    Vector3 GetFanForward()
    {
        return _rotationPivot.GlobalTransform.Basis.Z.Normalized();
    }
}