using Godot;
using System;

// Base rock used by other rocks for generic properties
public partial class RockBase : Node3D
{
    [Export] protected Color _rockColor;
    [Export] protected Color _rockLightColor;
    [Export] protected MeshInstance3D _rockModel;
    [Export] protected float _energyStrength = 1f;
    [Export] protected float _minActiveRadiusDistance = 30f;

    protected DogSled ds;
    protected bool _isPlayerInDistance = false;

    // Note: Godot forces you to create duplicates since materials are by default shared (For some stupid reason)
    public override void _Ready()
    {
        ds = GameManager.GetInstance().CurrentLevel._sled;
        StandardMaterial3D mat = _rockModel.GetSurfaceOverrideMaterial(0).Duplicate() as StandardMaterial3D;

        if (mat != null)
        {
            mat.Emission = _rockLightColor;
            mat.EmissionEnergyMultiplier = _energyStrength;
            mat.EmissionEnabled = true;
            mat.AlbedoColor = _rockColor;

            _rockModel.SetSurfaceOverrideMaterial(0, mat);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        isPlayerInDesignatedRadius();
        _energyStrength = calculateForceRatio();
    }

    public float calculateForceRatio()
    {
        float force = GlobalPosition.DistanceTo(ds.GlobalPosition);
        return 1 / force;
    }

    public void isPlayerInDesignatedRadius()
    {
        if (ds.GlobalPosition.DistanceTo(GlobalPosition) < _minActiveRadiusDistance)
            _isPlayerInDistance = true;
        else
            _isPlayerInDistance = false;
    }
}


