using Godot;
using System;

[Tool]
public partial class LevelSegment : Node3D
{
    [Export] public Vector3 size = new Vector3(1, 1, 1);
    bool isActive = true;

    public override void _Process(double delta)
    {
        // Debug render
        Color col = Colors.Red;
        if (isActive)
            col = Colors.Blue;

        DebugDraw3D.DrawText(GlobalPosition + new Vector3(0, 5, 0), Name, 100);
        DebugDraw3D.DrawBox(GlobalPosition, GlobalBasis.GetRotationQuaternion(), size, col, true);
    }

    public void Activate(bool activate)
    {
        // Skip
        if (isActive == activate)
            return;

        isActive = activate;

        // Activate
        if (isActive)
        {
            foreach (Node node in GetChildren())
            {
                node.ProcessMode = ProcessModeEnum.Always;
                if (node is Node3D node3d)
                {
                    node3d.Visible = true;
                }
            }

            return;
        }

        // Deactivate
        foreach (Node node in GetChildren())
        {
            node.ProcessMode = ProcessModeEnum.Disabled;
            if (node is Node3D node3d)
            {
                node3d.Visible = false;
            }
        }
    }
}
