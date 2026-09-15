using Godot;
using System;

public class DogBarksList{

	public static AudioStreamWav[] DOG_1_BARKS = {
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog1Barks/dog1bark1.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog1Barks/dog1bark2.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog1Barks/dog1bark3.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog1Barks/dog1bark4.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog1Barks/dog1bark5.wav")
	};

	public static AudioStreamWav[] DOG_2_BARKS = {
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog2Barks/dog2bark1.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog2Barks/dog2bark2.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog2Barks/dog2bark3.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog2Barks/dog2bark4.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog2Barks/dog2bark5.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog2Barks/dog2bark6.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog2Barks/dog2bark7.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog2Barks/dog2bark8.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog2Barks/dog2bark9.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog2Barks/dog2bark10.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog2Barks/dog2bark11.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog2Barks/dog2bark12.wav")
	};

	public static AudioStreamWav[] DOG_3_BARKS = {
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog9Barks/dog9bark1.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog9Barks/dog9bark2.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog9Barks/dog9bark3.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog9Barks/dog9bark4.wav"),
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog9Barks/dog9bark5.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog9Barks/dog9bark6.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog9Barks/dog9bark7.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog9Barks/dog9bark8.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog9Barks/dog9bark9.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog9Barks/dog9bark10.wav")
	};
	
	public static AudioStreamWav[] MISC_DOG_BARKS = {
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/SingleDogBarks/dog3bark.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/SingleDogBarks/dog5bark.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/SingleDogBarks/dog6bark.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/SingleDogBarks/dog7bark.wav")
	};

	public static AudioStreamWav[] DOG_4_BARKS = {
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog4Barks/dog4bark1.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog4Barks/dog4bark2.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog4Barks/dog4bark3.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog4Barks/dog4bark4.wav")
	};

	public static AudioStreamWav[] DOG_5_BARKS = {
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog8Barks/dog8bark1.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog8Barks/dog8bark2.wav"), 
		ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog8Barks/dog8bark3.wav")
	};
	
	public static AudioStreamWav DOUBLE_BARK = ResourceLoader.Load<AudioStreamWav>("res://assets/AudioAssets/Dog4Barks/dog4doublebark.wav");
}
