using Godot;
using System;

public partial class ButtonPrompt2 : ButtonPrompt
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

    public override void CreatePrompt(int sym, int beats, float bpm, double metRem, float calibrationOffset)
    {
		_symbol = sym;
		_direction = new Vector2(1.0f, 0.0f);
        Position = (_direction * (-1) * _mult * beats)/(bpm/60f);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Position += _direction * _mult * (float)delta;
	}
}
