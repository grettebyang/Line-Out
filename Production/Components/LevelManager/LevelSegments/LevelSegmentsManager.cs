using Godot;
using System;

public partial class LevelSegmentsManager : Node3D
{
    // How many segments in front and behind active segment should stay active
    [Export] int revealFront = 1;
    [Export] int revealBack = 1;

    [Export] float updateDelay = 1;

    public LevelSegment[] levelSegments;
    int closestSegmentId = 0;

    float updateTimer = 0;
    Node3D player;

    public override void _Ready()
    {
        player = GameManager.GetInstance().CurrentLevel._sled;

        // Get all segments at start
        int count = this.GetChildCount();
        levelSegments = new LevelSegment[count];

        for (int i = 0; i < count; i++)
        {
            LevelSegment segment = this.GetChild(i) as LevelSegment;
            levelSegments[i] = segment;
        }
    }

    public override void _Process(double delta)
    {
        // Update timer
        if (updateTimer > 0)
        {
            updateTimer -= (float)delta;
            return;
        }

        // Select current segment
        int count = levelSegments.Length;
        Vector3 plPos = player.GlobalPosition;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = levelSegments[i].GlobalPosition;
            Vector3 curPos = levelSegments[closestSegmentId].GlobalPosition;

            if (pos.DistanceSquaredTo(plPos) < curPos.DistanceSquaredTo(plPos))
            {
                closestSegmentId = i;
            }
        }

        // Update activity
        int max = Mathf.Clamp(closestSegmentId + revealFront, 0, count);
        int min = Mathf.Clamp(closestSegmentId - revealBack, 0, count);

        DebugDraw2D.SetText("Max: " + max);
        DebugDraw2D.SetText("Min: " + max);

        for (int i = 0; i < count; i++)
        {
            if (i >= min && i <= max)
            {
                levelSegments[i].Activate(true);
                DebugDraw2D.SetText("Activate sg: " + i);
            }
            else
            {
                levelSegments[i].Activate(false);
                DebugDraw2D.SetText("--------------------Kill sg: " + i);
            }
        }

        // Restart timer
        updateTimer = updateDelay;
    }

}
