using Godot;
using System;
using System.Collections.Generic;
using System.Dynamic;

public partial class LoopingCrossfade : Node3D
{
	// Called when the node enters the scene tree for the first time.

	public List<AudioStreamPlayer3D> AudioList = new List<AudioStreamPlayer3D>();
	[Export] public float _volume;
	public float _playLength;
	public float _timeElapsed;
	public int _audioIndex;
	public bool _muted;

	public override void _Ready(){
		GetChildren();
		GD.Print(AudioList.Count);
		_muted = false;
		_volume = -10;
		MuteAudio();
		_audioIndex = 0;
		PlayNextAudio();
	}

	public void GetChildren(){
		foreach (Node child in GetChildren(false)) {
			if (child is AudioStreamPlayer3D audioChild) {
				AudioList.Add(audioChild);
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta){
		_timeElapsed += (float)delta;

		if (AudioList.Count == 0) {
			return;
		}

		if(_timeElapsed >= _playLength){
			_audioIndex = (_audioIndex + 1) % AudioList.Count;
			//_audioIndex = (_audioIndex + 1) % 2;
			PlayNextAudio();
		}
		//GD.Print(_muted);
	}

	public void MuteAudio(){
		if(!_muted){
			foreach(AudioStreamPlayer3D audio in AudioList){
				audio.VolumeDb = -80f;
			}
			_muted = !_muted;
		}
	}

	public void PlayAudio(){
		if(_muted){
			foreach(AudioStreamPlayer3D audio in AudioList){
				audio.VolumeDb = _volume;
			}
			_muted = !_muted;
		}
	}

	public void PlayNextAudio(){
		//GD.Print(_timeElapsed);	
		_playLength = (float)AudioList[_audioIndex].Stream.GetLength() - .4f;
		_timeElapsed = 0.0f;
		AudioList[_audioIndex].Play();	
	}
}
