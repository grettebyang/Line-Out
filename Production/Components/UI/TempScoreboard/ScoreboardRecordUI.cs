using Godot;
using System;

public partial class ScoreboardRecordUI : Control
{
    [Export] public Label Left;
    [Export] public Label Right;
    [Export] Color regular;
    [Export] Color highlight;

    public void DisplayHighlighted(bool isHighlighted)
    {
        if (isHighlighted)
            Modulate = highlight;
        else
            Modulate = regular;
    }

    public void DisplayText(string left, string right)
    {
        Left.Text = left;
        Right.Text = right;
    }
}
