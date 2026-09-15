using Godot;
using System;
using System.Collections.Generic;

public partial class Rhythmbillboardui : RhythmUI
{
    public RhythmManager _rhythmManager;
    public float metDelta;
    public int index = 0;
	public int[] _beatInput;
	public int[] _currentBeatsPlayed;
    public int _prevBeat;
    public float _accuracy = 0;
	public List<Control> _fbLabels = new List<Control>();
    public bool go = false;

    public override void _Ready()
    {
        base._Ready();
        _rhythmManager = RhythmManager.GetInstance();
        GameManager.GetInstance().CurrentLevel.OnStart += OnLevelStart;
        _prevBeat = _rhythmManager._soundtrack._curBeat;
    }

    public virtual void OnLevelStart()
    {
		if(!IsInstanceValid(this))
			return;

		ResetBeatInput();
		ResetBeatsPlayed();
		CreateFeedbackLabels();
		go = true;
    }

    public override void _ExitTree()
    {
       GameManager.GetInstance().CurrentLevel.OnStart -= OnLevelStart;
    }

	public void GatherInputForPlayer(int _player)
	{
		if (_rhythmManager._controllers[_player].IsJustPressed("bark"))
		{
			_beatInput[_player]++;
			InputPulse();
		}
	}

	public void ResetBeatInput(){
		_beatInput = new int[_rhythmManager._playerCount];
		for(int i = 0; i < _rhythmManager._playerCount; i++){
		 	_beatInput[i] = 0;
		}
	}

	public void ResetBeatsPlayed()
    {
		_currentBeatsPlayed = new int[_rhythmManager._playerCount];
		for(int i = 0; i < _rhythmManager._playerCount; i++)
        {
			_currentBeatsPlayed[i] = 0;
        }
    }

	public void ShowFeedbackIfReady()
	{
		if(_accuracy == _rhythmManager._playerCount)
		{
			int accuracyLevel = (int)Math.Floor((float)_accuracy/((float)_rhythmManager._playerCount/4f));
			ShowCollectiveFeedback(accuracyLevel);
		}
	}

	public void ShowFeedback()
	{
        for(int i = 0; i < _rhythmManager._playerCount; i++)
        {
            if(_currentBeatsPlayed[i] == 0)
            {
                SetIndividualFeedbackText(i, 0);
            }
        }
		int accuracyLevel = (int)Math.Floor((float)_accuracy/((float)_rhythmManager._playerCount/4f));
        if(accuracyLevel > 0)
        {
		    ShowCollectiveFeedback(accuracyLevel);
        }
	}

	public void SetIndividualFeedbackText(int player, int message){
		if(!_fbLabels[player].IsInsideTree())
			return;

		FeedbackText label = _fbLabels[player].GetChild<FeedbackText>(-1);
		if(_beatInput[player] == Math.Abs(message)){
			switch(message){
				case -1 : label.Text = "Too early!"; break;
				case 0 : label.Text = "Miss!"; break;
				case 1 : 
					label.Text = "Perfect!"; 
					break;
				default : break;
			}
			//label.SetVisible();
		}
	}

	public void CreateFeedbackLabels(){
		foreach(Control con in _fbLabels){
			if(IsInstanceValid(con))
				con.QueueFree();
		}
		_fbLabels = new List<Control>();
		for(int i = 0; i < _rhythmManager._playerCount; i++){
			var scene = ResourceLoader.Load<PackedScene>("res://Production/Components/Rhythm/RhythmPlayerFeedback.tscn").Instantiate();
			Control fb = (Control)scene;
			AddChild(scene);
			_fbLabels.Add(fb);
			var section = (i*Math.PI/_rhythmManager._playerCount) + Math.PI/(float)(2*_rhythmManager._playerCount) - Math.PI;
			var m = 150f;
			fb.Position = GetCenterPosition() + new Vector2(m*(float)Math.Cos(section), m*(float)Math.Sin(section));
			FeedbackText label = fb.GetChild<FeedbackText>(-1);
			label.Modulate = UIConstants.PLAYER_COLORS[i];
			label.LabelSettings.FontSize = 32;
			label.SetInvisible();
		}
	}

}
