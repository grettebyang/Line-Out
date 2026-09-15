using Godot;
using System;
using System.Collections.Generic;

public partial class Hover : Powerup
{
	// Called when the node enters the scene tree for the first time.

	public override void PowerEffect(List<Node> dogs, DogSled sled, int dogCount, float perfection){
		base.PowerEffect(dogs, sled, dogCount, perfection);
		float perfPercent = GetEffectMagnitude(perfection);
		for(int i = 0; i < dogCount; i++){
			BaseCharacter curDog = dogs[i] as BaseCharacter;
			if(curDog != null){
				curDog.Hover();
			}
		}
		if(sled != null){
			sled.Hover();
		}
		_duration = _maxDuration/2f * perfPercent + _maxDuration/2f;
	}

	public override void EndEffect(List<Node> dogs, DogSled sled, int dogCount){
		base.EndEffect(dogs, sled, dogCount);
		if(sled != null){
	 		sled.HoverEnd();
		}
		for(int i = 0; i < dogCount; i++){
			BaseCharacter curDog = dogs[i] as BaseCharacter;
			if(curDog != null){
				curDog.HoverEnd();
			}
		}
		_duration = _maxDuration;
	}

    public override float GetEffectMagnitude(float perfection){
		float increment = 1.0f/_numOfLevels;
		int meterLevel = (int)Math.Floor(perfection/increment);
		return meterLevel * increment;
    }

    public override void PauseSFX(bool pause)
    {
		if(pause)
		{
			_pausedAudioPosition = _powerupSound.GetPlaybackPosition() + (float)AudioServer.GetTimeSinceLastMix();
			_powerupSound.Stop();
		}
		else
		{
			_powerupSound.Play(_pausedAudioPosition);
		}
    }

	public override void _Ready(){		
		_duration = 8.0f;
		_maxDuration = 8.0f;
		_sequence = new List<int>(new[] {1,3,2,4,1,1});
		_numOfLevels = 4f;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta){
	}
}
