using Godot;
using System;
using System.Collections.Generic;

public partial class Boost : Powerup
{
	// Called when the node enters the scene tree for the first time.
	[Export] public float _speed;
	public float _prevSpeed;
	public override void PowerEffect(List<Node> dogs, DogSled sled, int dogCount, float perfection){
		base.PowerEffect(dogs, sled, dogCount, perfection);
		float perfPercent = GetEffectMagnitude(perfection);
		for(int i = 0; i < dogCount; i++){
			BaseCharacter curDog = dogs[i] as BaseCharacter;
			if(curDog != null){
				curDog.SpeedBoost(_speed);
			}
		}
		if(sled != null){
			sled.SpeedBoost(_speed);
		}
		_duration = _maxDuration/2f * perfPercent + _maxDuration/2f;
	}

	public override void EndEffect(List<Node> dogs, DogSled sled, int dogCount){
		base.EndEffect(dogs, sled, dogCount);
	 	sled.SpeedBoostEnd(2f);
		for(int i = 0; i < dogCount; i++){
			BaseCharacter curDog = dogs[i] as BaseCharacter;
			if(curDog != null){
				curDog.SpeedBoostEnd(5f);
			}
		}
		_duration = _maxDuration;
	}

    public override float GetEffectMagnitude(float perfection){
		float increment = 1.0f/_numOfLevels;
		int meterLevel = (int)Math.Floor(perfection/increment);
		return meterLevel * increment;
    }

	public override void _Ready()
	{
		_duration = 3.0f;
		_maxDuration = 3.0f;
		_sequence = new List<int>(new[] {1,3,2,4,1,1});
		_numOfLevels = 4f;
		//sequence = new List<int>(new[] {5,5,5,5,5,5,5,5});
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
