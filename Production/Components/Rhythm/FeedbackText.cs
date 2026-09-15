using Godot;
using System;

public partial class FeedbackText : Label
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float alph = Math.Max(0.0f, Modulate.A - .05f);
		Color col = new Color(Modulate, alph);
		Modulate = col;
	}

	public void SetVisible(){
		Color col = new Color(Modulate, 1);
		Modulate = col;
	}

	public void SetInvisible(){
		Color col = new Color(Modulate, 0);
		Modulate = col;
	}
}
