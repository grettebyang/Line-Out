using Godot;
using System;
using System.Collections.Generic;

public partial class RhythmUI : Node
{
	[Export]
	public AccuracyMeter _accuracyMeter;
	[Export]
	public ColorRect _background;
	[Export]
	public Control _initialText;
	[Export]
	public Control _countdownText;
	[Export]
	public InitialBeatIndicator _initialIndicators;
	[Export]
	public Control _center;
	[Export]
	public Label _collectiveFeedback;
	[Export]
	public Label _barkTriggerText;
	[Export]
	public Label _hikeText;
	[Export]
	public Control _collectiveFeedbackSymbols;
	[Export]
	public BarkToTrigger _barkTrigger;
	[Export]
	public Sprite2D _backgroundCue;
	[Export]
	public Control _beatCue;
	[Export]
	public Color _inputHighlightColor;
	[Export]
	public Color _wrongInputHighlightColor;
	[Export]
	public Color _innerCircleSelfModulate;
	public Label _textPrompt;
	public Sprite2D _circ;
	public Sprite2D _outerRingCircle;
	public Sprite2D _innerCircle;
	public Vector2 _innerCircleSetScale = new Vector2();
	public Vector2 _outerRingCircleSetScale = new Vector2();
	public Vector2 _initialCircleRingScale = new Vector2();

	public Godot.Color _outerRingBaseModulate = new Godot.Color();

	
	// UI constants
	public String[] AccuracyBasedFeedbackText =
	{
		"Missed!",
		"OK",
		"Good!",
		"Great!",
		"Perfect!"
	};

	public Vector2 pulseLarge = new Vector2(1.6f, 1.6f);
	public Vector2 pulseSmall = new Vector2(1.4f, 1.4f);
	public float smallCircle = .1f;
	public float bigCircle = .2f;
	public Vector2 baseScale = new Vector2(1.0f, 1.0f);
	public Vector2 countdownScale = new Vector2(1.0f, 1.0f);
	public Vector2 countdownPulseScale = new Vector2(1.4f, 1.4f);
	public List<TextureRect> feedbackSymbols = new List<TextureRect>();
	public TextureRect currentFeedback;

	public Vector2 pulseCircle = new Vector2(.04f, .04f);
	public bool fadeCircle = true;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_center = GetNode<Control>("CenterCircle"); 
		_textPrompt = _center.GetNode<Label>("Label");
		_outerRingCircle = _center.GetNode<Sprite2D>("OuterRingCircle");
		_innerCircle = _center.GetNode<Sprite2D>("InnerCircle");
		_circ = _innerCircle.GetNode<Sprite2D>("Cover");
		_initialCircleRingScale = _outerRingCircle.Scale;
		var symbols = _collectiveFeedbackSymbols.GetChildren();
		_outerRingBaseModulate = _outerRingCircle.Modulate;
		foreach(TextureRect s in symbols)
		{
			feedbackSymbols.Add(s);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	public void UpdateUI(float metDelta)
	{
		//_background.Modulate = new Godot.Color(_background.Modulate, Math.Max(0.0f, _background.Modulate.A - .08f));
		//Sprite2D circ = _center.GetNode<Sprite2D>("CircleFilled");
		Sprite2D circOutline = _center.GetNode<Sprite2D>("CircleOutline");

		_textPrompt = _center.GetNode<Label>("Label");
		_textPrompt.PivotOffset = _textPrompt.Size / 2;
		_textPrompt.Scale = _textPrompt.Scale.Lerp(baseScale, 10f * metDelta);
		_countdownText.Scale = _countdownText.Scale.Lerp(countdownScale, 10f * metDelta);
		_initialText.Scale = _initialText.Scale.Lerp(baseScale, 10f * metDelta);
		_barkTrigger.Scale = _barkTrigger.Scale.Lerp(baseScale, 10f * metDelta);
		_barkTriggerText.Scale = _barkTriggerText.Scale.Lerp(baseScale, 10f * metDelta);
		_accuracyMeter.Scale = _accuracyMeter.Scale.Lerp(baseScale, 10f * metDelta);
		_beatCue.Scale = _beatCue.Scale.Lerp(baseScale, 10f * metDelta);
		//_outerRingCircle.Modulate = _outerRingCircle.Modulate.Lerp(_outerRingBaseModulate, 8f * metDelta);
		_backgroundCue.Modulate = _backgroundCue.Modulate.Lerp(new Godot.Color(_backgroundCue.Modulate, 0.0f), 8f * metDelta);
		_accuracyMeter.Update(metDelta);
		if(_hikeText.Visible && _hikeText.Modulate.A > 0.0f)
		{
			_hikeText.Modulate = new Color(_hikeText.Modulate, Math.Max(_hikeText.Modulate.A - .01f, 0.0f));
		}
		
		// if(fadeCircle)
		// {
		// 	_circ.SelfModulate = new Godot.Color(_circ.SelfModulate, Math.Max(0.0f, _circ.SelfModulate.A - .05f));
		// }
		// _circ.Scale = new Vector2(Math.Max(1.0f, _circ.Scale.X - .01f), Math.Max(1.0f, _circ.Scale.Y - .01f));
		circOutline.Scale = circOutline.Scale.Lerp(baseScale, 10f * metDelta);	

		//_collectiveFeedback.Modulate = new Color(_collectiveFeedback.Modulate, Math.Max(_collectiveFeedback.Modulate.A - .05f, 0.0f));
		if(currentFeedback != null)
		{
			currentFeedback.Modulate = new Godot.Color(currentFeedback.Modulate, Math.Max(0.0f, currentFeedback.Modulate.A - .05f));
		}

		_initialIndicators.UpdateCircles(metDelta);
		_barkTrigger.UpdateCircles(metDelta);
	}

	public void LevelRestart(float bpm) // on start of level
	{
		// Set circle sizes
		SetCircleSizes(bpm, .3f);
		_innerCircle.SelfModulate = _innerCircleSelfModulate;

		// Reset bark to trigger ui
		_barkTriggerText.Visible = false;
		_barkTrigger.ResetCircleSize();
		_barkTrigger.DestroyTriggers();
		
		// Reset countdown and initial rhythm event ui
		countdownScale = baseScale;
		countdownPulseScale.X = 1.4f;
		countdownPulseScale.Y = 1.4f;
		_initialIndicators.ResetCircleSize();
		_initialIndicators.Visible = false;
        _initialText.Visible = false;
		_initialText.Visible = false;
		_countdownText.Visible = false;
		_initialIndicators.Visible = false;
		_center.Visible = false;
		_collectiveFeedback.Visible = true;
		_collectiveFeedback.Text = "";
		_hikeText.Visible = false;
		_hikeText.Modulate = new Color(_hikeText.Modulate, 1.0f);

		// Reset accuracy meter
		_accuracyMeter.SequenceGo(4);
		_accuracyMeter._finalFeedback.Visible = false;
	}

	public void SetCircleSizes(float bpm, float window) // window is the fraction of the beat duration to leave on either side of the beat
	{
		float halfBeatDuration = window * (60f/bpm);
		_outerRingCircleSetScale = _initialCircleRingScale + baseScale * halfBeatDuration * (_initialCircleRingScale/_center.Scale);
		_outerRingCircle.Scale = _outerRingCircleSetScale;
		_innerCircleSetScale = _initialCircleRingScale - baseScale * halfBeatDuration * (_initialCircleRingScale/_center.Scale);
		_innerCircle.Scale = _innerCircleSetScale;
	}

	public void SetupBarkTrigger(int playerCount)
	{
		_barkTrigger.CreateTriggers(playerCount);
	}

	public void PreLevelRhythmEvent(int increments)
	{
		_initialIndicators._fade = false;
		_initialIndicators.Visible = true;
		for (int k = 0; k < 4; k++)
		{
			_initialIndicators.GetChild(k).GetNode<Sprite2D>("CircleFilled").Visible = false;
		}
		_initialIndicators.SetVisible();
        _initialText.Visible = true;

		_accuracyMeter.SequenceGo(increments);
		_accuracyMeter._finalFeedback.Visible = false;

		_center.Visible = true;
		_center.Modulate = new Godot.Color(_center.Modulate, 1f);
	}

	public void BeginCountdown(int countdown)
	{
		if(_countdownText.Visible == false)
		{
			_countdownText.GetNode<Label>("CountdownText").Text = countdown.ToString();
			_countdownText.Scale = new Vector2(1.6f, 1.6f);
			_countdownText.Visible = true;
			_initialIndicators.GetChild(0).GetNode<Sprite2D>("CircleFilled").Visible = true;
		}
	}

	public void CountdownPulse(int countdown)
	{
		int index = Math.Min(3, 4 - countdown);
		countdownScale.X = 1.0f + .2f * index;
		countdownScale.Y = 1.0f + .2f * index;
		countdownPulseScale.X = 1.4f + .4f * index;
		countdownPulseScale.Y = 1.4f + .4f * index;
		_countdownText.Scale = countdownPulseScale;
		_countdownText.GetNode<Label>("CountdownText").Text = countdown.ToString();
		_initialIndicators.HighlightIndicator(index);
		_initialIndicators.UpdateCircleSize(index);
		_initialIndicators.SetIndicatorScale(index);
	}

	public void InitialEventPulse()
	{
		_countdownText.Scale = countdownPulseScale;
	}

	public void Pulse()
	{
		_initialText.Scale = pulseSmall;
		_barkTriggerText.Scale = pulseSmall;
		//_barkTrigger.Scale = pulseSmall;
		// if(_accuracyMeter.Scale.X >= .9f)
		// {
		// 	_accuracyMeter.Scale = pulseSmall;
		// }
	}

	public void UpdateAccuracyMeter(float value)
	{
		_accuracyMeter.UpdateValue((double)value);
		//_accuracyMeter.Scale = pulseSmall;
		_accuracyMeter.ChangeColor();
	}

	public void EndInitialEvent()
	{
		for(int i = 0; i < 4; i++)
		{
			_initialIndicators.GetChild(i).GetNode<Sprite2D>("CircleFilled").Visible = true;
		}
		_initialText.Visible = false;
		_countdownText.Visible = false;
		_initialIndicators._fade = true;
		_accuracyMeter.Hike();
		_hikeText.Visible = true;
	}

	public void FadeUI(float delta)
	{
		if(_center.Visible && _center.Modulate.A > 0.0f)
		{
			_outerRingCircle.Scale = _outerRingCircle.Scale.Lerp(Vector2.Zero, 10f * delta);
			_innerCircle.Scale += baseScale * 10f * delta;
			_innerCircle.SelfModulate = _innerCircle.SelfModulate.Lerp(_accuracyMeter._newColor, delta);
			_center.Modulate = new Godot.Color(_center.Modulate, Math.Max(0.0f, _center.Modulate.A - delta));			
		}
		else
		{
			_center.Visible = false;
		}
		// if(_accuracyMeter.SelfModulate.A > 0.0f)
		// {
		// 	_accuracyMeter.SelfModulate = new Godot.Color(_accuracyMeter.SelfModulate, Math.Max(0.0f, _accuracyMeter.SelfModulate.A - 3f * delta));			
		// }
	}

	public void SequenceGo(int increments)
	{
		_innerCircle.SelfModulate = _innerCircleSelfModulate;
		_innerCircle.Scale = _innerCircleSetScale;
		_outerRingCircle.Scale = _outerRingCircleSetScale;
		_barkTriggerText.Visible = false;
		_barkTrigger._fade = true;
		_center.Visible = true;
		_collectiveFeedback.Text = "";
		_center.Modulate = new Godot.Color(_center.Modulate, 1f);
		_accuracyMeter.SequenceGo(increments);
		
		//Create Accuracy meter increment bars
		//_accuracyMeter.CreateIncrementBars(increments);
	}

	public void LevelEnd()
	{
		_barkTriggerText.Visible = false;
		_barkTrigger._fade = true;
	}

	public void EndSequence()
	{
		_barkTrigger.SetInvisible();
		_accuracyMeter.ChangeColor();
		_accuracyMeter.GiveFinalFeedback();
		_accuracyMeter.Visible = false;
		var _meterBars = _accuracyMeter.GetChildren();
		for(int i = 0; i < _meterBars.Count; i++){
			_meterBars[i].Free();
		}
	}

	public void BeginRhythmCalibration(float bpm)
	{
		_center.Visible = true;
		_center.Modulate = new Godot.Color(_center.Modulate, 1f);
		SetCircleSizes(bpm, .2f);
		//FillCircle();
	}

	public void EndRhythmCalibration()
	{
		_center.Visible = false;
		fadeCircle = true;
	}

	public void HighlightCircle()
	{
		Sprite2D circ = _center.GetNode<Sprite2D>("CircleFilled");
		//_textPrompt = _center.GetNode<Label>("Label");
		
		circ.Modulate = new Godot.Color(circ.Modulate, 1.0f);
		//_textPrompt.PivotOffset = _textPrompt.Size / 2;
		//_textPrompt.Scale = _textPrompt.Scale.Lerp(new Vector2(1.4f, 1.4f), 1.4f);
	}

	public void InputPulse()
	{
		//_textPrompt.PivotOffset = _textPrompt.Size / 2;
		//_textPrompt.Scale = _textPrompt.Scale.Lerp(new Vector2(1.5f, 1.5f), 1.5f);
		//_circ.SelfModulate = _inputHighlightColor;
	}

	public void FlashOnBeat()
	{
		//_outerRingCircle.Modulate = new Godot.Color(Godot.Colors.White, 1.0f); 
		_backgroundCue.Modulate = new Godot.Color(_backgroundCue.Modulate, 1.0f); 
		_beatCue.Scale = pulseLarge;
	}

	public Vector2 GetCenterPosition()
	{
		return _center.Position;
	}

	public void ShowCollectiveFeedback(int accuracyLevel)
	{
		//_collectiveFeedback.Text = AccuracyBasedFeedbackText[accuracyLevel];
		//_collectiveFeedback.Modulate = UIConstants.FEEDBACK_COLORS[accuracyLevel];
		if(currentFeedback != null)
		{
			currentFeedback.Visible = false;
		}
		currentFeedback = feedbackSymbols[accuracyLevel];
		currentFeedback.Visible = true;
		currentFeedback.Modulate = new Godot.Color(currentFeedback.Modulate, 1.0f); 
	}

	public void CollectablePickedUp()
	{
		_barkTrigger._fade = false;
		_barkTrigger.SetVisible();
		_barkTriggerText.Visible = true;
	}

	public void BarkTriggerUpdate(int index)
	{
		_barkTrigger.HighlightIndicator(index);
		_barkTrigger.UpdateCircleSize(index);
		_barkTrigger.SetIndicatorScale(index);
	}

	public void ResetBarkTrigger()
	{
		_barkTrigger.ResetCircleSize();
		for(int i = 0; i < _barkTrigger._indicatorCount; i++)
		{
			_barkTrigger.GetChild(i).GetNode<Sprite2D>("CircleFilled").Visible = false;
		}
	}
}
