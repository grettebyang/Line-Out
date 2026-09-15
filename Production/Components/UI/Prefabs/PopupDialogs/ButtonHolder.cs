using Godot;
using System;
using System.Collections.Generic;

public partial class ButtonHolder : BoxContainer
{
    public override void _Ready()
    {
        var buttons = GetChildren();
        foreach (CursorButton b in buttons) {
			b.ResizeCollisionShape();
        }
    }
}
