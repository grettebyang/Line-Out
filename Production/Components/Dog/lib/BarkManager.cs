using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BarkManager : Node3D {
    [Export] public PackedScene BarkParticlesScene { get; set; }
    protected List<GpuParticles3D> _spawnedParticles = new List<GpuParticles3D>();
    private int _lastRestarted = 0;

    // In order to change the spawn position of the bark vfx, it is possible to move BarkManager Node3D in the editor
    // instead of adding extra code
    public void Bark(int id) {
        if(_spawnedParticles.Count >= 10) // to avoid there being an infinite amount of gpuparticles spawned
        {
            _spawnedParticles[_lastRestarted].Restart();
            _spawnedParticles[_lastRestarted].GlobalPosition = GlobalPosition;
            _lastRestarted = (_lastRestarted + 1) % _spawnedParticles.Count;
        }
        else
        {
            GpuParticles3D particles = BarkParticlesScene.Instantiate<GpuParticles3D>();
            _spawnedParticles.Add(particles);
            AddChild(particles);
            particles.Finished += RemoveParticleFromQueue;
            particles.ProcessMaterial.Set("color", UIConstants.PLAYER_COLORS[id]);
            particles.LocalCoords = false;
            particles.GlobalPosition = GlobalPosition;
            particles.Emitting = true;
        }
    }

    public void RemoveParticleFromQueue()
    {
        _spawnedParticles[_lastRestarted].QueueFree();
        _spawnedParticles.RemoveAt(_lastRestarted);
        if(_lastRestarted == _spawnedParticles.Count)
        {
            _lastRestarted = 0;
        }
    }
}