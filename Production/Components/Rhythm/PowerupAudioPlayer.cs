using Godot;
using System;

public partial class PowerupAudioPlayer : AudioStreamPlayer
{
	// Called when the node enters the scene tree for the first time.

	private AudioStreamWav[] _powerUpSounds = {
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Powerups/Swoosh.wav"),
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Powerups/SleighBells.wav"),
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Powerups/ShieldActivateSound.wav")
	};

	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void PlayPowerupSound(int index){
		Stream = _powerUpSounds[index];
		Play();
	}
}
