using Godot;
using System;
using System.Linq.Expressions;

public partial class FallingTree : RigidbodyObstacle
{
	protected Vector3 _crashRotationOffset = new Vector3(0.0f, 3.25f, 0.0f);
    public bool fallen = false;
    private Vector3 zeroVelocity = new Vector3(0.0f, 0.0f, 0.0f);

    public override void _Process(double delta)
    {
        base._Process(delta);

        if(fallen)
        {
            AngularDamp = Math.Min(AngularDamp + 30f * (float)delta, 100.0f);
            
            if(AngularDamp == 100f)
            {
                Freeze = true;
            }
        }
    }

    public override void Crashed()
    {
        // Reposition the crashpivot by moving the tree by the difference
        Vector3 treePos = GlobalPosition;
        Vector3 rot = Rotation;
        _crashPivot.GlobalPosition = _collider.GlobalPosition;
        _crashPivot.Rotation = rot;
        Rotation = Vector3.Zero;
        GlobalPosition = treePos;
        base.Crashed();
    }
}
