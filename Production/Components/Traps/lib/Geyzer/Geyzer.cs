using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
public partial class Geyzer : TrapBase{
    //some of these should be replaced with particles at some point, specifically big water and small water
    [Export] protected int _dmgAmount = 0;
    [Export] public GpuParticles3D _geyserVFX;
    public Damage _effect = new Damage();

    // [Export]
    // Node3D BigWater;
    [Export]
    float pushVal = 13;
    [Export]
    public AudioStreamPlayer3D _steamSound;
    Vector3 scaleAmount;

    private DogSled ds;
    
    private const float MinDistance = 25 * 25; // Squared distances to allow using LenghtSquared
    private const float MaxDistance = 50 * 50; 
    private const int MaxParticles = 200;  
    private const int MinParticles = 15;

    public override void _EnterTree()
    {
        _active = true;
        _gracePeriod = 2f;
        _effect._source = "Geyzer";
        _effect._amount = _dmgAmount;
        _effect._pushAmountVec = GlobalBasis.Y * pushVal;
        SetDamage(_effect);
        RandomNumberGenerator randomOffset = new RandomNumberGenerator();
        _timer = randomOffset.Randf();
    }

    public override void _Ready()
    {
        base._Ready();
        ds = GameManager.GetInstance().CurrentLevel._sled;
    }
    
    /*
    REFACTORNOTE: 
    Might be good to split into segments and run by manager instead of each object update.
    */

    public override void Always(float delta)
    {
        _timer += delta;

        if ((int)_timer % 3 != 0)
        {
            _geyserVFX.Emitting = false;
            _active = false;
        }
        else
        {
            float distance = (GlobalPosition - ds.GlobalPosition).LengthSquared();
            distance = Mathf.Clamp(distance, MinDistance, MaxDistance);

            float normalizedDistance = (distance - MinDistance) / (MaxDistance - MinDistance);
            int particleCount = (int)Mathf.Lerp(MaxParticles, MinParticles, normalizedDistance);

            _geyserVFX.Amount = particleCount;
            _geyserVFX.Emitting = true;
            PlayAudio();
            _active = true;
        }
    }

    public void PlayAudio()
    {
        if (!_steamSound.Playing)
        {
            _steamSound.Play();
        }
    }
    // public override void CustomCollision(float delta){GD.Print("I am active!");
    // }
}





