using Godot;
using System;

public partial class CirclePrompt : ButtonPrompt
{
	[Export]
	public Vector2 initialScale = new Vector2(.2f, .2f);
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

    public override void CreatePrompt(int sym, int beats, float bpm, double metRem, float calibrationOffset)
    {
		_symbol = sym;
		_direction = new Vector2(1.0f, 1.0f);
		var _offset = 0.00f;
		Scale = initialScale + (_direction * ((float)beats + (((float)metRem + calibrationOffset)/(60f/bpm)) - _offset))/(bpm/60f);
		Modulate = new Color(Modulate, 1.0f - (((float)beats) + (((float)metRem + calibrationOffset)/(60f/bpm)) - _offset)/(bpm/60f));
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// if(Scale > Vector2.Zero)
        // {
		// 	Scale -= _direction * (float)delta; 
        // }
		// Modulate = new Color(Modulate, Math.Min(1.0f, Modulate.A + (float)delta * 1f));
		// //GD.Print(Modulate.A);
	}

	public override void UpdateButton(float metDelta)
	{
		if(Scale > Vector2.Zero)
        {
			Scale -= _direction * metDelta; 
        }
		Modulate = new Color(Modulate, Math.Min(1.0f, Modulate.A + metDelta * 1f));		
	}
}
