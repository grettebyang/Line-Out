using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class CheckpointManager : Node3D
{
    private List<Checkpoint> _checkpoints = new List<Checkpoint>();

    [Export] public Checkpoint InitialCheckpoint;
    private Checkpoint _currentCheckpoint;
    private Checkpoint _nextCheckpoint;

    public Checkpoint CurrentCheckpoint { get => _currentCheckpoint; }
    public Checkpoint NextCheckpoint { get => _nextCheckpoint; }
    
    [Signal] public delegate void OnEnteredEventHandler(Checkpoint checkpoint);

    // Used so the camera knows where to look

    public override void _Ready()
    {
        InitiateCheckpoints();

        if (InitialCheckpoint != null)
        {
            InitialCheckpoint.Trigger();
        }
    }

    // Call when any dog passes checkpoint, does nothing currently
    private void OnCheckpointPassed(Checkpoint checkpoint)
    {
        // Heal sled damage
        DogSled sled = GameManager.GetInstance().CurrentLevel._sled;
        if (sled != null)
        {
            PackageCarrier carrier = sled.PackageCarrier;
            carrier.Health.Health += 20;
        }

        _currentCheckpoint = checkpoint;
        _nextCheckpoint = null;

        for (int i = 0; i < _checkpoints.Count - 1; i++)
        {
            if (_currentCheckpoint == _checkpoints[i])
                _nextCheckpoint = _checkpoints[i + 1];
        }

        EmitSignal(nameof(OnEntered), checkpoint);
    }

    //------------------------------------------
    // Public
    //------------------------------------------

    // Check all children nodes that are checkpoints
    public void InitiateCheckpoints() {
        _checkpoints.Clear();

        Array<Node> children = GetChildren();
        foreach (Node3D child in children) {
            Checkpoint checkpoint = child as Checkpoint;
            if (checkpoint == null)
                continue;
            
            // Setup callbacks and add
            checkpoint.OnEntered += OnCheckpointPassed;
            _checkpoints.Add(checkpoint);
        }
    }
}
