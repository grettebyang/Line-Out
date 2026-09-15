using Godot;
using System;

public partial class PenguinManager : Node
{
	[Export] Node3D PenguinPath;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetPenguinPaths();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void SetPenguinPaths() {
		if (PenguinPath is null) {
			GD.PrintErr("Penguin path is null");
			return;
		}

		foreach (Node child in GetChildren()) {
			if (child is Penguin penguin) {
				penguin.AccessPointsParent = PenguinPath;
				penguin.InitializeAccessPoints();
            }
		}
	}
	
	/*
    REFACTORNOTE: 
    Speed setting not used?
    */

    // If someone wants to set the speed of all penguins, this is the function to call
	public void SetPenguinSpeed(float speed)
	{
		foreach (Node child in GetChildren())
		{
			if (child is Penguin penguin)
			{
				penguin.Speed = speed;
			}
		}
	}

}
