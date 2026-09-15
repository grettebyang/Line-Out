using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class PackageCarrier : Node3D
{
    [Export] protected HealthComponent _health;
    [Export] protected float _lowHealthTreshold;
    //[Export] protected Array<Package> _packages;
    [Export] protected Package _fikaPackage;
    [Export] protected Package _coffeePackage;
    [Export] protected int _largeHitTreshhold = 10;
    [Export] protected AudioStreamPlayer3D _failSound;

    protected bool _initialized = false;
    public List<Collectable> _fikaPickups;

    // Properties
    public HealthComponent Health { get => _health; }

    // Signals
    [Signal] public delegate void OnAddPackageEventHandler();
    [Signal] public delegate void OnRemovePackageEventHandler();

    public override void _Ready(){
        SetPackage(_fikaPackage);
        SetPackage(_coffeePackage);
        GetLevelFikaPickups();
        _health.OnChangeHealth += OnHealthChange;
        _health.OnResurrected += OnHealthRessurected;
    }


    public override void _PhysicsProcess(double delta){
        if (!_initialized)
        {
            _initialized = true;
        }
    }

    protected void OnHealthChange(int currentHealth, int change){
        if (change < 0){
            if (Math.Abs(change) <= _largeHitTreshhold)
            {
                _fikaPackage.PlaySmallHitAnimation();
                _coffeePackage.PlaySmallHitAnimation();
            }
            else
            {
                _fikaPackage.PlayLargeHitAnimation();
                _coffeePackage.PlayLargeHitAnimation();
            }
        }

        if (currentHealth <= 0)
            RemovePackage();
    }

    protected void OnHealthRessurected(){
        _fikaPackage.PlayRestart();
        _coffeePackage.PlayRestart();
    }

    //------------------------------------------
    // Public API
    //------------------------------------------

    /// <summary>
    /// Set package as child and adjust overal health and weight.
    /// </summary>
    /// <param name="package"></param>
    public void SetPackage(Package package){
        // Setup
        _fikaPackage = package;
        _coffeePackage = package;
        //package.GlobalPosition = GlobalPosition;

        _health.MaxHealth = package._health;
        _health.Health = package._health;
        _health.LowHealthTreshold = (int)(_health.MaxHealth * _lowHealthTreshold);

        // Emit 
        EmitSignal(nameof(OnAddPackage));
    }

    /// <summary>
    /// Remove package and remove effects health and weight. Only max health is influenced but current health stays same.
    /// </summary>
    /// <param name="package"></param>
    private void RemovePackage(){
        if(_fikaPackage == null || _coffeePackage == null)
            return;

        _fikaPackage.PlayDestroyAnimation();
        _failSound.Play();

        _coffeePackage.PlayDestroyAnimation();
        _failSound.Play();
    }

    public void RevivePackages(){
        _health.Revive();
    }

    public void GetLevelFikaPickups()
    {
        _fikaPickups = new List<Collectable>();

        if (GameManager.GetInstance().CurrentLevel.Meat is null)
        {
            return;
        }

        var collectables = GameManager.GetInstance().CurrentLevel.Meat.GetChildren();

        foreach (Node3D collectable in collectables)
        {
            _fikaPickups.Add(collectable as Collectable);
        }       
        for(int i = 0; i < _fikaPickups.Count; i++){
			_fikaPickups[i].OnFikaEntered += GetFika;
		}
    }

    public void GetFika()
    {
        // var scene = ResourceLoader.Load<PackedScene>("res://Production/Components/Rhythm/Powerups/TestPackage.tscn").Instantiate();
        // Package p = (Package)scene;
        // AddChild(p);
        // AddPackage(p);
        GD.Print("Meat");
        _health.Health += 25;
    }
}
