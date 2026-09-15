using Godot;
using System;

public partial class InitialBeatIndicator : Control
{
	// Called when the node enters the scene tree for the first time.
	public bool _fade = false;
	public Control[] _indicators = new Control[4];
	public Vector2[] indicatorLightUpSize = { new Vector2(1.0f, 1.0f), new Vector2(1.0f, 1.0f), new Vector2(1.0f, 1.0f), new Vector2(1.0f, 1.0f) };
	public Vector2 _initialScale;
	public int _indicatorCount = 0;
	public override void _Ready()
	{
		var inds = GetChildren();
		for(int i = 0; i < inds.Count; i++)
		{
			if(inds[i] is Control && i < 4)
			{
				_indicatorCount++;
				_indicators[i] = (Control)inds[i];
				_initialScale = _indicators[i].Scale;
				indicatorLightUpSize[i] = _initialScale;
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta){
		if(_fade){
			Modulate = new Color(Modulate, Math.Max(Modulate.A - .05f, 0.0f));
		}
		if(Modulate.A == 0.0f){
			for(int i = 0; i < _indicatorCount; i++){
				GetChild(i).GetNode<Sprite2D>("CircleFilled").Visible = false;
			}
			ResetCircleSize();
		}
	}

	public void HighlightIndicator(int index)
	{
		if(index >= _indicatorCount)
			return;
		_indicators[index].GetNode<Sprite2D>("CircleFilled").Visible = true;
	}

	public void SetIndicatorScale(int index)
	{
		if(index >= _indicatorCount)
			return;
		_indicators[index].Scale = _initialScale * 1.6f;
	}

	public void UpdateCircles(float delta)
	{
		for(int i = 0; i < _indicatorCount; i++)
		{
			_indicators[i].Scale = _indicators[i].Scale.Lerp(indicatorLightUpSize[i], 10f * (float)delta);	
		}
	}

	public void ResetCircleSize()
	{
		for(int i = 0; i < _indicatorCount; i++)
		{
			indicatorLightUpSize[i].X = _initialScale.X;
			indicatorLightUpSize[i].Y = _initialScale.Y;
		}
	}

	public void UpdateCircleSize(int index)
	{
		if(index >= _indicatorCount)
			return;
		indicatorLightUpSize[index].X = _initialScale.X * 1.2f;
		indicatorLightUpSize[index].Y = _initialScale.Y * 1.2f;
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
