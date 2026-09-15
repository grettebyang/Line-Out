using Godot;
using System;

public partial class ButtonPrompt3 : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void RandomizePitch(){
		Position = new Vector2(Position.X, (float)MakeRandom(100, 600));
	}

	public int MakeRandom(int lower, int upper){
		Random r = new Random();
		return r.Next(lower, upper);
	}
}
