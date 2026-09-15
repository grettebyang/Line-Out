using Godot;
using System;

public partial class ScriptedButton : Button
{
    [Export] protected AudioStreamPlayer _audioPlayer;
    [Export] protected AudioStream _soundFocus;
    [Export] protected AudioStream _soundClick;

    public override void _Ready(){
        this.FocusEntered += OnFocus;
        this.Pressed += OnPressed;
    }

    protected void OnFocus(){
        _audioPlayer.Stream = _soundFocus;
        _audioPlayer.Play();
    }

    protected void OnPressed(){
        _audioPlayer.Stream = _soundClick;
        _audioPlayer.Play();
    }
}
