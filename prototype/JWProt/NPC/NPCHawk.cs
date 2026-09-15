using Godot;
using System;
using System.Collections.Generic;

public partial class NPCHawk : Node3D
{
    [Export] public float TriggerRadius { get; set; }
    [Export] public float Speed { get; set; }
    [Export] public float StopDistanceY { get; set; }
    [Export] public float AttackDelay { get; set; }
    [Export] public float DespawnDelay { get; set; }
    [Export] public float MovementAnticipation { get; set; }
    [Export] public float HitDistance { get; set; }
    [Export] public int Damage { get; set; }

    [Export] Node3D model;
    [Export] AudioStreamPlayer3D _sfxScream;
    [Export] AnimationPlayer _animator;

    Node3D _currentTargetObject;
    Vector3 _currentTarget;
    List<Node3D> _targets = new List<Node3D>();
    bool _isTriggered = false;
    bool _hitSuccess = false;
    bool _attackDone = false;
    bool _flyAway = false;
    float _radiusSq;
    float _hitDistanceSq;

    public override void _Ready()
    {
        _radiusSq = TriggerRadius * TriggerRadius;
        _hitDistanceSq = HitDistance * HitDistance;

        // Setup targets
        SetupTargets();

        //ControllersActivator activator = InputSystem.GetInstance().GetControllersActivator();
        //activator.OnConfirm += SetupTargets;

        LevelManager level = GameManager.GetInstance().CurrentLevel;
        level.OnBark += OnBark;
    }

    void SetupTargets()
    {
        _targets.Clear();

        DogSled sled = GameManager.GetInstance().CurrentLevel._sled;
        _targets.Add(sled);

        /*
        foreach (Node3D dog in sled.DogSpawner.DogList)
        {
            _targets.Add(dog);
        }
        */
    }

    public override void _Process(double delta)
    {
        // Rotate to target
        if (_isTriggered || _flyAway)
        {
            model.LookAtFromPosition(GlobalPosition, _currentTarget, GlobalBasis.Z);

            if (!_animator.IsPlaying())
                _animator.Play("Fly");
            //DebugDraw3D.DrawSphere(_currentTarget, 0.5f, Colors.Red);
        }

        // Continue attack after hitting
        if (_hitSuccess && !_flyAway)
        {
            model.Rotation = new Vector3(0, 0, 0);
            if (_animator.CurrentAnimation != "Attack" || !_animator.IsPlaying())
                _animator.Play("Attack");

            if (_currentTargetObject != null)
                GlobalPosition = _currentTargetObject.GlobalPosition + new Vector3(0, 1.5f, 0);
        }

        // Despawn
        if (_attackDone)
        {
            if (DespawnDelay > 0)
            {
                DespawnDelay -= (float)delta;
            }
            else
            {
                _currentTarget = GlobalPosition + new Vector3(0, 5, 0);
                _flyAway = true;
                _animator.Stop();
            }

            return;
        }

        // Debug draw
        Color col = Colors.Red;
        if (_isTriggered)
            col = Colors.Green;

        DebugDraw3D.DrawCylinderAb(GlobalPosition, GlobalPosition + new Vector3(0, -10, 0), TriggerRadius, col);

        if (Engine.IsEditorHint())
        {
            //DebugDraw3D.DrawCylinderAb(GlobalPosition, GlobalPosition + new Vector3(0, -10, 0), TriggerRadius, Colors.Red);
            return;
        }

        if (!_isTriggered)
        {
            // Trigger if target is in radius
            Vector3 myPosition = new Vector3(GlobalPosition.X, 0, GlobalPosition.Z);

            foreach (Node3D target in _targets)
            {
                Vector3 targetPos = new Vector3(target.GlobalPosition.X, 0, target.GlobalPosition.Z);

                if (myPosition.DistanceSquaredTo(targetPos) < _radiusSq)
                {
                    _isTriggered = true;

                    // Pick random target
                    RandomNumberGenerator rn = new RandomNumberGenerator();
                    int targetId = 0;
                    _currentTargetObject = _targets[targetId];
                    _currentTarget = _targets[targetId].GlobalPosition + _targets[targetId].GlobalTransform.Basis.Z * MovementAnticipation;

                    _sfxScream.Play();
                    break;
                }
            }
        }

        if (_isTriggered && AttackDelay > 0)
        {
            AttackDelay -= (float)delta;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_flyAway)
        {
            GlobalPosition = GlobalPosition.Lerp(_currentTarget, Speed * (float)delta);
            return;
        }

        if (!_isTriggered || _attackDone || AttackDelay > 0)
            return;

        // Attack
        GlobalPosition = GlobalPosition.Lerp(_currentTarget, Speed * (float)delta);

        // Stop
        Vector3 diff = _currentTarget - GlobalPosition;
        if (Mathf.Abs(diff.Y) < StopDistanceY)
        {
            _attackDone = true;

            if (GlobalPosition.DistanceSquaredTo(_currentTarget) < _hitDistanceSq)
            {
                _hitSuccess = true;
            }
        }
    }

    public void DamageSled()
    {
        DogSled sled = GameManager.GetInstance().CurrentLevel._sled;
        sled.PackageCarrier.Health.Health -= Damage;
    }

    void OnBark(Vector3 position)
    {
        //if (position.DistanceSquaredTo(GlobalPosition) > 3 * 3)
        //return;

        if (!_isTriggered)
            return;

        // TODO: Need aim direction dot product to check if dog is barking on hawk
        _currentTarget = GlobalPosition + new Vector3(0, 5, 0);

        _hitSuccess = false;
        _flyAway = true;
        _animator.Stop();
    }
}
