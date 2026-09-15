using Godot;
using System;

public partial class Icicle : Node3D
{
    [Export] RigidBody3D treeRB;
    [Export] int detectionDistance = 8;
    [Export] int fallForce = 250;
    // We want shadow indicator to remain a bit after isicle falls
    [Export] float shadowIndicatorDisplayLag = 3f;
    [Export] Timer timer;
    [Export] RigidBody3D _shadowIndicatorMesh;
    [Export] AudioStreamPlayer3D _shatterSound;
    private bool _falling = false;

    public override void _Ready()
    {
        treeRB.Freeze = true;
        _shadowIndicatorMesh.Freeze = true;
        _shadowIndicatorMesh.Visible = false;

		treeRB.AngularVelocity = new Vector3(0, 0, 0);
        _shadowIndicatorMesh.AngularVelocity = new Vector3(0, 0, 0);

        treeRB.ContactMonitor = true;
        treeRB.MaxContactsReported = 1;
        treeRB.BodyEntered += OnLanded;
        _shadowIndicatorMesh.BodyEntered += OnIndicatorLanded;
    }

    // Called when icicle lands
    void OnLanded(Node body)
    {
        if (!treeRB.Freeze && body is not DogSled && body is not BaseCharacter)
        {
            _shatterSound.Play();
            treeRB.Freeze = true;
            _shadowIndicatorMesh.Visible = false;
        }
    }

    void OnIndicatorLanded(Node body)
    {
        if (!treeRB.Freeze && body is not DogSled && body is not BaseCharacter)
        {
            _shadowIndicatorMesh.Freeze = true;
            _shadowIndicatorMesh.Visible = true;
            timer.Start();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (timer.TimeLeft <= shadowIndicatorDisplayLag)
        {
            _shadowIndicatorMesh.Visible = false;
        }
    }

    void BodyEntered(Node3D body)
    {
        if (body is DogSled sled && GlobalPosition.DistanceTo(sled.GlobalPosition) > detectionDistance)
        {
            // _shadowIndicatorMesh.Freeze = false;
        }
    }

    void timeOut()
    {
        icicleFall();
    }

    void icicleFall()
    {
        treeRB.Freeze = false;
    }
}
