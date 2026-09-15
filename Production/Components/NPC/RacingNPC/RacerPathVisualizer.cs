using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class RacerPathVisualizer : Node3D
{
    private Array<Node3D> _cachedPathPoints = new Array<Node3D>();

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
        {
            int count = GetChildCount();
            if (_cachedPathPoints.Count != count)
            {
                Array<Node> nodes = GetChildren();
                Node3D pathNode = null;
                Node3D prevPathNode = null;

                for (int i = 0; i < count; i++)
                {
                    pathNode = nodes[i] as Node3D;
                    DebugDraw3D.DrawSphere(pathNode.GlobalPosition, 0.2f, Colors.Violet);

                    if (i > 0)
                    {
                        prevPathNode = nodes[i - 1] as Node3D;
                        DebugDraw3D.DrawLine(prevPathNode.GlobalPosition, pathNode.GlobalPosition, Colors.Violet);
                    }
                }
            }
        }
    }

}
