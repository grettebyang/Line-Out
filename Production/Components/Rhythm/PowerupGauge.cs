using Godot;
using System;

public partial class PowerupGauge : ProgressBar
{
	// Called when the node enters the scene tree for the first time.
	
	public int _fillAmount;
	public int _puType;
	public override void _Ready()
	{
		_puType = -1;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	public void CreatePowerup(){
		
	}

	public void FillGauge(int puType, Godot.Color col, double val){
		GD.Print(_puType);
		if(Value == 0){
			_puType = puType;
			Modulate = col;
			Value += val;
		}
		else if(_puType == puType){
			Value += val;
		}
	}
}
