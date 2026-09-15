using Godot;
using System;

interface ICrashObstacle{
	void Crashed();
}
public partial class InvincibilityShield : Area3D
{
	// Called when the node enters the scene tree for the first time.
	[Export]
	public AudioStreamPlayer3D _hitSound;
	public override void _Ready()
	{
		BodyEntered += HitObject;
		AreaEntered += HitArea;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void HitObject(Node3D body){
		//GD.Print("hitobject");
		if(body is ICrashObstacle){
			//Make object fly away at random angle
			//body.Free();
			GD.Print("crash");
			ICrashObstacle b = body as ICrashObstacle;
			b.Crashed();
			_hitSound.Play();
		}

	}

	public void HitArea(Area3D area){
		//GD.Print("hitobject");
		if(area is ICrashObstacle){
			//Make object fly away at random angle
			//body.Free();
			ICrashObstacle b = area as ICrashObstacle;
			b.Crashed();
			_hitSound.Play();
		}

	}
}
