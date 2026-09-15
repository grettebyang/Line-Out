using Godot;
using System;

public partial class ConfusionSoundsList : Node
{
	public static AudioStreamWav[] CONFUSION_SOUNDS = {
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Traps/MushroomPatch/Confused1.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Traps/MushroomPatch/Confused2.wav"),
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Traps/MushroomPatch/Confused3.wav"),
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Traps/MushroomPatch/Confused4.wav")
	};
}
