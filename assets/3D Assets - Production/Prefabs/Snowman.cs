using Godot;
using System;

public partial class Snowman : Obstacle
{
    [Export] AudioStreamPlayer3D _audioPlayer;

    public override void Crashed()
    {
        base.Crashed();
        _audioPlayer.Play();
    }
}
