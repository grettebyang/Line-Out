using Godot;
using System;

public partial class Package : Node3D
{
    public const int DEFAULT_FALL_TIME = 5;
    public const float GRAVITY = 2;

    [Export] public int _health;
    [Export] public float _sledWeightAdd;
    [Export] private MeshInstance3D _boxMaterialHolder;
    [Export] private AnimationPlayer _animator;
    [Export] private float _hitEffectDelay = 1;

    [ExportGroup("Package Color")]
    [Export] private Color _colorPackageNormal;
    [Export] private Color _colorPackageHit;

    protected float _fallingTime = 0;
    protected Vector3 _fallDirection;
    private Material _boxMaterial;
    private float _hitEffectTimer;

    public override void _Ready(){
        _boxMaterial = _boxMaterialHolder.GetSurfaceOverrideMaterial(0);
    }


    public override void _Process(double delta){
        // Simulate falling 
        if (_fallingTime > 0){
            GlobalPosition = GlobalPosition.Lerp(GlobalPosition + _fallDirection, (float)delta);
            _fallingTime -= (float)delta;

            // Destroy
            /*
            if (_fallingTime <= 0)
                QueueFree();
                */
        }

        if (_hitEffectTimer > 0){
            _hitEffectTimer -= (float)delta;
            Color color = _colorPackageNormal.Lerp(_colorPackageHit, _hitEffectTimer / _hitEffectDelay);
            _boxMaterial.Set("albedo_color", color);
        }
    }


    public void MakeFall(Vector3 direction, float forTime = DEFAULT_FALL_TIME){
        _fallDirection = direction;
        _fallDirection.Y = -GRAVITY;
        _fallingTime = forTime;
    }

    public void PlaySmallHitAnimation(){
        _animator.Play("HitSmall");
        _hitEffectTimer = _hitEffectDelay;
    }

    public void PlayLargeHitAnimation(){
        _animator.Play("HitLarge");
        _hitEffectTimer = _hitEffectDelay * 1.5f;
    }

    public void PlayDestroyAnimation(){
        _animator.Play("Destroyed");
        _hitEffectTimer = 0;
        _boxMaterial.Set("albedo_color", _colorPackageNormal);
    }

    public void PlayRestart(){
         _animator.Play("RESET");
    }
}
