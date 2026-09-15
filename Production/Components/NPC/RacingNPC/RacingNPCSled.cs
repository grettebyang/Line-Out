using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Xml.Serialization;
public partial class RacingNPCSled : BaseSled<RacingNpc> {

    [ExportGroup("AI")]
    [Export] public PackedScene RacingNPCScene { get; set; }
    [Export] public Node3D CurrentPath;
    [Export] public Node3D AllPaths;
    [Export] public float CheatingDistance = 15f;
    private LevelManager _levelManager { get; set; }
    private float _timeElapsedStuck = 0;
    private Vector3 _lastPosition = Vector3.Zero;
    private List<Node3D> _pathList = new List<Node3D>();
    private bool _active = false;
    private float _damagedTimer = 0;

    // This is temporary so that this does not desturb anyone else
    public override void _Ready() {
        base._Ready();
        _levelManager = GameManager.GetInstance().CurrentLevel;
        _levelManager.OnStart += ActivateRacingNPC;
        SetupRacingNPC();
    }
    public override void _Process(double delta) {
        base._Process(delta);
        if (_active && BasicTimeManager.GetInstance().GameSpeed != 0) {
            delta *= BasicTimeManager.GetInstance().GameSpeed;
            CheckIfHasReachedEnd();
            CheckIfStuck(delta);
            CatchupSpeedBoost();
        }
    }

    public override void _PhysicsProcess(double delta) {
        if (_active && BasicTimeManager.GetInstance().GameSpeed != 0) {
            base._PhysicsProcess(delta);
            delta *= BasicTimeManager.GetInstance().GameSpeed;

            if (_damagedTimer > 0) {
                _damagedTimer -= (float)delta;
            }
            else {
                _damagedTimer = 0;
            }

        }
    }

    public override void _ExitTree() {
        _pathList.Clear();
        _levelManager.OnStart -= ActivateRacingNPC;
        base._ExitTree();
        foreach (RacingNpc racingNpc in _attachedCharacters) {
            racingNpc.QueueFree();
        }

        if (GameManager.GetInstance().CurrentLevel != null)
            GameManager.GetInstance().CurrentLevel.OnRespawn -= Respawn;
    }

    private void ActivateRacingNPC() {
        if (_active) {
            return;
        }

        _active = true;
        foreach (RacingNpc racingNpc in _attachedCharacters) {
            racingNpc.State = BaseCharacter.CharacterState.DEFAULT;
        }
    }

    private void SetupRacingNPC() {
        AddPaths();
        CreateDogs();
    }

    public override void Respawn() {
        base.Respawn();
        GlobalPosition += GlobalBasis.X * 5;
    }

    // Exists for debugging purposes
    private float _previousSpeedAdjustment = 1;

    private void CatchupSpeedBoost() {
        DogSled sled = GameManager.GetInstance().CurrentLevel._sled;
        int closestPathIndex = GetClosestPathIndex(GlobalPosition);
        int playerPathIndex = GetClosestPathIndex(sled.GlobalPosition);
        float distanceToPlayer = DistanceBetweenPaths(_pathList[closestPathIndex], _pathList[playerPathIndex]) + DistancePastPath(sled.GlobalPosition, playerPathIndex) - DistancePastPath(GlobalPosition, closestPathIndex);

        float speedAdjustment = 1;
        float speedBoostCap = 3f;
        float speedLimitCap = 1.3f;

        if (distanceToPlayer > CheatingDistance) {
            foreach (RacingNpc racingNpc in _attachedCharacters) {
                speedAdjustment = Mathf.Min(distanceToPlayer / CheatingDistance, speedBoostCap);
            }
        }

        if (distanceToPlayer < -CheatingDistance) {
            foreach (RacingNpc racingNpc in _attachedCharacters) {
                speedAdjustment = Mathf.Max(1 / Mathf.Pow(( distanceToPlayer / -CheatingDistance ), 0.5f), speedLimitCap);
            }
        }

        GlobalSpeedAdjustment = Mathf.Clamp(speedAdjustment/_attachedCharacters.Count, speedLimitCap, speedBoostCap);

        // Only for debugging purposes
        if (GlobalSpeedAdjustment > _previousSpeedAdjustment + 0.05f || GlobalSpeedAdjustment < _previousSpeedAdjustment - 0.05f || (GlobalSpeedAdjustment == speedBoostCap && _previousSpeedAdjustment != speedBoostCap) || ( GlobalSpeedAdjustment == speedLimitCap && _previousSpeedAdjustment != speedLimitCap )) {
            _previousSpeedAdjustment = GlobalSpeedAdjustment;
            GD.Print($"SpeedAdjustment: {GlobalSpeedAdjustment}");
        }
    }

    private float DistancePastPath(Vector3 position, int pathIndex) {
        if (pathIndex == 0) {
            return 0;
        }

        Vector3 pathDirection = (_pathList[pathIndex].GlobalPosition - _pathList[pathIndex - 1].GlobalPosition).Normalized();
        pathDirection.Y = 0;
        pathDirection = pathDirection.Normalized();
        float distPastPath = pathDirection.Dot(position - _pathList[pathIndex].GlobalPosition);
        //if (distPastNode > 0)
        //    DebugDraw3D.DrawLine(_pathList[nodeIndex].GlobalPosition, _pathList[nodeIndex].GlobalPosition + nodeDirection * 2, Colors.Green);
        //else
        //    DebugDraw3D.DrawLine(_pathList[nodeIndex].GlobalPosition, _pathList[nodeIndex].GlobalPosition + nodeDirection * 2, Colors.Red);
        
        return distPastPath;

    }

    // From path1 to path2
    private float DistanceBetweenPaths(Node3D path1, Node3D path2) {
        if (path1 == null || path2 == null) {
            GD.PrintErr("One of the paths is null");
            return 0;
        }
        if (path1 == path2) {
            return 0;
        }

        float distance = 0;
        int index1 = _pathList.FindIndex(path => path == path1);
        int index2 = _pathList.FindIndex(path => path == path2);

        int isAheadAdjustment = 1;

        if (index1 > index2) {
            isAheadAdjustment = -1;
        }

        int minimumIndex = Mathf.Min(index1, index2);
        int currentIndex = Mathf.Max(index1, index2);

        if (currentIndex == 0) {
            return 0;
        }

        while (currentIndex > minimumIndex) {
            distance += ( _pathList[currentIndex].GlobalPosition - _pathList[currentIndex - 1].GlobalPosition ).Length();
            currentIndex--;
        }

        return distance * isAheadAdjustment;
    }

    private int GetClosestPathIndex(Vector3 position) {
        int closestPathIndex = -1;
        float closestDistance = float.MaxValue;
        for (int i = 0; i < _pathList.Count; i++) {
            float distance = ( _pathList[i].GlobalPosition - position ).Length();
            if (distance < closestDistance) {
                closestDistance = distance;
                closestPathIndex = i;
            }
        }
        if (closestPathIndex == -1) {
            GD.PrintErr("Closest path index not found");
        }

        //DebugDraw3D.DrawSphere(_pathList[closestPathIndex].GlobalPosition, 0.5f, Colors.Yellow);

        return closestPathIndex;
    }

    private void CreateDogs() {

        if (_attachedCharacters.Count != 0) {
            return;
        }

        int dogAmount = _levelManager._sled.DogSpawner.DogList.Count;
        for (int i = 0; i < dogAmount; i++) {
            RacingNpc racingNpc = RacingNPCScene.Instantiate<RacingNpc>();

            if (racingNpc == null) {
                GD.PrintErr("Failed to instantiate RacingNpc!");
                continue;
            }

            racingNpc.TopLevel = true;

            racingNpc.AddSled(this);
            AddChild(racingNpc);
            _attachedCharacters.Add(racingNpc);

            AttatchRope(racingNpc);
            racingNpc.SetPathList(_pathList);
            racingNpc.CurrentPath = CurrentPath;
            racingNpc.State = BaseCharacter.CharacterState.IDLE;

            AdjustSledToPlayerChange(1);
        }

        PositionCharacters();
        SetAxisVelocity(Vector3.Zero);
        AngularVelocity = Vector3.Zero;
    }

    public void AddPaths() {
        foreach (Node3D path in AllPaths.GetChildren()) {
            _pathList.Add(path);
        }

        if (_pathList.Count == 0) {
            GD.PrintErr("No paths found");
            return;
        }

        CurrentPath = _pathList[0];
    }

    public void CheckIfHasReachedEnd() {
        foreach (RacingNpc racingNpc in _attachedCharacters) {
            if (racingNpc.State != BaseCharacter.CharacterState.FINISHED) {
                return;
            }
        }
        _active = false;
    }

    private void CheckIfStuck(double delta) {
        if (( _lastPosition - GlobalPosition ).Length() < 0.08f * BasicTimeManager.GetInstance().GameSpeed * GlobalSpeedAdjustment) {
            _timeElapsedStuck += (float)delta;
        }
        else {
            _timeElapsedStuck = 0;
        }

        _lastPosition = GlobalPosition;

        if (_timeElapsedStuck > 2.5f) {
            Unstuck();
            _timeElapsedStuck = 0;
        }
    }

    private Vector3 GetDogsGeneralDirection() {
        if (_attachedCharacters.Count == 0) {
            return Vector3.Zero;
        }

        Vector3 cumulativeDirection = Vector3.Zero;
        foreach (RacingNpc racingNpc in _attachedCharacters) {
            cumulativeDirection += ( racingNpc.GlobalPosition - GlobalPosition ).Normalized();
        }

        return cumulativeDirection.Normalized();
    }

    private void Unstuck() {
        Vector3 dir = GetDogsGeneralDirection();
        if (dir.Length() < 0.01f) {
            dir = Vector3.Forward;
        }
        dir.Y = 0; // Ignore vertical component
        GlobalPosition += dir * 2 + Vector3.Up;  
    }

    protected override void HandleDamage(int dmg) {
        bool anyRacerAttacking = false;

        foreach (RacingNpc racingNpc in _attachedCharacters) {
            if (racingNpc.AiMode == RacingNpc.AI_MODE.ATTACK) {
                anyRacerAttacking = true;
            }
        }

        if (anyRacerAttacking) {
            return;
        }

        _damagedTimer += 1f;
        _damagedTimer = Mathf.Clamp(_damagedTimer, 0, 3f);
    }
}

