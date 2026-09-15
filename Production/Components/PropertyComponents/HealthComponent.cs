using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class HealthComponent : Area3D
{    
    [Export] protected int _health = 100;
    [Export] protected bool _useMaxHealth = true;
    [Export] protected int _maxHealth = 100;
    [Export] protected int _lowHealthTreshold = 25;

    public int Health{
        get => _health;
        set{
            int prevHealth = _health;
            _health = value;

            int change = _health - prevHealth;

            // Max
            if (_useMaxHealth && _health > _maxHealth)
                _health = _maxHealth;

            EmitSignal(nameof(OnChangeHealth), _health, change);

            // Low
            if (_health <= _lowHealthTreshold){
                EmitSignal(nameof(OnLowHealth));
            }

            // Death
            if (_health <= 0 && _maxHealth > 0){
                EmitSignal(nameof(OnDeath));
            }

            // Resurrect
            if (prevHealth <= 0 && _health > 0){
                EmitSignal(nameof(OnResurrected));
            }
        }
    }

    // Properties
    public bool UseMaxHealth { get => _useMaxHealth; set {_useMaxHealth = value; } }
    public int MaxHealth{ 
        get => _maxHealth; 
        set{
            _maxHealth = value;

            // Fix max health overflow
            if (_useMaxHealth && _health > _maxHealth)
                _health = _maxHealth;
        } 
    }
    public int LowHealthTreshold{ get => _lowHealthTreshold; set { _lowHealthTreshold = value; } }

    // Signals
    [Signal] public delegate void OnChangeHealthEventHandler(int currentHealth, int change);
    [Signal] public delegate void OnDeathEventHandler();
    [Signal] public delegate void OnResurrectedEventHandler();
    [Signal] public delegate void OnLowHealthEventHandler();

    //------------------------------------------
    // Public API
    //------------------------------------------

    public bool IsDead() { return Health <= 0; }

    public void Revive() { Health = MaxHealth; }

    public float HealthPercentage() { return Health / MaxHealth; }
}
