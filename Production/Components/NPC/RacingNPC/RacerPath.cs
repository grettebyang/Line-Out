using Godot;
using System;

public partial class RacerPath : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        var result = RaycastDown(false);
        if (result.ContainsKey("position")) {
            var position = result["position"];
            GlobalPosition = (Vector3)position + Vector3.Up;
        }

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    protected virtual Godot.Collections.Dictionary Raycast(Vector3 rayStart, Vector3 rayEnd, Godot.Collections.Array<Rid> exclude = null) {
        PhysicsRayQueryParameters3D rayQuery = new PhysicsRayQueryParameters3D {
            From = rayStart,
            To = rayEnd,
            Exclude = exclude
        };

        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
        var result = spaceState.IntersectRay(rayQuery);

        return result;
    }

    protected Godot.Collections.Dictionary RaycastDown(bool shouldUseLocalBasis) {
        Vector3 rayStart;
        Vector3 rayEnd;

        rayStart = GlobalPosition + Vector3.Up * 0.3f;
        rayEnd = GlobalPosition + Vector3.Down * 150f;

        //DebugDraw3D.DrawLine(rayStart, rayEnd, Colors.Red);

        return Raycast(rayStart, rayEnd, new Godot.Collections.Array<Rid> { });
    }
}
