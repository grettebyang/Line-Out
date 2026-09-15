using Godot;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

public partial class BaseCharacter : CharacterBody3D, ITrappable, IPowerupEffects
{
    [Export] public Curve SmoothstepCurve;

    [Export] public float MaxSpeed;
    public float BaseMaxSpeed = 18f;

    [Export] protected float MaxJumpHeight;

    [Export] AudioStreamPlayer3D _ropeStretchSound;
    [Export] public AudioStreamPlayer3D _trapAudio; // Whenever this is called to play, make sure stream is set to desired audio first

    public Node3D _currentTrap;
    public float SpeedAdjustment;
    public float SpeedMultiplier;
    public CharacterState State;
    public float CurrentSpeedBoost;
    public float DesiredSpeed;

    public bool isHoldingRightJoystick = false;

    public enum CharacterState
    {
        DEFAULT,
        TRAPPED,
        IDLE,
        RESPAWNING,
        FINISHED
    }

    private enum BackwardsWalking
    {
        cannotWalkBackwards = 180,
        canWalkBackwards = 150
    }

    BasicTimeManager _timeManager;

    // Private and protected:
    protected Vector3 _inputVector;
    public Vector3 _externalForce;
    protected Vector3 _carriedVelocity;
    protected Vector3 _jumpForce;
    private Vector3 _backupForce;
    private int _walkBackwardsAngle;
    private float _elapsedTimeRunning;
    private float _accelerationFactor, _decelerationFactor;
    private float _frictionFactor;
    private float _minRotationSpeed, _maxRotationSpeed;
    private float _trappedTimer = 0;
    private float _catchup;
    private float _lastGameSpeed = 1f;
    public bool _hover;
    private float _surfaceTypeSpeedAdjustment;
    private float _lastYPos;

    public float _hoverHeight;
    public bool _invincible = false;

    private Dictionary<PowerupType, Node> _activePowerups = new Dictionary<PowerupType, Node>();

    public IBaseSled Sled { get; set; }

    public Vector2 MoveInput { get { return new Vector2(_inputVector.X, _inputVector.Y); } }

    public override void _Ready()
    {
        Velocity = Vector3.Zero;
        MaxSpeed = BaseMaxSpeed;
        SpeedAdjustment = 1;
        MaxJumpHeight = 5;
        CurrentSpeedBoost = 1;
        SpeedMultiplier = 1;

        _timeManager = BasicTimeManager.GetInstance();
        _walkBackwardsAngle = (int)BackwardsWalking.canWalkBackwards;
        _accelerationFactor = 1.0f;
        _decelerationFactor = 4.0f;
        _maxRotationSpeed = 15f;
        _minRotationSpeed = 5f;
        _frictionFactor = 3f;
        _catchup = 1.0f;
        _surfaceTypeSpeedAdjustment = 1.0f;
        _elapsedTimeRunning = 0;
        _carriedVelocity = Vector3.Zero;
        _inputVector = Vector3.Zero;
        _jumpForce = Vector3.Zero;
        _externalForce = Vector3.Zero;
        _backupForce = Vector3.Zero;
    }

    public void ResetForces()
    {
        _elapsedTimeRunning = 0;
        _carriedVelocity = Vector3.Zero;
        _inputVector = Vector3.Zero;
        _jumpForce = Vector3.Zero;
        _externalForce = Vector3.Zero;
        _backupForce = Vector3.Zero;
        Velocity = Vector3.Zero;
    }

    /*
    REFACTORNOTE: 
    Most part of default state process will be better in physics process.
    */
    public override void _Process(double delta)
    {
        delta = delta * _timeManager.GameSpeed;
        switch (State)
        {
            case CharacterState.DEFAULT:
                DefaultStateProcess(delta);
                break;
            case CharacterState.TRAPPED:
                TrappedState(delta);
                break;
            case CharacterState.IDLE:
                break;
            case CharacterState.FINISHED:
                FinishedState(delta);
                break;
            default:
                break;
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();
    }



    protected virtual void FinishedState(double delta)
    {
        // Handle finished state logic here
    }

    protected virtual void DefaultStateProcess(double delta)
    {
        if (_timeManager.GameSpeed == 0)
        {
            return;
        }

        RotateTowardsInput(delta);
        OnGameSpeedChanged(_timeManager.GameSpeed);
        AlignWithFloor();
        DetectSurface();
        CheckIfTouchingDeathObject();
        Catchup();
        UpdateRunningTime(delta);
        ApplyMovementForces(delta);
        HandleCarriedForces(delta);
        CollideWithCeiling();
        //DrawDebugLines();
    }
    protected void CollideWithCeiling()
    {
        if (IsOnCeiling())
        {
            Velocity -= Velocity * Basis.Y;
            _jumpForce = Vector3.Zero;
            _carriedVelocity = Vector3.Zero;
        }
    }

    protected void HoverAboveGround(double delta)
    {
        //raycast downwards and increase distance between ground and self if smaller than minimum distance
        if (_hover)
        {
            _jumpForce = Vector3.Zero;
            var result = Raycast(GlobalPosition, GlobalPosition + (-Vector3.Up * _hoverHeight), new Godot.Collections.Array<Rid> { Sled.GetRid() });
            if (result.Count > 0)
            {
                var heightDiff = ((Vector3)result["position"] - GlobalPosition).Length();
                if (heightDiff < _hoverHeight)
                {

                    // GD.Print("go higher");
                    _lastYPos = ((Vector3)result["position"]).Y;
                }
            }
            GlobalPosition = GlobalPosition.Lerp(new Vector3(GlobalPosition.X, _lastYPos + _hoverHeight, GlobalPosition.Z), 5f * (float)delta);
        }
    }

    protected void ApplyMovementForces(double delta)
    {
        DesiredSpeed = 0;
        float acceleration = SmoothstepCurve.Sample(Mathf.Clamp(_elapsedTimeRunning, 0, 1));
        float sledSPeedAdjustment = 1.0f;

        if (Sled != null)
        {
            sledSPeedAdjustment = Sled.GlobalSpeedAdjustment;
        }

        DesiredSpeed = acceleration * MaxSpeed * _catchup * _inputVector.Length() * SpeedAdjustment * CurrentSpeedBoost * SpeedMultiplier * _timeManager.GameSpeed * sledSPeedAdjustment;

        // Move the character in the direction it is facing
        Vector3 currentForwardInput = -Basis.Z;

        Vector3 desiredVelocity = currentForwardInput * DesiredSpeed;
        if (!IsOnFloor())
        {
            _jumpForce += GetGravity() * (float)delta * _timeManager.GameSpeed;
        }

        if (IsOnFloor() && _jumpForce < Vector3.Zero)
        {
            _jumpForce = Vector3.Zero;
        }

        HoverAboveGround(delta);

        // If the input-based velocity is smaller than the sliding velocity, apply sliding
        if (desiredVelocity.Length() < _carriedVelocity.Length())
        {
            Velocity = _carriedVelocity + _externalForce + _backupForce + _jumpForce;
        }
        else
        {
            _carriedVelocity = desiredVelocity;
            Velocity = desiredVelocity + _externalForce + _backupForce + _jumpForce;
        }

        Velocity *= _surfaceTypeSpeedAdjustment;
        MoveAndSlide();
    }

    public void OnGameSpeedChanged(float newSpeed)
    {
        if (Math.Abs(_lastGameSpeed - newSpeed) > 0.001f)
        {
            float ratio = newSpeed / _lastGameSpeed;
            _jumpForce *= ratio;
            _externalForce *= ratio;
            _carriedVelocity *= ratio;
            _backupForce *= ratio;
            _lastGameSpeed = newSpeed;
        }
    }

    protected void DrawDebugLines()
    {
        // Input direction
        DebugDraw3D.DrawLine(GlobalPosition, GlobalPosition + _inputVector, Colors.Red);

        // Dog forward
        DebugDraw3D.DrawLine(GlobalPosition, GlobalPosition + -Basis.Z, Colors.Blue);

        // Dog down
        DebugDraw3D.DrawLine(GlobalPosition, GlobalPosition + -Basis.Y, Colors.LightGreen);
    }

    public virtual bool InAir()
    {
        // Adjust to make the InAir function more accurate
        float offset = 0.35f;

        Vector3 rayStart = GlobalPosition;
        Vector3 rayEnd = GlobalPosition + -GlobalBasis.Y * 3;

        // Cast a ray downwards to check if the dog is on the ground
        var result = Raycast(rayStart, rayEnd);
        //DebugDraw3D.DrawLine(rayStart, GlobalPosition + -GlobalBasis.Y * offset, Colors.Red);

        if (result.Count > 0)
        {
            if ((((Vector3)result["position"]) - GlobalPosition).Length() < offset)
            {
                return false;
            }
        }

        return true;
    }

    protected void UpdateRunningTime(double delta)
    {
        if (_inputVector.Length() > 0)
        {
            // Acceleration is slower on more slippery surfaces
            _elapsedTimeRunning += (float)delta * _accelerationFactor;
        }
        else
        {
            _elapsedTimeRunning -= (float)delta * _decelerationFactor;
        }

        _elapsedTimeRunning = Mathf.Clamp(_elapsedTimeRunning, 0, 1);
    }

    protected void HandleCarriedForces(double delta)
    {
        _externalForce = _externalForce.Lerp(Vector3.Zero, (float)delta);

        if (IsOnFloor())
        {
            _externalForce = new Vector3(_externalForce.X, 0, _externalForce.Z);
        }

        if (_externalForce.Length() < 0.2f)
        {
            _externalForce = Vector3.Zero;
        }

        _backupForce *= (float)delta * 30;
        _carriedVelocity = _carriedVelocity.Slerp(Vector3.Zero, _frictionFactor * (float)delta);
    }

    protected void Catchup()
    {
        if (IsCatchupNeeded())
        {
            _catchup = 1.2f;
        }
        else
        {
            _catchup = 1;
        }
    }

    protected bool IsCatchupNeeded()
    {

        if (Sled is null)
            return false;

        Vector2 flatZBasis = FlattenVector(Sled.GlobalBasis.Z).Normalized();
        Vector2 flatDifference = FlattenVector(GlobalPosition - Sled.GlobalPosition);

        if (flatZBasis.Dot(flatDifference) < 0 && Sled.IsMoving())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    protected Vector2 FlattenVector(Vector3 vector)
    {
        return new Vector2(vector.X, vector.Z);
    }

    public void AddSled(IBaseSled sled)
    {
        Sled = sled;
    }

    //------------------------------------------
    // Environment
    //------------------------------------------

    protected virtual Godot.Collections.Dictionary Raycast(Vector3 rayStart, Vector3 rayEnd, Godot.Collections.Array<Rid> exclude = null)
    {
        PhysicsRayQueryParameters3D rayQuery = new PhysicsRayQueryParameters3D
        {
            From = rayStart,
            To = rayEnd,
            Exclude = exclude
        };

        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
        var result = spaceState.IntersectRay(rayQuery);

        return result;
    }

    protected Godot.Collections.Dictionary RaycastDown(bool shouldUseLocalBasis)
    {
        Vector3 rayStart;
        Vector3 rayEnd;

        if (shouldUseLocalBasis)
        {
            rayStart = GlobalPosition + Basis.Y * 0.3f;
            rayEnd = GlobalPosition + -Basis.Y * 20.0f;
        }
        else
        {
            rayStart = GlobalPosition + Vector3.Up * 0.3f;
            rayEnd = GlobalPosition + Vector3.Down * 20.0f;
        }

        //DebugDraw3D.DrawLine(rayStart, rayEnd, Colors.Red);

        return Raycast(rayStart, rayEnd, new Godot.Collections.Array<Rid> { GetRid() });
    }

    protected void CheckIfTouchingDeathObject()
    {
        float dist = 0.5f;
        var result = RaycastDown(false);

        if (result.Count > 0)
        {
            if ((Node3D)result["collider"] is DeathObject deathObject)
            {
                float distToObj = ((Vector3)result["position"] - GlobalPosition).Length();
                if (distToObj < dist)
                {
                    deathObject._deathSFX.Play();
                    Sled.Respawn();
                }
            }
        }
    }

    // Exists for debugging purposes
    private FrictionBody.SURFACE_TYPES _prevSurfaceType = FrictionBody.SURFACE_TYPES.NORMAL;

    protected void DetectSurface()
    {
        var result = RaycastDown(true);

        if (result.Count > 0)
        {
            if (!IsOnFloor())
            {
                _frictionFactor = FrictionBody.NormalFriction * _timeManager.GameSpeed;
                _surfaceTypeSpeedAdjustment = FrictionBody.NormalSpeed;
            }
            FrictionBody.SURFACE_TYPES surfaceType = FrictionBody.SURFACE_TYPES.NORMAL;
            GodotObject item = (GodotObject)result["collider"];
            if (item is FrictionBody surface)
            {
                _frictionFactor = surface.GetFriction() * _timeManager.GameSpeed;
                _surfaceTypeSpeedAdjustment = surface.GetSpeedAdjustment();
                surfaceType = surface.SurfaceType;
            }
            else
            {
                _frictionFactor = FrictionBody.NormalFriction * _timeManager.GameSpeed;
                _surfaceTypeSpeedAdjustment = FrictionBody.NormalSpeed;
                surfaceType = FrictionBody.SURFACE_TYPES.UNDEFINED;
            }
            if (_prevSurfaceType != surfaceType)
            {
                GD.Print(Name + "'s surface type changed to: " + surfaceType);
            }
            _prevSurfaceType = surfaceType;
        }
    }

    public void AlignWithFloor()
    {
        // Cast a ray downward to find the floor
        var result = RaycastDown(false);

        if (result.Count > 0)
        {
            if (((Vector3)result["position"] - GlobalPosition).Length() < 1)
            {
                // Get the normal of the floor
                Vector3 floorNormal = ((Vector3)result["normal"]).Normalized();
                if (floorNormal.SignedAngleTo(Vector3.Up, Vector3.Right) < Mathf.DegToRad(45))
                {
                    GlobalTransform = AlignToSurface(GlobalTransform, floorNormal);
                }
            }
        }
        else
        {
            // Revert only the Y axis to the global basis when in the air
            Transform3D globalTransform = GlobalTransform;
            globalTransform.Basis.Y = Vector3.Up;

            // Ensure Z is not colinear with Y
            Vector3 z = globalTransform.Basis.Z;
            if (Mathf.Abs(z.Dot(Vector3.Up)) > 0.99f)
            {
                // Z is almost vertical, use X axis as fallback
                z = Vector3.Forward;
            }
            globalTransform.Basis.X = -z.Cross(Vector3.Up).Normalized();
            globalTransform.Basis.Z = globalTransform.Basis.X.Cross(globalTransform.Basis.Y).Normalized();
            globalTransform.Basis = globalTransform.Basis.Orthonormalized();
            GlobalTransform = globalTransform;
        }
    }

    protected Transform3D AlignToSurface(Transform3D Xform, Vector3 normal)
    {
        Xform.Basis.Y = normal;
        Xform.Basis.X = -Xform.Basis.Z.Cross(normal);
        Xform.Basis = Xform.Basis.Orthonormalized();
        return Xform;
    }

    //------------------------------------------
    // Input Handling
    //------------------------------------------

    protected void HandleJump()
    {
        if (IsOnFloor())
        {
            float jumpScale = _timeManager.GameSpeed;
            Vector3 jumpVector = new Vector3(0, MaxJumpHeight * jumpScale, 0);
            _jumpForce = jumpVector;
        }
    }

    protected void RotateTowardsInput(double delta)
    {
        if (_inputVector.Length() > 0)
        {
            // Adjust rotation speed based on current speed

            float speed = Mathf.Min(0, Velocity.Length());
            float rotationSpeed = Mathf.Lerp(_maxRotationSpeed, _minRotationSpeed, speed / MaxSpeed);

            // Calculate the target rotation and rotate towards it using the shortest path
            Vector3 targetDirection = new Vector3(_inputVector.X, 0, _inputVector.Z).Normalized();
            float targetAngle = Mathf.Atan2(-targetDirection.X, -targetDirection.Z);
            float currentAngle = Rotation.Y;
            float shortestAngle = Mathf.Wrap(targetAngle - currentAngle, -Mathf.Pi, Mathf.Pi);

            Vector3 targetRotation = new Vector3(0, currentAngle + shortestAngle, 0);
            Rotation = Rotation.Lerp(targetRotation, rotationSpeed * (float)delta);
        }
    }

    //------------------------------------------
    // External Forces and effects
    //------------------------------------------

    // If an external object wants to apply a force on this dog
    public void ApplyExternalForce(Vector3 force)
    {
        _externalForce += force * _timeManager.GameSpeed;
    }

    //------------------------------------------
    // Public API
    //------------------------------------------
    public void TrapEffect(Damage dmg)
    {
        if(dmg._trapSFX != null)
        {
            _trapAudio.Stream = dmg._trapSFX;
            _trapAudio.Play();
        }
        if (dmg._posNode != null && dmg._makesStuck)
        {
            State = CharacterState.TRAPPED;

            _currentTrap = dmg._posNode;
            _trappedTimer = dmg._stuckTime;
        }
        //these next two are I would consider to be one off forces, 
        //slow down being handled by friction or gravity
        //absolute push value, if it's something like a geyzer, always up
        _externalForce += dmg._pushAmountVec;
        //relative push value if we have something like a boulder
        if (dmg._posNode != null)
        {
            Vector3 external = (GlobalPosition - dmg._posNode.GlobalPosition) * dmg._pushAmountVal;
            _externalForce += external;
        }
    }
    public float GetInputLength()
    {
        return _inputVector.Length();
    }

    //-------------------------
    // Traps
    //-------------------------

    private void TrappedState(double delta)
    {
        _trappedTimer -= (float)delta;
        //GD.Print(_stuckTimer);
        if (_currentTrap != null)
        {
            float _trapLerpFactorOut = 0.8f;
            float _trapLerpFactorIn = 1.4f;
            if (_inputVector.Length() > 0)
            {
                Vector3 tempPosition = _currentTrap.GlobalPosition + _inputVector.Normalized() / 2;
                GlobalPosition = GlobalPosition.Slerp(tempPosition, (float)delta * _trapLerpFactorOut);
                RotateTowardsInput(delta);
                Velocity = _inputVector;
            }
            else
            {
                GlobalPosition = GlobalPosition.Slerp(_currentTrap.GlobalPosition, (float)delta * _trapLerpFactorIn);
            }
        }
        else
        {
            State = CharacterState.DEFAULT;
            ResetForces();
        }
        if (_trappedTimer < 0)
        {
            ResetForces();
            _currentTrap = null;
            State = CharacterState.DEFAULT;
        }
    }

    //Powerup effects
    public void SpeedBoost(float mult)
    {
        CurrentSpeedBoost = mult;
        //Create particle effect
        if(!_activePowerups.ContainsKey(PowerupType.SpeedBoost))
        {
            var scene = ResourceLoader.Load<PackedScene>("res://Production/Components/Rhythm/PowerUps/BoostEffect.tscn").Instantiate();
            _activePowerups.Add(PowerupType.SpeedBoost, scene);
            AddChild(scene);
        }
    }

    public void SpeedBoostEnd(float mult)
    {
        CurrentSpeedBoost = 1;
        if (_activePowerups.ContainsKey(PowerupType.SpeedBoost))
        {
            GD.Print("Active powerups: " + _activePowerups.Count);
            Node pu = _activePowerups[PowerupType.SpeedBoost];
            _activePowerups.Remove(PowerupType.SpeedBoost);
            //pu.QueueFree();
            if(pu != null)
                pu.Free();
        }
    }

    public void PlayRopeStretchSound()
    {
        if (!_ropeStretchSound.IsPlaying())
        {
            _ropeStretchSound.Play();
        }
    }

    private enum PowerupType
    {
        SpeedBoost,
        Invincibility,
        Hover
    }

    /*
    REFACTORNOTE: 
    Loading scenes to use powerups probably requires redundant memory allocation.
    Might be just staticaly set once in scene.
    */

    public void Hover()
    {
        MaxJumpHeight = 0.0f;
        //turn off gravity
        _hover = true;
        _hoverHeight = 3.0f;
        _lastYPos = GlobalPosition.Y;
        GlobalPosition += new Vector3(.0f, .2f, .0f);
        if(!_activePowerups.ContainsKey(PowerupType.Hover))
        {
            var scene = ResourceLoader.Load<PackedScene>("res://Production/Components/Rhythm/PowerUps/HoverEffect.tscn").Instantiate();
            _activePowerups.Add(PowerupType.Hover, scene);
            AddChild(scene);
        }
    }

    public void HoverEnd()
    {
        MaxJumpHeight = 5;
        _hover = false;
        if (_activePowerups.ContainsKey(PowerupType.Hover))
        {
            _activePowerups[PowerupType.Hover].Free();
            _activePowerups.Remove(PowerupType.Hover);
        }
    }

    public void Invincibility()
    {
        //Make dogs sparkly or something
        if (!_invincible)
        {
            _invincible = true;
            //add an area3d "invinsibility shield" as child node temporarily
            var scene = ResourceLoader.Load<PackedScene>("res://Production/Components/Dog/lib/InvincibilityShield.tscn").Instantiate();
            InvincibilityShield s = (InvincibilityShield)scene;
            _activePowerups.Add(PowerupType.Invincibility, s);
            AddChild(s);
        }
    }

    public void InvincibilityEnd()
    {
        _invincible = false;
        //delete invinsibility shield
        if (_activePowerups.ContainsKey(PowerupType.Invincibility))
        {
            _activePowerups[PowerupType.Invincibility].Free();
            _activePowerups.Remove(PowerupType.Invincibility);
        }
    }
}

