using Godot;
using System;
using System.Collections.Generic;
//base class for traps, when creating a new trap update the member functions, use Always instead of _Process and update 
public partial class CrashableTrapBase : TrapBase, ICrashObstacle{
    private bool _crashedInto = false;
    private Vector3 _crashVel = new Vector3();
    private Vector3 _crashRot = new Vector3();
	private float _timeSinceCrashed;
	[Export] protected CollisionShape3D _collider;


    // Invincibility crash code
    public void Crashed(){
        _crashedInto = true;
        _crashVel = new Vector3(GD.Randf()*2 - 1, GD.Randf(), GD.Randf()*2 - 1).Normalized() * 50f;
        _crashRot = new Vector3(GD.Randf()*2 - 1, GD.Randf(), GD.Randf()*2 - 1).Normalized() * 50f;
		_timeSinceCrashed = 3.0f;
		_collider.Disabled = true;
    }

    public override void _Process(double delta){
        if(_crashedInto){
            GlobalPosition += _crashVel * (float)delta;
            Rotation += _crashRot * (float)delta;
			_timeSinceCrashed -= (float)delta;
			if(_timeSinceCrashed <= 0.0f) // Make sure it doesn't keep going forever
				this.QueueFree();
        }
        base._Process(delta);
    }
}
