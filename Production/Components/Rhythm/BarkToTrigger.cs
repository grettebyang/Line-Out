using Godot;
using System;

public partial class BarkToTrigger : InitialBeatIndicator
{
    public int spacing = 200;
    public void CreateTriggers(int count)
    {
        int width = (count - 1) * spacing;
        for(int i = 0; i < count; i++)
        {
            Control circle = (Control)ResourceLoader.Load<PackedScene>("res://Production/Components/Rhythm/bark_to_trigger_circle.tscn").Instantiate();
            circle.Position += new Vector2(-width/2 + spacing * i, 0.0f);
            circle.GetNode<Sprite2D>("CircleFilled").Modulate = UIConstants.PLAYER_COLORS[i];
            AddChild(circle);
            
            _indicatorCount++;
            _indicators[i] = circle;
            _initialScale = _indicators[i].Scale;
            indicatorLightUpSize[i] = _initialScale;
        }
    }

    public void DestroyTriggers()
    {
        for(int i = 0; i < _indicatorCount; i++)
        {
            _indicators[i].QueueFree();
        }
        _indicatorCount = 0;
    }
}
