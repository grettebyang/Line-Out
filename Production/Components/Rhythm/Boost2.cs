using Godot;
using System;
using System.Collections.Generic;

public partial class Boost2 : Powerup
{
	// Called when the node enters the scene tree for the first time.
	public float _prevSpeed;
	public override void PowerEffect(List<Node> dogs, DogSled _sled, int dogCount, float perfection){
		//_prevSpeed = dog._speed;
	 	//dog._speed = 20f;
	}

	public override void EndEffect(List<Node> dogs, DogSled _sled, int dogCount){
	 	//dog._speed = _prevSpeed;
	}

	public override float GetEffectMagnitude(float perfection)
    {
        return base.GetEffectMagnitude(perfection);
    }
	public override void _Ready()
	{
		_duration = 5.0f;
		//sequence = new List<int>(new[] {1,3,2,4,1,1,4,4});
		_sequence = new List<int>(new[] {5,5,5,5,5,5});
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
