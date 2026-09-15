using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

// Note for future: When we set up things like rope, we probably wanna pre-load everything since players
// Will be able to join and leave the session, and we might wanna avoid loading and unloading everything
// however at the same time this will be done during main menu, so maybe performance there matters less
// So it is a consideration for future selves.


// Feedback from playtesting:
// Mini jump when jumping off a ledge: Cast 2-3 raycasts downwards and compare the distance. If it is sufficiently large (distance being say 3 units of length)
// then the dog will produce a automatic minijump (unless) the player jumped of course. Inspiration: Coyote timer

// Bug: There seems to be a case where if you hold down joystick, leave session mid-game then the dog will keep running because the joystick seems to be considered active
// So we need to constantly check if joystick is on, and if not then default to off.

public partial class Dog : RigidBody3D
{
    [Export] public float startingMoveSpeed = 10.0f;
    public float MoveSpeed = 7.0f;
    [Export] public float JumpForce = 7.0f;
    [Export] public NodePath sledPath;
    private RigidBody3D sled;

    [Export] public float MaxRopeLength = 4f;

    private float ropeLength;

    [Export] public NodePath sledPivotPath;
    private Node3D sledPivot;

    private PackedScene ropePartScene = GD.Load<PackedScene>("res://prototype//scenes//RopePart.tscn");
    private readonly List<RigidBody3D> ropeList = new();
    private bool haltMovementMode = false;

    public override void _Ready() {
        sled = GetNode<RigidBody3D>(sledPath);
        sledPivot = GetNode<Node3D>(sledPivotPath);
        instantiateRope();
    }

    // spawn rope things
    private void instantiateRope() {
        int ropeBitsCount = 100;
        for(int i = 0; i < ropeBitsCount; i++) {
            RigidBody3D ropeInstance = ropePartScene.Instantiate<RigidBody3D>();
            AddChild(ropeInstance);
            ropeList.Add(ropeInstance);
            if(i < 5)
                ropeList[i].Visible = false;
        }
    }

    private void maintainRope(double delta) {
        Vector3 end = new Vector3(GlobalPosition.X, GlobalPosition.Y - 1.5f, GlobalPosition.Z);
        Vector3 start = new Vector3(sledPivot.GlobalPosition.X, sledPivot.GlobalPosition.Y - 0.5f, sledPivot.GlobalPosition.Z);
        float distance = start.DistanceTo(end);
        
        float tension = Mathf.Clamp((distance - MaxRopeLength * 0.8f) / (MaxRopeLength * 0.8f), 0f, 1f);
        Vector3 direction = (end - start).Normalized();
        float steps = distance / ropeList.Count;

        for (int i = 0; i < ropeList.Count; i++) {
            float t = (float)i / ropeList.Count;

            float gravityFactor = 2f * Mathf.Pow(t - 0.5f, 2) * (1f - tension);
            float verticalDisplacement = Mathf.Cosh(gravityFactor * 2.5f);
            Vector3 baselinePos = start + direction * (steps * i);
            
            Vector3 targetPosition = new Vector3(
                baselinePos.X,
                baselinePos.Y + verticalDisplacement,
                baselinePos.Z
            );

            Vector3 toTarget = targetPosition - ropeList[i].GlobalPosition;
            float distanceToTarget = toTarget.Length();
            float basePull = 3f + tension * 2f;

            // dont remove this
            // DebugDraw3D.DrawLine(baselinePos, targetPosition, Colors.Red, 0.1f);
            
            float distanceErrorFactor = Mathf.Exp(distanceToTarget - 1f) - 1f;
            float pullStrength = basePull * (0.5f + distanceErrorFactor) + 0.5f;

            Vector3 moveDirection = toTarget.Normalized();
            ropeList[i].LinearVelocity = ropeList[i].LinearVelocity.Lerp(
            moveDirection * pullStrength * i / 15,
            4f *(float)delta);
        }
    }

    public void raycast(Vector3 raycastDirection, Vector3 from, float rayLength) {
		raycastDirection = new Basis(Vector3.Up, Mathf.DegToRad(0)) * raycastDirection;
		Vector3 to = from + raycastDirection * rayLength;

		var spaceState = GetWorld3D().DirectSpaceState;
		var query = new PhysicsRayQueryParameters3D {
			From = from,
			To = to
		};

		var result = spaceState.IntersectRay(query);
        DebugDraw3D.DrawLine(from, to);

        if(result.Count > 0) {
            sled.ApplyImpulse(raycastDirection * -30, raycastDirection);
        }
    }

    public override void _PhysicsProcess(double delta) {
        maintainRope(delta);

        // side raycasts from sled center:
        raycast(sled.Basis.X, sled.Position, 1.2f);
        raycast(-sled.Basis.X, sled.Position, 1.2f);

        // side raycasts from sled pivot:
        raycast(sled.Basis.X, sledPivot.GlobalPosition, 1.2f);
        raycast(-sled.Basis.X, sledPivot.GlobalPosition, 1.2f);
        raycast(sled.Basis.X, sledPivot.GlobalPosition + -sledPivot.GlobalTransform.Basis.Z * 0.5f, 1.2f);
        raycast(-sled.Basis.X, sledPivot.GlobalPosition + -sledPivot.GlobalTransform.Basis.Z * 0.5f, 1.2f);

        // up raycast from sled center:
        raycast(sled.Basis.Y, sled.Position, 1.2f);

        Vector3 inputDir = Vector3.Zero;

        if (Input.IsActionPressed("right"))    inputDir.X -= 1.0f;
        if (Input.IsActionPressed("left"))     inputDir.X += 1.0f;
        if (Input.IsActionPressed("forward"))  inputDir.Z += 1.0f;
        if (Input.IsActionPressed("backward")) inputDir.Z -= 1.0f;

        if(Input.IsActionJustPressed("halfMovementMode")) {
            haltMovementMode = !haltMovementMode;
            GD.Print("Halt movement on set to: ", haltMovementMode);
        }

        // for counteracting diagonal movement
        Vector3 direction = inputDir.Normalized();

        Vector3 pivotPos = sledPivot.GlobalTransform.Origin;
        Vector3 ropeDir = GlobalTransform.Origin - pivotPos;
        float ropeLength = ropeDir.Length();  

        bool shouldDragRope;
        // Future note: Instead of instantly stopping, just reduce movement over time
        bool haltMovement = false;
        Vector3 playerToSled = (sledPivot.GlobalPosition - GlobalTransform.Origin).Normalized();
        float dot = direction.Dot(playerToSled);
        if(dot < 0) {
            shouldDragRope = true;
            if(ropeLength > MaxRopeLength && haltMovementMode)
                haltMovement = true;
        }
        else {
            shouldDragRope = false;
            if(ropeLength <= MaxRopeLength)
                haltMovement = false;
        }

        // direction involving X and Z combined
        if (direction != Vector3.Zero && !haltMovement) {
            LinearVelocity = LinearVelocity.Lerp(new Vector3(
                direction.X * MoveSpeed,
                LinearVelocity.Y,
                direction.Z * MoveSpeed), 5f * (float)delta);
        }

        DebugDraw3D.DrawArrow(sledPivot.GlobalPosition, 2 * direction + sledPivot.GlobalPosition);
        DebugDraw3D.DrawArrow(sledPivot.GlobalPosition, 2 * direction + sledPivot.GlobalPosition);

        if (ropeLength >= MaxRopeLength && shouldDragRope) {
            Vector3 force = ropeDir.Normalized() * ropeLength * startingMoveSpeed;
            sled.ApplyForce(force * 10, pivotPos - sled.GlobalTransform.Origin);
            MoveSpeed = startingMoveSpeed - 4;
        }
        else {
            MoveSpeed = startingMoveSpeed;
            sled.ApplyForce(new Vector3(0, 0, 0), pivotPos - sled.GlobalTransform.Origin);
        }

        Vector3 sledForward = -sled.GlobalTransform.Basis.Z;
        Vector3 velocityDir = sled.LinearVelocity.Normalized();
        float angle = sledForward.AngleTo(velocityDir);

        // Recalibrates sled steering
        Vector3 torqueAxis = sledForward.Cross(velocityDir).Normalized();
        sled.ApplyTorque(torqueAxis * angle * 10f);

        // DebugDraw3D.DrawLine(direction + GlobalPosition, GlobalPosition + direction * 10, Colors.SkyBlue);
        // DebugDraw3D.DrawSphere(pivotPos, 0.25f, Colors.Cyan);
    }
}