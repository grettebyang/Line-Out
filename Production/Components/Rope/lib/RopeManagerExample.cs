using Godot;
using System;

// Simple implementation on how to make the rope and have it conected to 2 points:
public partial class RopeManagerExample : Node3D
{
    [Export] public RigidBody3D _point1;
    [Export] public RigidBody3D _point2;
    protected RopeNode _ropeComponent;
    protected PackedScene _ropeScene = GD.Load<PackedScene>("res://Production//Components//Rope//RopeScene.tscn");

    public override void _Ready() {
        _ropeComponent = _ropeScene.Instantiate<RopeNode>();
        _ropeComponent.GlobalPosition = new Vector3(GlobalPosition.X, GlobalPosition.Y + 1, GlobalPosition.Z);
        _ropeComponent.instantiateRope(_point1, _point2);
        AddChild(_ropeComponent);        
    }
}
