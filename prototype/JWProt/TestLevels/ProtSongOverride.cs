using Godot;
using System;

public partial class ProtSongOverride : Node
{
    [Export] AudioStream soundtrack;
    LevelManager level;

    bool switched = false;

    public override void _Ready()
    {
        level = GameManager.GetInstance().CurrentLevel;
        RhythmManager manager = RhythmManager.GetInstance();
        manager._soundtrack.Stop();
        manager._soundtrack.Stream = soundtrack;
        manager._soundtrack.Play();
    }


    public override void _Process(double delta)
    {
        if (!level.IsSessionRunning())
            return;

        RhythmManager manager = RhythmManager.GetInstance();

        if (switched)
        {
            if (!manager._soundtrack.Playing)
            {
                manager._soundtrack.Play();
            }

            return;
        }

        if (!manager._sequenceGo)
        {
            //manager._soundtrack.Stop();
            manager._soundtrack.Stream = soundtrack;
            manager._soundtrack.Play();

            switched = true;
        }

    }

}
