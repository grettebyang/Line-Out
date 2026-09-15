using Godot;
using System;

public partial class UnderShadow : Decal
{
    public override void _Ready()
    {
        PhysicsRayQueryParameters3D rayQuery = new PhysicsRayQueryParameters3D
		{
			From = GlobalPosition,
			To = GlobalPosition + (GlobalBasis.Y * -100f),
			// CollisionMask = _rayMask,
			Exclude = null
		};

		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
		var result = spaceState.IntersectRay(rayQuery);

		if (result.Count > 0) {
			GlobalPosition = (Vector3)result["position"];
		}
    }

}
