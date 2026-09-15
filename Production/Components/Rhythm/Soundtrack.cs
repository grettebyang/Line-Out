using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public struct TimeSignature()
{
	int top;
	int bottom;
	int bars;
}

public partial class Soundtrack : AudioStreamPlayer
{
	// Called when the node enters the scene tree for the first time.
	[Export]
	public int bpm;

	[Export]
	public int[] _sequence;

	[Export]
	public bool _anyTSChanges;

	[Export]
	public int _tsTop;

	[Export]
	public int _totalBars;

	[Export]
	public TimeSignatureChange[] _TimeSignatureChanges;

	[Export]
	public SequenceChange[] _SequenceChanges;

	public int _curSeqIndex;
	public int _curTSIndex;
	public int _curBeat;
	public int _curBar;
	public int _curSeqTS = 8;
	public int _curSeqBarLength = 2;
	public int _nextChangeSeqBar; // the bar number at which the sequence should change to the next one
	public int _nextChangeTSBar; 
	public double _metronome;
	public int _onBeat;

	public double _metDelta;
	public float _curPlaybackPosition;
	public float _calibratedPlaybackPosition;
	public float _audioCalibrationOffset = 0.0f;
	public float _lastBarTimeStamp;

	public List<int> _initialSequence;
	public List<int> _initialSequenceInput;

	public override void _Ready()
    {
        Reset();
    }

	public void Reset()
    {
		VolumeDb = 3;
		_initialSequence = GetInitialSequence();
		_curPlaybackPosition = 0.0f;
		_calibratedPlaybackPosition = (_audioCalibrationOffset + (float)Stream.GetLength()) % (float)Stream.GetLength();
		_metronome = 0.0f;
		_metDelta = 0.0f;
		if(_audioCalibrationOffset < 0)
		{
			_curBeat = _tsTop - (int)Math.Floor(Math.Abs(_audioCalibrationOffset)/(60f/(float)bpm));
		}
		else if (_audioCalibrationOffset > 0)
		{
			_curBeat = (int)Math.Floor(_audioCalibrationOffset/(60f/(float)bpm));
		}
		else
		{
			_curBeat = 1;		
		}	
		_curBar = 0;
		_lastBarTimeStamp = (float)Stream.GetLength() - (_tsTop * (60f/(float)bpm));
		_curSeqIndex = 0;
		_curTSIndex = 0;
		_curSeqBarLength = _SequenceChanges[0]._sequence.Length / _tsTop;
		_nextChangeSeqBar = _SequenceChanges[_curSeqIndex].bars;
		_curSeqTS = 8;
		_onBeat = 4;
		if(_anyTSChanges)
		{
			_curSeqTS = _SequenceChanges[_curSeqIndex].timeSignature;
			_nextChangeTSBar = _TimeSignatureChanges[_curTSIndex].bars;
			_tsTop = _TimeSignatureChanges[_curTSIndex].top;
			_onBeat = _TimeSignatureChanges[_curTSIndex].onBeat;
		}
		//GD.Print("\n" + _curSeqBarLength);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//_curPlaybackPosition += (float)delta;
	}

	// Keep metronome here in sync with RhythmManager and update from RhythmManager
	public void UpdateSoundtrack(double m) // m is metronome
	{
		float beatTime = (60f / bpm);
		float barTime = _tsTop * beatTime;

		_metDelta = _calibratedPlaybackPosition;
		_curPlaybackPosition = GetPlaybackPosition() + (float)AudioServer.GetTimeSinceLastMix();
		_calibratedPlaybackPosition = ((_curPlaybackPosition + _audioCalibrationOffset) + (float)Stream.GetLength()) % (float)Stream.GetLength();
		_metDelta = _calibratedPlaybackPosition - _metDelta;
		if(Math.Abs(_metDelta) > 5.0f) // if it loops, may need to change this
		{
			_metDelta += Stream.GetLength();
			//GD.Print("Soundtrack Looped");
		}
		//GD.Print("metronome delta: " + _metDelta);
		float timeInThisBar = _calibratedPlaybackPosition - _lastBarTimeStamp;
		//If the song has started over
		if(timeInThisBar < 0)
		{
			_lastBarTimeStamp = 0;
			timeInThisBar = _calibratedPlaybackPosition;
			_curBar = 1;
		}
		_curBeat = ((int)Math.Floor(timeInThisBar / beatTime) % _tsTop) + 1;

		if(timeInThisBar >= barTime)
		{
			_lastBarTimeStamp += barTime;
			_curBar++;
		}

		//Update sequence
		if ((_curBar + _curSeqBarLength) % _totalBars == _nextChangeSeqBar + 1)
		{
			_curSeqIndex = (_curSeqIndex + 1) % _SequenceChanges.Length;
			if(_anyTSChanges)
			{
				_curSeqTS = _SequenceChanges[_curSeqIndex].timeSignature;
			}
			_curSeqBarLength = _SequenceChanges[_curSeqIndex]._sequence.Length / _curSeqTS;
			_nextChangeSeqBar = (_nextChangeSeqBar + _SequenceChanges[_curSeqIndex].bars) % _totalBars;
			//GD.Print("\n\n\n\n\n\n\n" + "Sequence Index: " + _curSeqIndex + "\nNext Sequence Change Bar: " + _nextChangeSeqBar + "\nCurrent Bar: " + _curBar + "\nTotal Bars: " + _totalBars + "\nBars of this sequence: " + _SequenceChanges[_curSeqIndex].bars);
			//GD.Print("\n\n\n\nSequence Bar Length: " + _curSeqBarLength);
		}
		//Update time signature
		if (_anyTSChanges && _curBar == _nextChangeTSBar + 1)
		{
			_curTSIndex = (_curTSIndex + 1) % _TimeSignatureChanges.Length;
			_tsTop = _TimeSignatureChanges[_curTSIndex].top;
			_onBeat = _TimeSignatureChanges[_curTSIndex].onBeat;
			//_curSeqBarLength = _SequenceChanges[_curSeqIndex]._sequence.Length / _tsTop;
			_nextChangeTSBar = (_nextChangeTSBar + _TimeSignatureChanges[_curTSIndex].bars) % _totalBars;
			//_initialSequence = GetInitialSequence();
			//GD.Print("\n\n\n\n\n\n\n" + "TSIndex: " + _curTSIndex + "\nNext TS Change Bar: " + _nextChangeTSBar + "\nCurrent Bar: " + _curBar + "\nCurrent Time Signature: " + _tsTop + "/8");
		}
	}

	public int GetSequenceLength()
	{
		return _SequenceChanges[_curSeqIndex]._sequence.Length;
	}

	public int GetSequenceBeatCount()
    {
		int count = 0;
		foreach(int beat in _SequenceChanges[_curSeqIndex]._sequence)
        {
            if(beat == 1)
            {
                count++;
            }
        }
        return count;
    }

	public int GetSequenceItemAtIndex(int index)
	{
		return _SequenceChanges[_curSeqIndex]._sequence[index];
	}
	
	public int[] GetCurrentSequence()
	{
		return _SequenceChanges[_curSeqIndex]._sequence;
	}

	public int GetTimeSignatureAtBar(int bar)
	{
		int ts = 8;
		int totalBarCount = 0;
		for(int i = 0; i < _TimeSignatureChanges.Length; i++)
		{
			totalBarCount += _TimeSignatureChanges[i].bars;
			if(bar <= totalBarCount)
			{
				return _TimeSignatureChanges[i].top;
			}
		}
		return ts;
	}

	public int GetBeatsUntilNextSequenceBar()
    {
		UpdateSoundtrack(_metronome);
		//This needs to be fixed for varying time signatures and sequences
		int ncsq = _nextChangeSeqBar;
		if(ncsq == 0)
		{
			ncsq = _totalBars;
		}
		int barsOfCurrentSequence = _SequenceChanges[_curSeqIndex].bars;
		int curSequenceStartBar = ncsq - barsOfCurrentSequence;
		int curBarInSeq = _curBar - curSequenceStartBar;
		int barsUntilNextSequence;
		if(_curBar < curSequenceStartBar || _curBar > ncsq) //we are before the sequence actually starts
		{
			if(curBarInSeq > 0)
			{
				curBarInSeq -= _totalBars;
			}
			barsUntilNextSequence = Math.Abs(curBarInSeq);
		}
		else
		{
			barsUntilNextSequence = curBarInSeq % _curSeqBarLength;
			if(curBarInSeq + barsUntilNextSequence + _curSeqBarLength > barsOfCurrentSequence)
			{
				barsUntilNextSequence += barsOfCurrentSequence - (curBarInSeq + barsUntilNextSequence + _curSeqBarLength);
			}
		}

		GD.Print("Bars until next sequence: " + barsUntilNextSequence);
		int beatCount = (_tsTop - _curBeat) + (barsUntilNextSequence) * _tsTop;
		// GD.Print("Top: " + _tsTop);
		// GD.Print("Current Beat: " + _curBeat);
		// GD.Print("Current Bar: " + _curBar);
		return beatCount;
    }

	public List<int> GetInitialSequence()
	{
		//4/4 [1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0] 7/8 [1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0] 6/8 [1, 0, 0, 1, 0, 0, 1, 0, 0] 5/4 [1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0]
		switch(_tsTop)
		{
			case 8: _initialSequenceInput = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]; return [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0]; break;
			case 7: _initialSequenceInput = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]; return [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0]; break;
			default: break;
		}
		return [1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0];
	}

	public int GetInitialSequencePosition(int index)
	{
		return _initialSequence[index];
	}


}

	
