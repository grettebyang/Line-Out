using Godot;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;

public partial class DogSled : BaseSled<DogController> {

	[Export] public ISDogSpawner DogSpawner;
	[Export] public Camera _camera;

	[Signal] public delegate void OnMudHitEventHandler();

	public override void _Ready() {
		base._Ready();
		GameManager.GetInstance().CurrentLevel.OnRespawn += Respawn;
	}

	//Powerup effects
	public void SpeedBoost(float mult)
	{
		SledMaxSpeed *= mult;
		ropePullPower *= 1.5f;
	}

	public void DeleteDog(DogController dog) {
		if (_attachedCharacters.Contains(dog)){
			_attachedCharacters.Remove(dog);
		}
	}

	public void SpeedBoostEnd(float mult)
	{
		SledMaxSpeed = _sledMaxSpeed;
		ropePullPower /= 1.5f;
	}
	public void Hover()
	{
        _hoverHeight = 3.0f;
        _lastYPos = GlobalPosition.Y;
		GravityScale = 0.0f;
		LinearDamp = _hoverDamping;
		//PhysicsMaterialOverride.Friction = 0.0f;
		_hover = true;
		toggleSledRotationXZ(true);
	}
	
	public void GravityHover()
	{
		_hover = true;
		toggleSledRotationXZ(true);
	}
	public void HoverEnd()
	{
		GravityScale = 1.0f;
		LinearDamp = 0.0f;
		//PhysicsMaterialOverride.Friction = 1.0f;
		_hover = false;
		toggleSledRotationCompletely(false);
	}
	public void Invincibility() {

	}

	public override void Respawn() {
		base.Respawn();

		_camera.GlobalPosition = GlobalPosition;
		_camera.respawnCameraAnimation();
	}

	public void InvincibilityEnd() {

	}

	public void AddDog(DogController dog) {
		if (dog == null) {
			return;
		}
		if (_attachedCharacters.Contains(dog)) {
			return;
		}

		dog.SetCamRef(_camera._sledCamera);
		_attachedCharacters.Add(dog);
	}

	public override void AttatchRope(BaseCharacter character) {
		base.AttatchRope(character);
		(character as DogController).SetCamRef(_camera._sledCamera);
	}

	protected override void HandleDamage(int dmg) {
		base.HandleDamage(dmg);

		if (dmg > 0)
		{
			GameManager.GetInstance().CurrentLevel.OnDamageReceived();
			_camera.ActivateCamShake(dmg);
		}
	}

	// Respawn dogs at sled if stuck
	public void RespawnDogsAtSled()
	{
		base.PositionCharacters();
	}
}
