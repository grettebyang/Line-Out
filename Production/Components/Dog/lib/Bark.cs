using Godot;
using System;

public partial class Bark : AudioStreamPlayer3D
{
	// Called when the node enters the scene tree for the first time.
	public AudioStreamWav[] _barksList;
	
	public override void _Ready()
	{
		GD.Randomize();
	}

	public void SetBarkList(int Id)
	{
		switch (Id)
		{
			case 0: _barksList = DogBarksList.DOG_1_BARKS; VolumeDb = 0f; break;
			case 1: _barksList = DogBarksList.DOG_4_BARKS; VolumeDb = 0f; break;
			case 2: _barksList = DogBarksList.DOG_3_BARKS; VolumeDb = 0f; break;
			case 3: _barksList = DogBarksList.DOG_2_BARKS; VolumeDb = 0f; break;
			default: break;
		}
	}

	public void OnBark(){
		//Get random bark
		Stream = _barksList[GD.Randi() % _barksList.Length];
		Play();
		//GD.Print("Bark");
	}
}
