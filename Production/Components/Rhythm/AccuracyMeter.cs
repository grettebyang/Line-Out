using Godot;
using System;

public partial class AccuracyMeter : TextureProgressBar
{
	// Called when the node enters the scene tree for the first time.
	[Export]
	public Label _finalFeedback;
	public int _increments;
	public Color _newColor;
	public double _newValue;
	public Vector2 _zeroScale = new Vector2(0.0f, 1.0f);
	public override void _Ready()
	{
		_newColor = Modulate;
		_newValue = Value;
		_increments = 0;
		//_finalFeedback.GlobalPosition = new Vector2(GetViewport().GetVisibleRect().Size.X/2, GetViewport().GetVisibleRect().Size.Y/2);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Update(float delta)
	{
		_finalFeedback.Modulate = new Color(_finalFeedback.Modulate, Math.Max(_finalFeedback.Modulate.A - .1f * delta, 0.0f));
		Modulate = Modulate.Lerp(_newColor, 3f * delta);
		Value = Math.Ceiling(double.Lerp(Value, _newValue, 10d * delta));
	}

	public void UpdateValue(double value)
	{
		_newValue = value;
	}

	public void CreateIncrementBars(int incs){
		_increments = incs;
		for(int i = 1; i < incs; i++){
			var scene = ResourceLoader.Load<PackedScene>("res://Production/Components/Rhythm/accuracy_meter_bar.tscn").Instantiate();
			ColorRect bar = (ColorRect)scene;
			AddChild(scene);
			bar.Position += new Vector2(i * Size.X/incs, 0.0f);
		}
	}

	public void ChangeColor(){
		int meterLevel = (int)Math.Floor(_newValue/(100f/(float)_increments));
		switch(meterLevel){
			case 0: _newColor = Colors.Beige; SetFinalFeedback("Missed!"); break;
			case 1: _newColor = Colors.Yellow; SetFinalFeedback("OK"); break;
			case 2: _newColor = Colors.Green; SetFinalFeedback("Good!"); break;
			case 3: _newColor = Colors.Blue; SetFinalFeedback("Great!"); break;
			case 4: _newColor = Colors.Purple; SetFinalFeedback("Perfect!"); break;
			default: break;
		}
	}

	public void SetFinalFeedback(string feedback){
		_finalFeedback.Text = feedback;
	}

	public void GiveFinalFeedback(){
		_finalFeedback.Modulate = new Color(_finalFeedback.Modulate, 1.0f);;
	}

	public void ResetFeedback(){
		_finalFeedback.Text = "";
		Modulate = Colors.Beige;
	}

	public void Hike(){
		Visible = false;
		_finalFeedback.Visible = true;
		_finalFeedback.Modulate = new Color(_finalFeedback.Modulate, 1.0f);
	}

	public void SequenceGo(int increments)
	{
		Scale = _zeroScale;
		UpdateValue(0.0f);
		Value = 0.0f;
		ChangeColor();
		Visible = true;
		Modulate = Colors.Beige;
		_finalFeedback.Visible = true;
		_finalFeedback.Modulate = new Color(_finalFeedback.Modulate, 0.0f);
		SelfModulate = new Color(SelfModulate, 1.0f);
		_increments = increments;
	}
}
