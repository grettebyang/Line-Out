using Godot;
using System;
using System.Collections.Generic;

public partial class Rhythmbillboard1ui : Rhythmbillboardui
{
    [Export]
    CirclePrompt[] Rings;
    public int _beatsInBar = 0;

    public List<int> _beatSequence = [1, 0, 0, 0];
    public List<int> _beatIndexes = [0, 0, 0, 0];

    public int beatIndex = -1;
    public bool gone = false;

    public override void _Ready()
    {
        base._Ready();
        for(int i = 0; i < 3; i++)
        {
            Rings[i].CreatePrompt(1, i * _rhythmManager._soundtrack._onBeat, (float)_rhythmManager._bpm, (60d / _rhythmManager._bpm) - _rhythmManager._metronome, _rhythmManager._calibrationOffset);
        }
		SetCircleSizes(_rhythmManager._bpm, .3f);
        _accuracyMeter._increments = 4;
    }

    public override void _Process(double delta)
    {
        if(go)
        {
            metDelta = (float)_rhythmManager._soundtrack._metDelta;
            UpdateUI(metDelta);
            foreach(CirclePrompt c in Rings)
            {
                c.UpdateButton(metDelta);
            }

            var _prevAccuracy = _accuracy;
            var _barLength = _rhythmManager._soundtrack._tsTop;
            var beatDiff = _rhythmManager._soundtrack._curBeat - _prevBeat;
            if(beatDiff < -1)
            {
                beatDiff += _barLength;
            }
            if (beatDiff > 0)
            {
                _barLength = _rhythmManager._soundtrack._tsTop;
                //beatIndex = _rhythmManager._soundtrack._curBeat - 1;
                beatIndex++;

                // Hardcoded solution for if _beatIndex gets ahead, but as long as beatDiff > 0, it shouldn't happen?
                if(beatIndex % _barLength == _rhythmManager._soundtrack._curBeat % _barLength)
                {
                    beatIndex--;
                }
                
                int beatIndexModulate = beatIndex % _rhythmManager._soundtrack._onBeat;
                if(_beatSequence[beatIndex % 4] == 1)
                {
                    //flash
                    FlashOnBeat();
                }
                else if (beatIndexModulate == 1) // resize circles
                {
                    ResetBeatsPlayed();
                    //ShowFeedback();
                    //var index = ((beatIndex - 1) % 12)/4;
                    //GD.Print(index);
                    Rings[index].CreatePrompt(1, _rhythmManager._soundtrack._onBeat * 3 - 2, (float)_rhythmManager._bpm, (60f / (float)_rhythmManager._bpm) - _rhythmManager._metronome, _rhythmManager._calibrationOffset);
                    index = (index + 1) % 3;
                }
            }

            for(int j = 0; j < _rhythmManager._playerCount; j++)
            {
                var _curIndex = _beatIndexes[j] % 4;
            
                // If beat is current beat but on the back end of the beat (after _beatIndex increases) 
                // if(metronome < t && _beatIndex > _beatIndexes[j])
                // 		if two beats are back to back, only the first one will count as played

                // If beat is next beat but at the front of the beat (before _beatIndex increases)
                // if(metronome > t && _beatIndex == _beatIndexes[j])
                // 		if two beats are back to back, only the first one will count as played

                // If beat is played correctly
                GatherInputForPlayer(j);
                if (_curIndex < 4 && _beatSequence[_curIndex] == 1)
                {
                    if (_beatInput[j] == 1 && _currentBeatsPlayed[j] == 0)
                    {
                        _currentBeatsPlayed[j] = 1;
                        //SetIndividualFeedbackText(j, 1);
                        _accuracy++;
                    }

                    if (_beatIndexes[j] <= beatIndex && _currentBeatsPlayed[j] == 1 && _rhythmManager._metronome < (60f / (float)_rhythmManager._bpm) / 2f) //Prior to beat change
                    {
                        _beatIndexes[j]++;
                        _currentBeatsPlayed[j] = 0;
                        //GD.Print("beat changed before beat " + _beatIndex);
                    }
                    else if (_beatIndexes[j] < beatIndex && _currentBeatsPlayed[j] == 0 && _rhythmManager._metronome > (60f / (float)_rhythmManager._bpm) / 2f) //After beat change
                    {
                        _beatIndexes[j]++;
                        _currentBeatsPlayed[j] = 0;
                        //GD.Print("beat changed after beat " + _beatIndex);
                        //SetIndividualFeedbackText(j, 0);
                    }
            
                }
                else //If it is an empty beat
                {
                    //Beat index changes prior to end of metronome
                    if(_beatIndexes[j] <= beatIndex && _rhythmManager._metronome > (60f / (float)_rhythmManager._bpm) / 2f)
                    {
                        _beatIndexes[j]++;
                        //_currentBeatsPlayed[j] = 0;
                        //GD.Print("rest beat changed before beat " + _beatIndex);
                    }
                    if (_beatInput[j] == 1)
                    {
                        _currentBeatsPlayed[j] = 1;
                        // Beat played on an empty beat (too early)
                        //SetIndividualFeedbackText(j, -1);
                    }
                }
                
                //GD.Print("Beat index: " + _beatIndex + ", Player beat index: " + _beatIndexes[0] + ", sequence number: " + _soundtrack.GetSequenceItemAtIndex(_beatIndex) + ", beat played: " + _currentBeatsPlayed[0]);
            }  
            ResetBeatInput();
            if(_prevAccuracy != _accuracy)
            {
                if(_accuracy > 4f*_rhythmManager._playerCount)
                {
                    _accuracy = _accuracy % (4f*_rhythmManager._playerCount);
                }
                UpdateAccuracyMeter(100f * _accuracy / ((float)4*_rhythmManager._playerCount));
            }
            _prevBeat = _rhythmManager._soundtrack._curBeat;
        }
    }   

    public void ResizeCircles()
    {
        GD.Print("resize");
        Rings[index].CreatePrompt(1, _rhythmManager._soundtrack._onBeat * 3 - 1, (float)_rhythmManager._bpm, (60f / (float)_rhythmManager._bpm) - _rhythmManager._metronome, _rhythmManager._calibrationOffset);
        index = (index + 1) % 3;
    }

    public override void OnLevelStart()
    {
        base.OnLevelStart();
        beatIndex = _rhythmManager._soundtrack._curBeat - 1;
    }

}
