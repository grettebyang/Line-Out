using Godot;
using System;

public partial class Rhythmbillboard2ui : Rhythmbillboardui
{
    public int _beatsBarked = 0;
    public override void _Ready()
    {
        base._Ready();
    }

    public override void OnLevelStart()
    {
		if(!IsInstanceValid(this))
			return;
        
        base.OnLevelStart();
        _barkTrigger.CreateTriggers(_rhythmManager._playerCount);
    }

    public override void _Process(double delta)
    {
        if(go)
        {
            bool trigger = true;
            for(int j = 0; j < _rhythmManager._playerCount; j++)
            {
                // _beatIndex doesn't matter here
                GatherInputForPlayer(j);
                if (_beatInput[j] == 1 && _currentBeatsPlayed[j] == 0)
                {
                    _currentBeatsPlayed[j] = 1;
                    BarkTriggerUpdate(j);
                }
                if(_currentBeatsPlayed[j] == 0)
                {
                    trigger = false;
                }
            }  
            if(trigger)
            {
                _barkTrigger._fade = true;
                ResetBeatsPlayed();
            }
            else if(_barkTrigger.Modulate.A == 0.0f)
            {
                ResetBarkTrigger();
                _barkTrigger._fade = false;
                _barkTrigger.SetVisible();
            }
            ResetBeatInput();
            _barkTrigger.UpdateCircles((float)delta);
        }        
    }

}
