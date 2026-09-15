using Godot;
using Godot.Collections;
using Godot.NativeInterop;
using System;

public enum AvalancheState {
    Static,
    Active
}

// Avalanche needs the DeathZone script manually attatched to it, since we should probably decide by hand what scale the avalanche and its 
// DeathZone should have on a case-by-case basis (Since it's going to be so few of them). It's easiest to just make the DeathZone a script
// Also, deathzone is a rigidbody, so naturally it needs a collisionshape, and within that shape is how we determine where the sled dies/respawns.
public partial class Avalanche : Node3D
{
    [Export] public AvalancheState AvalancheState;

    [ExportGroup("Values")]
    [Export] public float Speed = 5f;
    [Export] public float ActivationRange = 40f;

    // Slow down avalanche if it's close to sleed and apply camera shake
    [Export] public float SlowDownCap = 15f;
    [Export] public Vector3 startingPos;

    [ExportGroup("References")]
    [Export] public DogSled Sled;

    [ExportGroup("Effects")]
    [Export] protected GpuParticles3D _particles;
    [Export] private AudioStreamPlayer3D _avalancheRumble;

    public override void _Ready() {
        startingPos = GlobalPosition;
        _particles.Emitting = false;
        AvalancheState = AvalancheState.Static;
    }

    public override void _Process(double delta){
        switch (AvalancheState) {
            case AvalancheState.Static:
                StaticAvalancheProcess(delta);
                break;
            case AvalancheState.Active:
                ActiveAvalancheProcess(delta);
                break;
            default:
                break;
        }
    }

    // Avalanche can't start if sled is still
    private void StaticAvalancheProcess(double delta) {
        if(Sled == null)
        {
            GD.Print("Sled reference not set up properly to avalanche");
            return;
        }
        // only start avalanche if we're moving (for some reason sled lin vel is above 0 when standing still so i added a bias)
        if (( Sled.GlobalPosition - GlobalPosition ).Length() > ActivationRange && Sled.LinearVelocity.Length() > 2) {
            ActivateAvalanche();
        }
    }

    private void ActiveAvalancheProcess(double delta) {
        if ((GlobalPosition - Sled.GlobalPosition).Length() < 1) {
            DeactivateAvalanche();
            Sled.Respawn();
            GlobalPosition = startingPos;
            return;
        }

        Vector3 dist = Sled.GlobalPosition - GlobalPosition;
        Vector3 _chaseDirection = dist.Normalized();

        if(dist.Length() < SlowDownCap)
        {
            // invert it so more camera shake the closer avalanche is to sled
            Sled._camera.ActivateCamShake(50 * (1 / dist.Length()));
            GlobalPosition += _chaseDirection * (Speed / 2) * (float)delta;
        }
        else if(dist.Length() > SlowDownCap * 2)
        {
            GlobalPosition += _chaseDirection * (Speed * 2) * (float)delta;
        }
        else
        {
            GlobalPosition += _chaseDirection * Speed * (float)delta;
        }
    }

    private void ActivateAvalanche() {
        GD.Print("Avalanche started");
        AvalancheState = AvalancheState.Active;
        _particles.Emitting = true;
        _avalancheRumble.Play();
    }
    public void DeactivateAvalanche() {
        GD.Print("Avalanche stopped");
        AvalancheState = AvalancheState.Static;
        _particles.Emitting = false;
        _avalancheRumble.Stop();
    }
}
