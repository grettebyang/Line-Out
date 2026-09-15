using Godot;
using System;

public partial class MushroomPatch : Area3D
{
	[Signal]
	public delegate void OnPatchEnteredEventHandler();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BodyEntered += OnTriggerBodyEntered;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	private void OnTriggerBodyEntered(Node3D body)
	{
		DogController dog = body as DogController;
		if (dog == null)
			return;

		//Invert the controls of this dog
		if(dog._invincible)
			return;
            
		dog.ConfuseDog();
    }
}
