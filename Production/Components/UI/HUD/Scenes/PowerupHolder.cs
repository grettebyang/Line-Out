using Godot;
using System;

public partial class PowerupHolder : BeatListener
{
    [Export] protected TextureRect _powerupIcon;
    [Export] protected TextureRect _powerupPie;
    [Export] protected TextureProgressBar _powerupTimer;
    [Export] protected Button _hintButton;
    [Export] protected AnimationPlayer _animation;

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public void ShowReady(Texture2D icon)
    {
        _animation.Play("PowerAdd");
        //_animation.Queue("PowerReady");

        _powerupPie.Visible = false;
        //_hintButton.Visible = true;
        Visible = true;
        
        _powerupIcon.Texture = icon;
    }

    public void UpdateTimer(float remainingTime)
    {
        _powerupTimer.Value = remainingTime;
    }

    public void PowerupEnded()
    {
        _powerupPie.Visible = false;
        Visible = false;
    }

    public void PowerupActivate(float duration, float maxDuration){
        _powerupPie.Visible = true;
        _powerupTimer.MaxValue = 100f * maxDuration;
        _powerupTimer.Value = 100f * duration;
    }

    public void StartRhythmEvent()
    {
        //_hintButton.Visible = false;
        _pulse = false;
        //_animation.Play("Still");
    }

    public void ShiftUp()
    {
        _animation.Play("ShiftUp");
    }

    public void ShiftDown()
    {
        _animation.Play("ShiftDown");
    }

    public override void PulseToBeat()
    {
        // On signal from soundtrack/rhythmmanager
        if(_pulse && !_animation.IsPlaying())
            _animation.Play("PowerReady");
    }
}
