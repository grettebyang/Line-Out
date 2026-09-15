using Godot;
using System;

public partial class Obstacle : StaticBody3D, ICrashObstacle
{
	[Export] protected CollisionShape3D _collider;
	[Export] protected Node3D _crashPivot;
	protected Vector3 _vel;
	protected Vector3 _rot;
	protected float _timeSinceCrashed;
	protected bool _crashed = false;

	public virtual void Crashed(){
		_vel = new Vector3(GD.Randf()*2 - 1, GD.Randf(), GD.Randf()*2 - 1).Normalized() * 50f;
		_rot = new Vector3(GD.Randf()*2 - 1, GD.Randf(), GD.Randf()*2 - 1).Normalized() * 50f;
		_crashed = true;
		_timeSinceCrashed = 3.0f;
		_collider.Disabled = true;
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_vel = Vector3.Zero;
		_rot = Vector3.Zero;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		
		if(_crashed)
		{
			_crashPivot.GlobalPosition += _vel * (float)delta;
			_crashPivot.Rotation += _rot * (float)delta;
			_timeSinceCrashed -= (float)delta;
			if(_timeSinceCrashed <= 0.0f) // Make sure it doesn't keep going forever
				this.QueueFree();
		}
	}
}
