using Godot;
using System;
using System.Linq.Expressions;

public partial class ButtonPrompt : Control
{
	public int _symbol;
	public Vector2 _direction;

	public float _mult = 200f;

	public virtual void CreatePrompt(int sym, int beats, float bpm, double metRem, float calibrationOffset){
		_symbol = sym;
		switch(_symbol){
			case 1: _direction = new Vector2(-1.0f, 0.0f); break;
			case 2: _direction = new Vector2(0.0f, -1.0f); break;
			case 3: _direction = new Vector2(1.0f, 0.0f); break;
			case 4: _direction = new Vector2(0.0f, 1.0f); break;
			default: break;
		}
		Position = (_direction * (-1)* _mult * beats)/(bpm/60f);
	} 
	public ButtonPrompt(){

	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//direction = new Vector3(0.0f, -1.0f, 0.0f);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Position += _direction * _mult * (float)delta;
	}

	public virtual void UpdateButton(float metDelta)
	{
		
	}

}
