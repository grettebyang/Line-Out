using Godot;
using System;

public partial class BonkTrap : Node3D
{
	[Export] MeshInstance3D bonkerCurrent;
	[Export] MeshInstance3D bonkerStarting;
	[Export] MeshInstance3D bonkerEnding;
	[Export] Curve pushSpeedCurve;
	[Export] Curve pullSpeedCurve;
	[Export] Area3D bonkerCurrentCollisionDetection;
	[Export] float pushSpeed = 10.0f;
	[Export] float pullSpeed = 10.0f;
	[Export] float cooldownTimer = 15f;
	[Export] Vector3 sledImpulseEffect = new Vector3(10, 250, 15);
	[Export] private AudioStream bumpSound;
	[Export] private int activationDistance = 25;
	[Export] private float _creepThreshold = 0.1f; // 10% creeping
	[Export] private float _creepSpeedMultiplier = 0.01f; 
	private bool _hasCreeped = false;
	private float currentCooldown = 0f;
	private float movementProgress = 0f;
	private float totalMovementTime = 0f;
	[Export] private int _trapDamage = 15; 
	bool isPulling = false;
	DogSled ds;
	
	public override void _Ready()
	{
		ds = GameManager.GetInstance()?.CurrentLevel?._sled;
		bonkerCurrentCollisionDetection.BodyEntered += damageSled;
	}

	// Signal
	void damageSled(Node3D body)
	{
		if (!isPulling)
		{
			if (body is DogSled sled)
				sled.BonkTrap(_trapDamage, sledImpulseEffect);
			if (body is DogController dog)
				dog.SmoshDog();
		}
	}

	public override void _PhysicsProcess(double delta) {
		HandleCurrentBonk((float)delta);
	}

	void HandleCurrentBonk(float delta) {
		// If CurrentLevel null, it probably means we're in a debug level
		if (GameManager.GetInstance().CurrentLevel.IsSessionRunning()
			|| GameManager.GetInstance().DebugMode == true)
		{
			if (currentCooldown > 0)
			{
				currentCooldown -= delta;
			}
			else
			{
				totalMovementTime += delta;
				movementProgress = Mathf.Clamp(totalMovementTime / GetMovementDuration(), 0f, 1f);
				if (isPulling)
				{
					Pulling(delta);
				}
				else
				{
					Pushing(delta);
				}
			}
		}
	}

	void Pushing(float delta) {
		float frameDelta = !_hasCreeped ? delta * _creepSpeedMultiplier : delta;
		float moveStep = pushSpeed * frameDelta;

		if (!_hasCreeped)
		{
			Vector3 upwardPosition = bonkerCurrent.GlobalPosition;
			upwardPosition.Y += moveStep;
			bonkerCurrent.GlobalPosition = upwardPosition;
			
			Vector3 increasedScale = bonkerCurrent.Scale * 1.1f; // 10% scale increase
			bonkerCurrent.Scale = bonkerCurrent.Scale.MoveToward(increasedScale, moveStep * 2f);
		}
		else
		{
			bonkerCurrent.GlobalPosition = bonkerCurrent.GlobalPosition.MoveToward(bonkerEnding.GlobalPosition, moveStep);
			bonkerCurrent.Scale = bonkerCurrent.Scale.MoveToward(bonkerEnding.Scale, moveStep);
		}

		float totalDistance = bonkerStarting.GlobalPosition.DistanceTo(bonkerEnding.GlobalPosition);
		float traveledDistance = bonkerStarting.GlobalPosition.DistanceTo(bonkerCurrent.GlobalPosition);
		movementProgress = Mathf.Clamp(traveledDistance / totalDistance, 0f, 1f);

		if (!_hasCreeped && movementProgress >= _creepThreshold)
			_hasCreeped = true;

		if (_hasCreeped && HasReachedTarget(bonkerEnding.GlobalPosition, bonkerEnding.Scale)) {
			ResetMovement();
			currentCooldown = cooldownTimer;
			isPulling = true;
			AffectSled();
			_hasCreeped = false;
		}
	}

	void Pulling(float delta) {
		MoveTowardTarget(bonkerStarting.GlobalPosition, bonkerStarting.Scale, delta, pullSpeedCurve, pullSpeed);

		if (HasReachedTarget(bonkerStarting.GlobalPosition, bonkerStarting.Scale)) {
			ResetMovement();
			currentCooldown = cooldownTimer;
			isPulling = false;
		}
	}

	void MoveTowardTarget(Vector3 targetPos, Vector3 targetScale, float delta, Curve speedCurve, float speed) {
		float speedMultiplier = speedCurve.Sample(movementProgress);

		float currentSpeed = speed * speedMultiplier;

		bonkerCurrent.GlobalPosition = bonkerCurrent.GlobalPosition.MoveToward(
			targetPos, currentSpeed * delta);

		bonkerCurrent.Scale = bonkerCurrent.Scale.MoveToward(
			targetScale, currentSpeed * delta);
	}

	void ResetMovement() {
		movementProgress = 0f;
		totalMovementTime = 0f;
	}

	float GetMovementDuration() {
		float distance = bonkerStarting.GlobalPosition.DistanceTo(bonkerEnding.GlobalPosition);
		float currentSpeed = isPulling ? pullSpeed : pushSpeed;
		return distance / currentSpeed;
	}

	bool HasReachedTarget(Vector3 targetPos, Vector3 targetScale) {
		float posDistance = bonkerCurrent.GlobalPosition.DistanceTo(targetPos);
		float scaleDistance = (bonkerCurrent.Scale - targetScale).Length();

		return posDistance < 0.1f && scaleDistance < 0.1f;
	}

	private void PlayBumpSound() {
		if (bumpSound != null && ds.GlobalPosition.DistanceTo(GlobalPosition) < activationDistance) {
			AudioStreamPlayer player = new AudioStreamPlayer();
			player.Stream = bumpSound;
			AddChild(player);
			player.Play();
			player.Finished += player.QueueFree;
		}
	}

	void AffectSled()
	{
		PlayBumpSound();
		ds._camera?.ActivateCamShake(10);
	}
}