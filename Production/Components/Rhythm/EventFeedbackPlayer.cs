using Godot;
using System;
using System.Linq;

public partial class EventFeedbackPlayer : AudioStreamPlayer
{
	// Called when the node enters the scene tree for the first time.
	public AudioStreamWav[] _feedbackSounds = {
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/RhythmSuccessFeedback/RhythmOK.wav"),
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/RhythmSuccessFeedback/RhythmGood.wav"),
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/RhythmSuccessFeedback/RhythmGreat.wav"),
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/RhythmSuccessFeedback/RhythmPerfect.wav")
	};
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void PlayFeedbackSound(float accuracy, float levels)
	{
		int index = (int)(accuracy / (1.0f / levels)) - 1;
		GD.Print("rhythm feedback: " + index);
		if (index >= 0 && index < _feedbackSounds.Length){
			Stream = _feedbackSounds[index];
			Play();
		}
	}
}
