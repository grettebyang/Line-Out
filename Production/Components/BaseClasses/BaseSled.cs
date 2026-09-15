using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;

public partial class BaseSled<T> : RigidBody3D, ITrappable, IBaseSled where T : BaseCharacter {
	// Public exported variables
	BasicTimeManager _timeManager;
	[Export] public Node3D SledPivot;
	[Export] public PackedScene RopeScene;
	[Export] public int SledMass = 320;
	[Export] public int sledMassIncreaseFactor = 40;
	[Export] public float SledTrapForceFactor = 50;
	[Export] public float SledMaxSpeed = 12f;
	[Export] public int SledToRopeResponse = 16;
	[Export] protected float ropePullPower = 14f;
	[Export] public float BaseFriction = 0.8f;
	[Export] public float _hoverDamping = 1.2f;
	[Export] public float _hoverDisplacement = 2.0f;
	public float _hoverHeight = 3.0f;
    public float _lastYPos;

	public bool _ropeStretched = false;
	public bool isSledGrounded = true;
	public bool isSledOnIce = false;
	protected bool _hover = false;
	protected float _sledMaxSpeed;
	// Global meaning that this speed adjustment will also apply to dogs
	public float GlobalSpeedAdjustment { get; set; }
	public bool IsUpsideDown => GlobalBasis.Y.Dot(Vector3.Up) < 0f;
	private int _upsideDownDmg = 5;

	[ExportGroup("SFX")]
	[Export] public AudioStreamPlayer3D _sledHitSound;
	[Export] public AudioStreamPlayer3D _sledSlidingAudio;

	public Vector3 _sledDirection;
	public Checkpoint Checkpoint { get; set; }
	public List<T> _attachedCharacters = new List<T>();
	public RopeNode _ropeNode;
	protected Dictionary<BaseCharacter, RopeNode> _ropeComponent = new Dictionary<BaseCharacter, RopeNode>();
	private float _ingnoreDmgTimer = 0;
	private float _teleportTimer = 0;
	private float _underMapTimer = 0;
	protected float _maxRopeLength = 3f;
	private float _dogSlowdownFactor = 0.7f;
	private float _sledAngularRotationCap = 0.6f;
	private float _sledAngularRotationCapY = 0.1f;

	[Signal] public delegate void RespawnSledEventHandler();
	[Signal] public delegate void OnDamageEventHandler();
	const string RESPAWN_SLED_MSG = "RespawnSled";

	[ExportGroup("Health and damage")]
	[Export] protected PackageCarrier _packageCarrier;
	[Export] protected float _minimalDmgSpeed = 8;
	[Export] protected float _dmgPerSpeed;
	[Export] protected float _dmgCap; // Hit speed dmg won't go above this no matter how fast is moving
	[Export] protected float _ingnoreDmgDelay = 0.25f;

	[ExportGroup("VFX")]
	[Export] public GpuParticles3D LeftTrackParticles;
	[Export] public GpuParticles3D RightTrackParticles;
	[Export] public GpuParticles3D IceLeftTrackParticles;
	[Export] public GpuParticles3D IceRightTrackParticles;
	[Export] public GpuParticles3D SledHitThingParticles;
	[Export] public GpuParticles3D SledHitThingParticles2;

	public PackageCarrier PackageCarrier { get => _packageCarrier; }

	public override void _Ready() {
		_timeManager = BasicTimeManager.GetInstance();
		if (GameManager.GetInstance().CurrentLevel._checkpointManager != null)
			Checkpoint = GameManager.GetInstance().CurrentLevel._checkpointManager.InitialCheckpoint;
		GlobalSpeedAdjustment = 1f;
		Mass = SledMass;
		_sledMaxSpeed = SledMaxSpeed;

		ContinuousCd = true;
	}

	// There is probably a nicer way of displaying tracks whether the sled is on ice or snow, so feel free to refactor this:
	// Reasoning for code duplication is that they otherwise will overlap each other in some scenarios and emit both tracks
	public void EmitSledTracks()
	{
		if (IceLeftTrackParticles is null || IceRightTrackParticles is null || RightTrackParticles is null || RightTrackParticles is null)
		{
			return;
		}
		if (isSledGrounded && LinearVelocity.Length() >= 0.5f)
		{
			if (isSledOnIce)
			{
				IceLeftTrackParticles.Emitting = true;
				IceRightTrackParticles.Emitting = true;
				LeftTrackParticles.Emitting = false;
				RightTrackParticles.Emitting = false;
			}
			else if (!isSledOnIce)
			{
				LeftTrackParticles.Emitting = true;
				RightTrackParticles.Emitting = true;
				IceLeftTrackParticles.Emitting = false;
				IceRightTrackParticles.Emitting = false;
			}
		}
		else
		{
			LeftTrackParticles.Emitting = false;
			RightTrackParticles.Emitting = false;
			IceLeftTrackParticles.Emitting = false;
			IceRightTrackParticles.Emitting = false;
		}
	}
	
	private void displaySledHitVFX(Vector3 _hitPoint) {
		SledHitThingParticles.Emitting = true;
		SledHitThingParticles2.Emitting = true;
		SledHitThingParticles.GlobalPosition = _hitPoint;
		SledHitThingParticles2.GlobalPosition = _hitPoint;
	}

	private void setupVFXBasics() {
		if(SledHitThingParticles != null)
			SledHitThingParticles.Emitting = false;
		if(SledHitThingParticles2 != null)
			SledHitThingParticles2.Emitting = false;
	}

	public override void _PhysicsProcess(double delta) {
		if (_timeManager.State != BasicTimeManager.TIMESTATE.MENU)
		{
			delta *= _timeManager.GameSpeed;
			PhysicsMaterialOverride.Friction = BaseFriction / _timeManager.GameSpeed;
			UpdateTimers(delta);
			LimitSledAngularVelocity();
			OutOfBounds();
			ApplyRaycasts();
			MoveSled(delta);
			ElevateSled(delta);
			CheckIfGrounded();
			SetSlidingAudio();
			CheckIfUnderMap();
			CheckIfTouchingDeathObject();
			EmitSledTracks();
			CheckIfSledIsOnIce();
			IsGoingDownhill();
		}
		else
		{
			SleepSled();
		}
	}

	private void UpdateTimers(double delta)
	{
		if (_ingnoreDmgTimer > 0)
			_ingnoreDmgTimer -= (float)delta;
		if (_teleportTimer > 0)
			_teleportTimer -= (float)delta;
		if (_underMapTimer > 0)
			_underMapTimer -= (float)delta;
	}
	private void OutOfBounds() {
		// Check if sled is out of bounds
		if (GlobalPosition.Y < -50) {
			Respawn();
		}
	}

	private void SleepSled() {
		LinearVelocity = Vector3.Zero;
		AngularVelocity = Vector3.Zero;
		Freeze = true;
	}

	private void DebugLines() {
		// Get sled transform and rectangle bounds in local space
		Transform3D sledTransform = GlobalTransform.AffineInverse();

		// Sled rectangle bounds
		float minX = -0.628f, maxX = 0.628f;
		float minZ = -1.441f, maxZ = 0.673f;
		float sledTopY = 0.25f;

		// Rectangle corners in local space
		Vector3[] localCorners = new Vector3[]
		{
			new Vector3(minX, sledTopY, minZ), // back left
			new Vector3(maxX, sledTopY, minZ), // back right
			new Vector3(maxX, sledTopY, maxZ), // front right
			new Vector3(minX, sledTopY, maxZ)  // front left
		};

		// Center in local space
		Vector3 localCenter = new Vector3(( minX + maxX ) * 0.5f, sledTopY, ( minZ + maxZ ) * 0.5f);

		// Transform to world space
		Transform3D worldTransform = GlobalTransform;
		Vector3[] worldCorners = localCorners.Select(corner => worldTransform * corner).ToArray();
		Vector3 worldCenter = worldTransform * localCenter;
	}

	public void AdjustSledToPlayerChange(int playerChangeAmount) {
		Mass += sledMassIncreaseFactor * playerChangeAmount;
		ropePullPower += 1f * -playerChangeAmount;
	}

	// If the sled gets under the map, teleport it up or respawn
	// If the sled clips under the map, pop it back on top of the surface.
	// Real falls into the void are handled by OutOfBounds, not here.
	public void CheckIfUnderMap() {
		if (_underMapTimer > 0)
			return;

		var result = RayCast(GlobalPosition + Vector3.Up, GlobalPosition - Vector3.Up * 50,
                     new Godot.Collections.Array<Rid> { GetRid() });
		if (result.Count == 0) {
			var inMapResult = RayCast(GlobalPosition, GlobalPosition + Vector3.Up * 50,
	                 new Godot.Collections.Array<Rid> { GetRid() });
			if (inMapResult.Count > 0) {
				var hitPosition = (Vector3)inMapResult["position"];
				// Only a true under-the-map clip has a surface just above our head.
				// Anything else (airtime over a deep drop, flying beside a cliff face)
				// is normal gameplay: leave the sled alone.
				if ((hitPosition - GlobalPosition).Length() < 4f) {
					GlobalPosition = hitPosition + Vector3.Up * 2;
					_underMapTimer = 3f;
					LinearVelocity = Vector3.Zero;
					AngularVelocity = Vector3.Zero;
				}
			}
		} 
	}

	public virtual void AttatchRope(BaseCharacter character)
	{
		Node3D _attatchmentPos = new Node3D();
		for (int i = 0; i < character.GetChildCount(); i++)
		{
			if (character.GetChild(i) is RopePivot rn)
			{
				_attatchmentPos = rn;
			}
		}
		_ropeNode = RopeScene.Instantiate<RopeNode>();
		CallDeferred("add_child", _ropeNode);
		_ropeComponent.Add(character, _ropeNode);

		_ropeNode.TopLevel = true;
		_ropeNode.instantiateRope(SledPivot, _attatchmentPos);
	}

	public virtual void DetatchRope(BaseCharacter character) {
		RemoveChild(_ropeComponent[character]);
		_ropeComponent[character].QueueFree();
		_ropeComponent.Remove(character);
	}

	// Way to make revolver trap not damage sled uppon collision, but messes up game for some reason:
	// [Export(PropertyHint.Layers3DPhysics)] uint _rayMask;
	protected virtual Godot.Collections.Dictionary RayCast(Vector3 rayStart, Vector3 rayEnd, Godot.Collections.Array<Rid> exclude = null)
	{
		PhysicsRayQueryParameters3D rayQuery = new PhysicsRayQueryParameters3D
		{
			From = rayStart,
			To = rayEnd,
			// CollisionMask = _rayMask,
			Exclude = exclude
		};

		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
		var result = spaceState.IntersectRay(rayQuery);
		return result;
	}
	private bool GroundedRaycast(Vector3 from, Vector3 to) {
		var spaceState = GetWorld3D().DirectSpaceState;
		var query = new PhysicsRayQueryParameters3D {
			From = from,
			To = to,
			Exclude = new Godot.Collections.Array<Rid> { GetRid() },
			HitFromInside = false 
		};

		var result = spaceState.IntersectRay(query);
		return result.Count > 0;
	}

	// This function could later on be repurposed once we have more materials than snow and ice to work with if that's ever relevant
	private void CheckIfSledIsOnIce() {
		// isSledOnIce
		var result = RayCast(GlobalPosition, GlobalPosition + (GlobalBasis.Y * -1.2f));
		// GD.Print("Raycast: ", result);
		if (result.Count > 0) {
			if ((Node3D)result["collider"] is StaticBody3D staticBody) {
				if (staticBody.PhysicsMaterialOverride?.Friction < 0.2f)
					isSledOnIce = true;
				else
					isSledOnIce = false;
			}
		}
	}		

	public void IsGoingDownhill()
	{
		Vector3 sledDirection = -GlobalTransform.Basis.Z.Normalized();
		float dot = sledDirection.Dot(Vector3.Down);

		float multiplier = 1f;

		// Sled tilting a bit less than 90 degrees (-0.10 is like 80 degrees or something)
		// Subtract by -3 for bias since actual max speed in air should be a big higher than when grounded
		if (dot < -0.10f && LinearVelocity.Length() < SledMaxSpeed - 5 && isSledGrounded)
		{
			float maxForce = Math.Min(3500f * Mathf.Abs(dot), 3000f);
			ApplyForce(GlobalTransform.Basis.Z * maxForce, sledDirection);
			// Normalized multiplier value between 1x and 3x
			multiplier = Mathf.Clamp(Mathf.InverseLerp(1500f, 2000f, maxForce), 1.1f, 1.5f) * 2f;
		}

		foreach (BaseCharacter character in _attachedCharacters)
			character.SpeedMultiplier = multiplier;
	}

	public void SetSledAngularRotationCap(float cap)
	{
		_sledAngularRotationCap = cap;
	}

	protected virtual void CheckIfGrounded() {
		Vector3 rayStart = GlobalPosition;
		Vector3 rayEnd = GlobalPosition + Vector3.Down * 1.0f;
		isSledGrounded = GroundedRaycast(rayStart, rayEnd);
	}

	public void SetSlidingAudio(){
		if(isSledGrounded && LinearVelocity.Length() > 0.1f){
			if(!_sledSlidingAudio.IsPlaying())
				_sledSlidingAudio.Play();
		}
		else{
			_sledSlidingAudio.Stop();
		}
	}

	public virtual List<BaseCharacter> GetAttachedCharacters() {
		List<BaseCharacter> attachedChars = new List<BaseCharacter>();
		foreach (var character in _attachedCharacters) {
				attachedChars.Add(character as BaseCharacter);
		}

		return attachedChars;
	}


	protected void CheckIfTouchingDeathObject() {
		Vector3 rayStart = GlobalPosition;
		Vector3 rayEnd = GlobalPosition + Vector3.Down * 3.0f;
		var result = RayCast(rayStart, rayEnd);
		if (result.Count > 0) {
			if ((Node3D)result["collider"] is DeathObject deathObject) {
				Respawn();
			}
		}
	}

	public virtual void Respawn() {
		// Heal
		if (_packageCarrier is null) {
			GD.Print("package carrier is null");
		}
		else {
			//_packageCarrier.Health.Revive();
			_packageCarrier.RevivePackages();
		}

		Node3D respawnPoint = Checkpoint._respawnPoint;

		LinearVelocity = Vector3.Zero;
		AngularVelocity = Vector3.Zero;

		GlobalPosition = respawnPoint.GlobalPosition;
		GlobalRotation = new Vector3(0, respawnPoint.GlobalRotation.Y - Mathf.Pi / 2, 0);

		PositionCharacters();
	}

	public virtual void PositionCharacters()
	{
		for (int i = 0; i < _attachedCharacters.Count; i++)
		{
			var character = _attachedCharacters[i];
			if (character == null)
			{
				GD.PrintErr("Character is null");
				continue;
			}

			Vector3 characterPos;
			characterPos = GlobalPosition + (GlobalTransform.Basis.Z * 4f);
			characterPos += GlobalBasis.X * (i - (_attachedCharacters.Count - 1) + (_attachedCharacters.Count - 1) * 0.5f);

			var result = RayCast(characterPos + Vector3.Up * 50, characterPos - Vector3.Up * 50, new Godot.Collections.Array<Rid> { character.GetRid(), GetRid() });

			if (result.Count > 0)
			{
				if ((Node3D)result["collider"] is not T)
				{
					characterPos = (Vector3)result["position"];
				}
			}

			character.GlobalPosition = characterPos;

			Vector3 rotation = Vector3.Zero;
			rotation.Y = GlobalRotation.Y + Mathf.Pi;
			character.GlobalRotation = rotation;
			character.AlignWithFloor();
		}
	}

	/*
	REFACTORNOTE: 
	Don't check character through "collider is T" as it is casting happening lot of times. 
	*/
	protected virtual bool SledRaycasts(Vector3 from, Vector3 raycastDirection, float rayLength, float forcePower, bool shouldBounce = true)
	{
		Vector3 to = from + raycastDirection * rayLength;

		var exclude = new Godot.Collections.Array<Rid> { GetRid() };
		foreach (var c in _attachedCharacters)
			if (c != null) 
				exclude.Add(c.GetRid());

		var result = RayCast(from, to, exclude);
		
		// DebugDraw3D.DrawLine(from, to, Colors.Green);

		if (result.Count > 0)
		{
			var collider = (Node3D)result["collider"];

			if (collider != null)
			{
				if (collider as Package != null)
					return false;

				// Attempt to cast the collider to T (BaseCharacter or its subclass)
				if (collider is T character && _attachedCharacters.Contains(character))
				{
					return true;
				}
				else
				{
					Vector3 hitPoint = (Vector3)result["position"];
					Vector3 normal = (Vector3)result["normal"];

					if (!IsUpsideDown && normal.Dot(Vector3.Up) > Mathf.Cos(Mathf.DegToRad(50f)))
						return true;

					if (shouldBounce && collider is not RigidBody3D && !collider.IsInGroup("terrain"))
						ApplePerpendicularOnCollideForce(-raycastDirection * forcePower, hitPoint);

					ApplySpeedHitDamage(normal);
					displaySledHitVFX(hitPoint);
				}
			}
			return true;
		}
		return false;
	}

	void ApplePerpendicularOnCollideForce(Vector3 totalForce, Vector3 hitPoint)
	{
		if (_bouncedThisTick)
			return;
		_bouncedThisTick = true;

		float impact = Mathf.Max(-LinearVelocity.Dot(totalForce.Normalized()), 0f);
		totalForce *= 0.57f + impact * 1.2f;

		ApplyImpulse(totalForce + Vector3.Up * totalForce.Length(), hitPoint - GlobalPosition);
	}
	
	/*
	REFACTORNOTE: 
		- Consider limiting amount of variable allocation. 
		- Also rewrite this AI mess.
	*/
	private bool CharacterPushSled(float forcePower)
	{
		float dogPushPower = 10f;

		// Sled rectangle bounds in local space
		float offset = 1.1f;
		float minX = -0.628f * offset, maxX = 0.628f * offset;
		float minZ = -1.441f * offset, maxZ = 0.673f * offset;
		float rayCastHeigth = 0.25f; // Height at which to cast rays
		int edgeSamples = 8;    // Number of samples per edge

		// Rectangle corners in local space
		Vector3[] localCorners = new Vector3[]
		{
		new Vector3(minX, rayCastHeigth, minZ),
		new Vector3(maxX, rayCastHeigth, minZ),
		new Vector3(maxX, rayCastHeigth, maxZ),
		new Vector3(minX, rayCastHeigth, maxZ)
		};

		Vector3 localCenter = new Vector3((minX + maxX) * 0.5f, rayCastHeigth, (minZ + maxZ) * 0.5f);

		Transform3D worldTransform = GlobalTransform;
		List<T> pushingCharacters = new List<T>();

		for (int i = 0; i < 4; i++)
		{
			Vector3 startLocal = localCorners[i];
			Vector3 endLocal = localCorners[(i + 1) % 4];
			Vector3 startWorld = worldTransform * startLocal;
			Vector3 endWorld = worldTransform * endLocal;
			Vector3 edgeDir = (endWorld - startWorld).Normalized();
			float edgeLength = (endWorld - startWorld).Length();

			// Outward normal in XZ plane
			Vector3 edge2D = new Vector3(endWorld.X - startWorld.X, 0, endWorld.Z - startWorld.Z).Normalized();
			Vector3 outward2D = new Vector3(-edge2D.Z, 0, edge2D.X); // 90 degree rotation in XZ
			Vector3 outwardWorld = outward2D.Normalized();

			// Draw the edge line for debug
			//DebugDraw3D.DrawLine(startWorld, endWorld, Colors.Cyan);

			for (int s = 0; s < edgeSamples; s++)
			{
				float t0 = (float)s / edgeSamples;
				float t1 = (float)(s + 1) / edgeSamples;
				Vector3 segStart = startWorld + edgeDir * (edgeLength * t0);
				Vector3 segEnd = startWorld + edgeDir * (edgeLength * t1);

				// Raycast along this segment
				var result = RayCast(segStart, segEnd, new Godot.Collections.Array<Rid> { GetRid() });
				if (result.Count > 0 && (Node3D)result["collider"] is T hitChar)
				{
					if (!pushingCharacters.Contains(hitChar))
					{
						pushingCharacters.Add(hitChar);

						Vector3 hitPoint = (Vector3)result["position"];
						Vector3 totalForce = outwardWorld * forcePower * dogPushPower;

						ApplyForce(totalForce, hitPoint - GlobalPosition);
					}
				}
			}
		}
		return pushingCharacters.Count > 0;
	}

	const float RAY_LENGHT = 1.15f;
	const float FORCE_POWER = 120;
	const float RAY_OFFSET_Y = 0.2f;
	const float HULL_HALF_WIDTH = 0.628f;
	const float HULL_HALF_LENGTH = 1.057f;
	const float HULL_FRONT_Z = 0.673f;
	const float SIDE_SPREAD = 0.55f; // how much of the hull length the side rays cover
	const int SIDE_RAY_COUNT = 6;
	const int UP_RAY_COUNT = 4;
	const float UP_FAN_ANGLE = 45f;
	const int FRONT_RAY_COUNT = 5;
	const float FRONT_LOW_Y = -0.15f;
	const int FRONT_FAN_STEPS = 4;
	const float FRONT_FAN_MAX_ANGLE = 60f;
	const float FRONT_RAY_SETBACK = 0.4f;
	bool _bouncedThisTick;

	protected virtual void ApplyRaycasts()
	{
		_bouncedThisTick = false;
		
		Basis basis = GlobalTransform.Basis.Orthonormalized();
		Vector3 origin = GlobalPosition;
		Vector3 right = basis.X;
		Vector3 up = basis.Y;
		Vector3 forward = basis.Z;

		Vector3 rowStart = origin + up * RAY_OFFSET_Y;
		Vector3 noseTip = origin + forward * HULL_FRONT_Z;
		float reach = HULL_HALF_LENGTH * SIDE_SPREAD;

		// Side raycasts
		for (int i = 0; i < SIDE_RAY_COUNT; i++)
		{
			Vector3 from = rowStart + forward * Mathf.Lerp(-reach, reach, (float)i / (SIDE_RAY_COUNT - 1));
			SledRaycasts(from, -right, RAY_LENGHT, FORCE_POWER * 1.5f);
			SledRaycasts(from, right, RAY_LENGHT, FORCE_POWER * 1.5f);
		}

		// Up raycasts
		float upFan = Mathf.DegToRad(UP_FAN_ANGLE);
		for (int i = 0; i < UP_RAY_COUNT; i++)
		{
			Vector3 from = rowStart + forward * Mathf.Lerp(-reach, reach, (float)i / (UP_RAY_COUNT - 1));
			SledRaycasts(from, up, RAY_LENGHT, FORCE_POWER * 1.2f);
			SledRaycasts(from, up.Rotated(forward, upFan), RAY_LENGHT, FORCE_POWER * 1.2f, false);
			SledRaycasts(from, up.Rotated(forward, -upFan), RAY_LENGHT, FORCE_POWER * 1.2f, false);
		}

		CharacterPushSled(FORCE_POWER);

		// Forward raycasts
		Vector3 frontStart = noseTip - forward * FRONT_RAY_SETBACK;
		for (int row = 0; row < 2; row++)
		{
			float height = row == 0 ? RAY_OFFSET_Y : FRONT_LOW_Y;
			for (int i = 0; i < FRONT_RAY_COUNT; i++)
			{
				float lateral = Mathf.Lerp(-HULL_HALF_WIDTH, HULL_HALF_WIDTH, (float)i / (FRONT_RAY_COUNT - 1));
				SledRaycasts(frontStart + up * height + right * lateral, forward,
							 RAY_LENGHT * 1.1f, FORCE_POWER * 1.2f, false);
			}
		}

		// Front corner raycasts
		Vector3 nose = noseTip + up * RAY_OFFSET_Y;
		for (int i = 1; i <= FRONT_FAN_STEPS; i++)
		{
			float angle = Mathf.DegToRad(FRONT_FAN_MAX_ANGLE * i / FRONT_FAN_STEPS);
			Vector3 dirA = forward.Rotated(up, angle);
			Vector3 dirB = forward.Rotated(up, -angle);

			SledRaycasts(nose + right * Mathf.Sign(dirA.Dot(right)) * HULL_HALF_WIDTH, dirA, RAY_LENGHT / 2, FORCE_POWER / 8);
			SledRaycasts(nose + right * Mathf.Sign(dirB.Dot(right)) * HULL_HALF_WIDTH, dirB, RAY_LENGHT / 2, FORCE_POWER / 8);
		}
	}

	protected virtual void LimitSledAngularVelocity() {
		float angularX = Mathf.Clamp(AngularVelocity.X, -_sledAngularRotationCap, _sledAngularRotationCap);
		float angularY = Mathf.Clamp(AngularVelocity.Y, -_sledAngularRotationCapY * 6, _sledAngularRotationCapY * 6);
		float angularZ = Mathf.Clamp(AngularVelocity.Z, -_sledAngularRotationCap, _sledAngularRotationCap);
		AngularVelocity = new Vector3(angularX, angularY, angularZ);
	}

	protected virtual void toggleSledRotationXZ(bool shouldToggle = false)
	{
		AxisLockAngularX = shouldToggle;
		AxisLockAngularZ = shouldToggle;
	}

	protected virtual void toggleSledRotationCompletely(bool shouldToggle = false)
	{
		AxisLockAngularX = shouldToggle;
		AxisLockAngularY = shouldToggle;
		AxisLockAngularZ = shouldToggle;
	}

	protected virtual void ApplySpeedHitDamage(Vector3 surfaceNormal)
	{
		if (_ingnoreDmgTimer > 0)
			return;

		if (IsUpsideDown && LinearVelocity.Length() > 1f)
		{
			HandleDamage(_upsideDownDmg);
			_ingnoreDmgTimer = 1f;
			return;
		}

		float impactSpeed = -LinearVelocity.Dot(surfaceNormal);
		if (impactSpeed <= _minimalDmgSpeed)
			return;

		float dmg = Mathf.Clamp(_dmgPerSpeed * Mathf.Log(impactSpeed - _minimalDmgSpeed + 3f), 0, _dmgCap);
		HandleDamage((int)dmg);
		_ingnoreDmgTimer = _ingnoreDmgDelay;
	}

	protected virtual Node3D GetCharacterPivot(BaseCharacter character) {
		foreach (Node3D child in character.GetChildren(true)) {
			if (child is RopePivot) {
				return child;
			}
		}
		return null;
	}

	protected virtual void MoveSled(double delta) {
		_sledDirection = Vector3.Zero;
		Freeze = false;
		if (IsUpsideDown)
		{
			Vector3 flat = new Vector3(LinearVelocity.X, 0, LinearVelocity.Z);
			if (flat.Length() > 4f)
				LinearVelocity = flat.Normalized() * 4f + Vector3.Up * LinearVelocity.Y;
		}

		foreach (BaseCharacter character in _attachedCharacters) {
			if (character != null) {
				
				Node3D characterPivot = GetCharacterPivot(character);
				_sledDirection += characterPivot.GlobalPosition - SledPivot.GlobalPosition;
				Vector3 characterDirection = characterPivot.GlobalPosition - SledPivot.GlobalPosition;

				float _ropeLength = characterDirection.Length();
				float dogToSledDiff = character.GlobalPosition.Y - GlobalPosition.Y + 2;
								
				if (_ropeLength >= _maxRopeLength)
				{
					if (!_ropeStretched)
					{
						_ropeStretched = true;
					}

					character.SpeedAdjustment = _dogSlowdownFactor;

					if (_ropeLength >= _maxRopeLength + 4)
					{
						float overstretch = _ropeLength - (_maxRopeLength + 4);

						Vector3 f = (character.GlobalPosition - GlobalPosition).Normalized() * overstretch;
						f = f.Lerp(f * ropePullPower, (float)delta);

						character.ApplyExternalForce(-f);

						if (character.InAir())
						{
							float towCap = SledMaxSpeed + LinearVelocity.Length() + overstretch;
							if (character._externalForce.Length() > towCap)
								character._externalForce = character._externalForce.Normalized() * towCap;
						}
					}
					if(_ropeLength <= _maxRopeLength + 5 && character._externalForce.Length() > 3)
					{
						character._externalForce *= 0.8f;
					}
					// If dog hangs and there is nothing under it, then we auto respawn, otherwise it should hang
					if (character.InAir() && dogToSledDiff < -(_maxRopeLength * 2)
						&& _ropeLength > (_maxRopeLength + 6) * 2)
					{
						TeleportAttached(_ropeLength, character, delta);
					}

					float gameSpeed = _timeManager.GameSpeed;
					// reduce rope stretchability by capping max sled velocity
					if (LinearVelocity.Length() > SledMaxSpeed * character.CurrentSpeedBoost * gameSpeed * GlobalSpeedAdjustment && isSledGrounded)
					{
						LinearVelocity = LinearVelocity.Normalized() * SledMaxSpeed * gameSpeed;
					}

					RopeNode _ropenodeInstance = _ropeComponent[character];

					float forceCoefficient = 2f;
					float exponentialFactor = 1.001f;

					float pullLength = Mathf.Min(_ropeLength, _maxRopeLength + 6f);

					Vector3 force = characterDirection.Normalized()
						* forceCoefficient
						* Mathf.Pow(pullLength, exponentialFactor)
						* character.DesiredSpeed;

					if (force.Length() > 40)
						force = force / 4;
						
					Vector3 pull = force * ropePullPower * pullLength;
					// We don't want dogs to drag the sled mid-air as much           
					if (character.InAir() && isSledGrounded)
						pull /= 4f;

					if (_ropeLength > _maxRopeLength + 6)
					{
						Vector3 away = (GlobalPosition - character.GlobalPosition).Normalized();
						float receding = Mathf.Max(LinearVelocity.Dot(away), 0f);
						float reelIn = Mathf.Min((_ropeLength - (_maxRopeLength + 6)) * 2f, 5f);

						LinearVelocity -= away * (receding + reelIn);
					}
					else
					{
						ApplyForce(pull, _ropenodeInstance.getRopeAverageDirection(SledToRopeResponse).Normalized());
					}
				}
				else
				{
					character.SpeedAdjustment = 1;
					_ropeStretched = false;
				}
			}
		}
	}

	protected virtual void TeleportAttached(float ropeLength, BaseCharacter character, double delta)
	{
		if (_teleportTimer > 0)
			return;
		character._externalForce = Vector3.Zero;
		_teleportTimer = 3f;

		Vector3 from = GlobalPosition + new Vector3(0, 3, 0);
		Vector3 to = character.GlobalPosition;

		var result = RayCast(from, to, new Godot.Collections.Array<Rid> { character.GetRid() });

		if (result.Count > 0)
		{
			Vector3 hitPosition = (Vector3)result["position"];
			if ((hitPosition - GlobalPosition).Length() < (character.GlobalPosition - GlobalPosition).Length())
			{
				Vector3 directionFromCharacter = (GlobalPosition - character.GlobalPosition);
				directionFromCharacter.Y = 0;
				directionFromCharacter = directionFromCharacter.Normalized();
				character.GlobalPosition = hitPosition + new Vector3(0, 1, 0) + directionFromCharacter;
				character.AlignWithFloor();
				return;
			}
		}

		Vector3 direction = (character.GlobalPosition - GlobalPosition).Normalized();
		direction.Y = 0;
		if (direction.Length() < 0.01f)
		{
			// Use sled's forward, but only if it's not vertical
			Vector3 sledForward = -GlobalBasis.Z;
			if (Math.Abs(sledForward.Y) > 0.7f) // If too vertical, use world forward
				sledForward = Vector3.Forward;
			sledForward.Y = 0;
			if (sledForward.Length() < 0.01f)
				sledForward = Vector3.Right; // Final fallback
		}
		direction = direction.Normalized();
		Vector3 newPos = GlobalPosition + direction * _maxRopeLength + Vector3.Up;
		character.GlobalPosition = newPos;
		float targetAngle = Mathf.Atan2(direction.X, direction.Z) + Mathf.Pi;
		character.GlobalRotation = new Vector3(0, targetAngle, 0);

		character.AlignWithFloor();
	}

	//------------------------------------------
	// Damage
	//------------------------------------------

	// Apply damage to all related components
	protected virtual void HandleDamage(int dmg) {
		if (dmg <= 0)
			return;
		// Packages
		if (_packageCarrier != null) {
			if (PackageCarrier.Health != null && !GameManager.GetInstance().DebugMode)
			{
				_packageCarrier.Health.Health -= (int)dmg;
			}
		}

		//Play sound
		if (_sledHitSound is not null) {
			if (!_sledHitSound.IsPlaying()) {
				_sledHitSound.Play();
			}
		}

		EmitSignal(nameof(OnDamage));
	}

	//------------------------------------------
	// Traps
	//------------------------------------------
	public virtual void TrapEffect(Damage dmg) {
		ApplyImpulse(dmg._pushAmountVec * SledTrapForceFactor, GlobalPosition * GlobalTransform.Basis.Z);
		HandleDamage(dmg._amount);
		if (dmg._source == "Death" || dmg._amount > 1000) {
			EmitSignal(RESPAWN_SLED_MSG);
		}
	}

	public virtual void BonkTrap(int dmg, Vector3 sledBumpEffect) {
		ApplyImpulse(sledBumpEffect, Vector3.Up);
		HandleDamage(dmg);
		if (PackageCarrier.Health.Health <= 0) {
			EmitSignal(RESPAWN_SLED_MSG);
		}
	}

	public bool IsMoving() {
		if (LinearVelocity.Length() < 1f) {
			return false;
		}
		return true;
	}

	//------------------------------------------
	// Powerups
	//------------------------------------------	
	protected void ElevateSled(double delta)
	{
		if (_hover)
		{
            var result = RayCast(GlobalPosition, GlobalPosition + (-Vector3.Up * _hoverHeight));
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
}

public interface IBaseSled {
	Checkpoint Checkpoint { get; set; }
	Vector3 GlobalPosition { get; set; }
	Basis GlobalBasis { get; set; }
	float GlobalSpeedAdjustment { get; set; }
	void Respawn();

	bool IsMoving();
	Rid GetRid();

	List<BaseCharacter> GetAttachedCharacters();
}
