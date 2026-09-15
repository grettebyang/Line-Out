using Godot;
using System;
using System.Collections.Generic;

public partial class Powerup : Node
{
	// Called when the node enters the scene tree for the first time.
	[Export]
	public AudioStreamPlayer _powerupSound;
	
	public List<int> _sequence;
	public float _duration;
	public float _maxDuration;
	public float _numOfLevels;
	public float _pausedAudioPosition;

	public virtual void PowerEffect(List<Node> dogs, DogSled sled, int dogCount, float perfection){
		_powerupSound.Play();
	}
	public virtual void EndEffect(List<Node> dogs, DogSled sled, int dogCount){
		_powerupSound.Stop();
	}
	public virtual void PauseSFX(bool pause)
	{
	}
	public virtual float GetEffectMagnitude(float perfection){
		return 0.0f;
	}
	
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	public int GetNextButton()
	{
		return 0;
	}


}
