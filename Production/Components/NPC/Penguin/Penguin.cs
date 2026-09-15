using Godot;
using System;
using System.Collections.Generic;


// Overview, for good structure, the Penguin scene takes in _accessPointParrent as the exportable, and that has children with Node3D poitns 
// That it will move to. It should automatically fetch the Node3d checkpoints to move to, it only needs a reference to: _accessPointParrent 
public partial class Penguin : CharacterBody3D, ICrashObstacle, ITrappable {
    // Exports
    [ExportGroup("References")]
    [Export] public Node3D AccessPointsParent;
    [Export] public Node3D PenguinModel;

    [ExportGroup("Values")]
    [Export] public float Speed = 1.8f;
    [Export] public float WaddleCycleTime = 0.9f;
    [Export] public float WaddleSize = 25; // in degrees


    // Movement & State
    private float _waddleTimer = 0f;
    private Vector3 _verticalVel = Vector3.Zero;
    private Vector3 _direction = Vector3.Zero;
    //private int _currentNodeIndex = 0;
    public List<Node3D> AccessPoints = new List<Node3D>();
    public Node3D CurrentAccessPoint;


    // Crash & Trap State
    private bool _crashedInto = false;
    private Vector3 _crashVel;
    private float _stuckTime = 0;
    private float _fleeTime = 0;
    private bool _fleeing = false;
    private Node3D _stuckPos;
    private Vector3 _pushedImpulse;

    // References
    private LevelManager _levelManager;


    public override void _Ready() {
        InitializeAccessPoints();
        _levelManager = GameManager.GetInstance().CurrentLevel;
        _levelManager.OnBark += FleeFrom;
    }

    public override void _ExitTree() {
        if (_levelManager is not null) {
            _levelManager.OnBark -= FleeFrom;
        }
    }

    public override void _PhysicsProcess(double delta) {
        delta *= BasicTimeManager.GetInstance().GameSpeed;

        if (HandleCrash()) return;

        if (HandleStuckState(delta)) return;
        
        UpdateFleeState(delta);

        if (!HasValidAccessPoints()) return;

        AlignWithFloor(PenguinModel);   

        UpdateVerticalVelocity(delta);

        UpdateDirection();

        UpdateRotation(delta);

        ApplyImpulse(delta);

        MoveWithTimeManager();

        UpdateModelWobble(delta);
        
        CheckAdvanceToNextAccessPoint();
    }

    private bool HandleCrash() {
        if (_crashedInto) {
            Velocity = _crashVel * BasicTimeManager.GetInstance().GameSpeed;
            MoveAndSlide();
            return true;
        }
        return false;
    }

    private void UpdateVerticalVelocity(double delta) {
        if (!IsOnFloor()) {
            _verticalVel.Y = Velocity.Y;
            _verticalVel += GetGravity() * (float)delta;
        }
        else {
            _verticalVel.Y = 0;
        }
    }
    private void UpdateDirection() {
        if (!_fleeing) {
            _direction = ( CurrentAccessPoint.GlobalPosition - GlobalPosition );
        }

        _direction = new Vector3(_direction.X, 0, _direction.Z).Normalized();
    }
    private void UpdateRotation(double delta) {

        Vector3 dir = _direction;
        if (dir.LengthSquared() < 0.0001f)
            return;

        // Get current and target yaw (rotation around Y)
        float currentYaw = GlobalRotation.Y;
        float targetYaw = Mathf.Atan2(dir.X, dir.Z);

        // Interpolate yaw
        float turnSpeed = 8f; // Adjust for turn rate
        float newYaw = Mathf.LerpAngle(currentYaw, targetYaw, (float)delta * turnSpeed);

        // Apply new rotation (keep X and Z rotation unchanged)
        GlobalRotation = new Vector3(Rotation.X, newYaw, Rotation.Z);
    }

    private void CheckAdvanceToNextAccessPoint() {
        Vector2 penguinPos2D = new Vector2(GlobalPosition.X, GlobalPosition.Z);
        Vector2 accessPointPos2D = new Vector2(CurrentAccessPoint.GlobalPosition.X, CurrentAccessPoint.GlobalPosition.Z);

        if (( accessPointPos2D - penguinPos2D ).Length() <= 2.1f) {
            CurrentAccessPoint = AccessPoints[(CurrentAccessPoint.GetIndex() + 1) % AccessPoints.Count];
        }
    }

    private void ApplyImpulse(double delta) {
        float speed = _fleeing ? Speed * 1.5f : Speed;
        Velocity = _direction * speed + _verticalVel;
        Velocity += _pushedImpulse;
        _pushedImpulse = _pushedImpulse.Lerp(Vector3.Zero, (float)delta);
    }

    private bool HandleStuckState(double delta) {
        if (_stuckTime > 0) {
            _stuckTime -= (float)delta;
            GlobalPosition = GlobalPosition.Lerp(_stuckPos.GlobalPosition, (float)delta * 4);
            return true;
        }
        return false;
    }

    private void UpdateModelWobble(double delta) {
        _waddleTimer += (float)delta;

        Vector3 rotation = PenguinModel.Rotation;
        rotation.Z = Mathf.Cos((Mathf.Pi / (WaddleCycleTime / 2)) * _waddleTimer) * Mathf.DegToRad(WaddleSize);
        PenguinModel.Rotation = rotation;
    }

    private bool HasValidAccessPoints() {
        if (AccessPoints == null || AccessPoints.Count == 0) {
            GD.Print("Penguin export MIA error");
            return false;
        }
        return true;
    }

    private void UpdateFleeState(double delta) {
        if (_fleeTime > 0)
            _fleeTime -= (float)delta;
        if (_fleeTime <= 0) {
            _fleeing = false;
            Speed = 1.8f;
            WaddleCycleTime = 0.9f;
        }
    }

    public void InitializeAccessPoints() {

        if (AccessPointsParent is null) {
            return;
        }

        AccessPoints.Clear();

        foreach (Node3D child in AccessPointsParent.GetChildren()) {
            AccessPoints.Add(child);
        }

        if (AccessPoints is not null && AccessPoints.Count > 0) {
            CurrentAccessPoint = AccessPoints[0];
        }
    }

    private void FleeFrom(Vector3 pos) {

        float fleeDistance = 7f;

        if ((GlobalPosition - pos).Length() > fleeDistance) {
            return;
        }

        Speed = 2.3f;
        WaddleCycleTime = 0.7f;
        _fleeing = true;
        _direction = (GlobalPosition - pos).Normalized();
        _fleeTime = 3f;
    }

    private void MoveWithTimeManager(){
        Vector3 temp = Velocity;
        Velocity = temp * BasicTimeManager.GetInstance().GameSpeed;
        MoveAndSlide();
        Velocity = temp;
    }

    // Invincible powerup effect
    public void Crashed() {
        _crashedInto = true;
        _crashVel = new Vector3(10 * GD.Randf(), Math.Abs(10 * GD.Randf()), 10 * GD.Randf());
    }

    public void TrapEffect(Damage dmg) {
        _pushedImpulse += dmg._pushAmountVec;
        if (dmg._makesStuck) {
            _stuckTime = dmg._stuckTime;
        }
        _stuckPos = dmg._posNode;
    }

    protected void AlignWithFloor(Node3D toBeAligned) {
        // Cast a ray downward to find the floor
        var result = RaycastDown(toBeAligned, false);

        Transform3D current = toBeAligned.Transform;
        Transform3D target = current;
        float lerpAmount = 0.2f;

        if (result.Count > 0) {
            if (( (Vector3)result["position"] - GlobalPosition ).Length() < 1) {
                // Get the normal of the floor
                Vector3 floorNormal = ( (Vector3)result["normal"] ).Normalized();
                if (floorNormal.SignedAngleTo(Vector3.Up, Vector3.Right) < Mathf.DegToRad(45)) {
                    target = AlignToSurface(toBeAligned.Transform, floorNormal);
                }
            }
        }
        else {
            target.Basis.Y = Vector3.Up;
            target.Basis.X = -target.Basis.Z.Cross(Vector3.Up).Normalized();
            target.Basis.Z = target.Basis.X.Cross(target.Basis.Y).Normalized();
            target.Basis = target.Basis.Orthonormalized();
        }

        target.Basis = target.Basis.Orthonormalized();

        // Lerp position
        Vector3 lerpedPos = current.Origin.Lerp(target.Origin, lerpAmount);

        // Slerp rotation (Basis)
        Basis lerpedBasis = current.Basis.Slerp(target.Basis, lerpAmount);

        lerpedBasis = lerpedBasis.Orthonormalized();

        // Apply the lerped transform
        toBeAligned.Transform = new Transform3D(lerpedBasis, lerpedPos);
    }

    protected Transform3D AlignToSurface(Transform3D Xform, Vector3 normal) {
        Xform.Basis.Y = normal;
        Xform.Basis.X = -Xform.Basis.Z.Cross(normal);
        Xform.Basis = Xform.Basis.Orthonormalized();
        return Xform;
    }

    protected Godot.Collections.Dictionary RaycastDown(Node3D pivot, bool shouldUseLocalBasis) {
        Vector3 rayStart;
        Vector3 rayEnd;

        if (shouldUseLocalBasis) {
            rayStart = pivot.GlobalPosition + pivot.Basis.Y * 0.3f;
            rayEnd = pivot.GlobalPosition + -pivot.Basis.Y * 20.0f;
        }
        else {
            rayStart = pivot.GlobalPosition + Vector3.Up * 0.3f;
            rayEnd = pivot.GlobalPosition + Vector3.Down * 20.0f;
        }
        return Raycast(rayStart, rayEnd, new Godot.Collections.Array<Rid> { GetRid() });
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
}