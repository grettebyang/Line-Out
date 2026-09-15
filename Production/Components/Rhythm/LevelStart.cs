using Godot;
using System;
using System.Collections.Generic;

public partial class LevelStart : Powerup
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_duration = 0.0f;
		_sequence = new List<int>(new[] {1,1,1});
		_numOfLevels = 1;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
