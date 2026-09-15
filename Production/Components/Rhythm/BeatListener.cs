using Godot;
using System;

public partial class BeatListener : Control
{
    [Export] private Vector2 pulseSize = new Vector2(1.2f, 1.2f);
    [Export] protected int _frequency = 1;
    [Export] protected int _onBeat = 0;
    private Vector2 baseSize = new Vector2(1.0f, 1.0f);
    protected bool _pulse = true;
    protected int _curBeat = 1;
    public override void _Ready()
    {
        base._Ready();
        _curBeat = RhythmManager.GetInstance()._soundtrack._curBeat % _frequency;
        RhythmManager.GetInstance().BeatPulse += PulseToBeat;
    }
    public override void _ExitTree()
    {
        base._ExitTree();
        RhythmManager.GetInstance().BeatPulse -= PulseToBeat;
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        Scale = Scale.Lerp(baseSize, 10f * (float)delta);
    }

    public virtual void PulseToBeat()
    {
        // Basic pulse to beat
        if(_pulse && _curBeat % _frequency == _onBeat)
            Scale = pulseSize;
            _curBeat = (_curBeat + 1) % _frequency;
    }
}
