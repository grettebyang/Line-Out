using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;


public partial class RacingNpc : BaseCharacter {

    public enum AI_MODE {
        DEFAULT,
        ATTACK
    }

    public AI_MODE AiMode = AI_MODE.DEFAULT;

    private float _timeElapsedStuck = 0;
    private Vector3 _lastPosition = Vector3.Zero;
    public Node3D CurrentPath;
    private List<Node3D> _pathList = new List<Node3D>();
    private bool tryingToUnstuck = false;

    public override void _Ready() {
        base._Ready();
        MaxSpeed *= 1.01f;
    }


    protected override void DefaultStateProcess(double delta) { 
        base.DefaultStateProcess(delta);
        AI(delta);
    }

    protected override void FinishedState(double delta) {
        base.FinishedState(delta);

        // Jump and spin when finished
        Jump();
        RotateY(Mathf.Pi / 180);
        _jumpForce += GetGravity() * (float)delta;
        _jumpForce = _jumpForce.Clamp(GetGravity(), new Vector3(0, MaxJumpHeight, 0));
        Velocity = _jumpForce;
        MoveAndSlide();
    }
    public void Jump() {
        HandleJump();
    }


    //------------------------------------------
    // AI and Steering
    //------------------------------------------

    private void AI(double delta) {
        CheckIfReachedPath();
        MoveTowardsPoint();
        LookAhead();
        LookForwardSide();
        CheckForPlayer();
        CheckIfStuck(delta);
    }

    private void CheckForPlayer() {
        DogSled playerSled = GameManager.GetInstance().CurrentLevel._sled;
        List<BaseCharacter> sledDogs = playerSled.GetAttachedCharacters();

        float maxSideDistance = 10f;

        bool drawDebugLines = false;

        Vector3 forward = -GlobalBasis.Z.Normalized();
        Vector3 right = GlobalBasis.X.Normalized() * maxSideDistance;
        float thresholdBack = -1f;
        float thresholdFront = 1f;

        int nrOfTargetedDogs = 0;

        foreach (DogController dog in sledDogs) {
            float dogForwardDot = -GlobalBasis.Z.Dot(dog.GlobalPosition - GlobalPosition);
            float dogRightDot = GlobalBasis.X.Dot(dog.GlobalPosition - GlobalPosition);
            if (dogForwardDot > thresholdBack && dogForwardDot < thresholdFront && dogRightDot < maxSideDistance && dogRightDot > -maxSideDistance) {
                // Move towards the player
                nrOfTargetedDogs++;
                float inputLength = _inputVector.Length();
                _inputVector += ( dog.GlobalPosition - GlobalPosition ).Normalized() * 0.5f;
                _inputVector = _inputVector.Normalized() * inputLength;

                if (drawDebugLines) {
                    DrawPlayerCheckLine(forward, right, thresholdBack, Colors.Green);
                    DrawPlayerCheckLine(forward, right, thresholdFront, Colors.Green);
                }
            }
            else {
                if (drawDebugLines) {
                    // Draw the lines in red if the dog is not within the thresholds
                    DrawPlayerCheckLine(forward, right, thresholdBack, Colors.Red);
                    DrawPlayerCheckLine(forward, right, thresholdFront, Colors.Red);
                }
            }
        }

        if (nrOfTargetedDogs > 0) {
            AiMode = AI_MODE.ATTACK;
        }
        else {
            AiMode = AI_MODE.DEFAULT;
        }
    }

    private void DrawPlayerCheckLine(Vector3 forward, Vector3 right, float threshold, Color color) {
        // The center of the threshold plane
        Vector3 thresholdCenter = GlobalPosition + forward * threshold;
        // Draw a line segment 2 units wide (1 unit to each side)
        float halfWidth = 1.0f;
        Vector3 lineStart = thresholdCenter - right * halfWidth;
        Vector3 lineEnd = thresholdCenter + right * halfWidth;

        DebugDraw3D.DrawLine(lineStart, lineEnd, color);
    }

    private void CheckIfStuck(double delta) {
        if ((_lastPosition - GlobalPosition).Length() < 0.025f) {
            _timeElapsedStuck += (float)delta;
        }
        else {
            _timeElapsedStuck = 0;
        }

        if (!tryingToUnstuck) {
            _lastPosition = GlobalPosition;
        }

        if (tryingToUnstuck && IsOnFloor()) {
            tryingToUnstuck = false;
        }

        if (_timeElapsedStuck > 1f && _timeElapsedStuck < 2.5f) {
            tryingToUnstuck = true;
            Jump();
        }

        if (_timeElapsedStuck > 2.5f) {
            Unstuck();
        }
    }

    private void Unstuck() {
        Vector3 directionTowardsSled = (GlobalPosition - Sled.GlobalPosition).Normalized();
        directionTowardsSled.Y = 0; // Ignore vertical component
        if (directionTowardsSled.Length() < 0.01f)
            directionTowardsSled = Vector3.Forward;
        GlobalPosition += directionTowardsSled * 0.5f;
    }

    public void SetPathList(List<Node3D> pathList) {
        _pathList = pathList;
    }

    public Node3D ReachedCurrentPath(Node3D currentPath) {
        int currentPathIndex = currentPath.GetIndex();

        if (currentPathIndex < _pathList.Count - 1) {
            return _pathList[currentPathIndex + 1];
        } else {
            // Has reached end
            GD.Print("Racer reached end");
            State = CharacterState.FINISHED;
            return null;
        }
    }

    private void CheckIfReachedPath() {

        if ((GlobalPosition - CurrentPath.GlobalPosition).Length() < 2) {
            Node3D currentPath = CurrentPath;
            currentPath = ReachedCurrentPath(CurrentPath);
            if (currentPath != null) {
                CurrentPath = currentPath;
                if (Sled is RacingNPCSled npcSled) {
                    npcSled.CurrentPath = CurrentPath;
                }
            }
            else {
                // The last checkpoint has been reached
                State = CharacterState.FINISHED;
            }
        }
    }

    private void MoveTowardsPoint() {
        if (CurrentPath != null) {
            Vector3 targetPosition = CurrentPath.GlobalPosition;
            Vector3 directionToTarget = ( targetPosition - GlobalPosition ).Normalized();
            directionToTarget.Y = 0;
            _inputVector = directionToTarget;
        }
    }

    private void LookAhead() {
        Vector3 position = GlobalPosition + new Vector3(0, 0.3f, 0);
        Vector3 rayStart = position;
        Vector3 rayEnd = position - GlobalBasis.Z * 0.5f;

        //DebugDraw3D.DrawLine(rayStart, rayEnd, Colors.Red);
        var dogsRids = new Godot.Collections.Array<Rid>();

        List<BaseCharacter> sledDogs = Sled.GetAttachedCharacters();

        foreach (BaseCharacter dog in sledDogs) {
            dogsRids.Add(dog.GetRid());
        }

        var result = Raycast(rayStart, rayEnd, dogsRids);

        if (result.Count > 0) {
            Jump();
        }
    }

    private void LookForwardSide() {
        Vector3 position = GlobalPosition + new Vector3(0, 0.3f, 0);

        Vector3 rayStart = position;
        Vector3 rayEndRight = position - GlobalBasis.Z * 0.6f + GlobalBasis.X * 0.3f; // right
        Vector3 rayEndLeft = position - GlobalBasis.Z * 0.6f - GlobalBasis.X * 0.3f; // left

        DogSled playerSled = GameManager.GetInstance().CurrentLevel._sled;
        var sledRid = new Godot.Collections.Array<Rid>();
        sledRid.Add(playerSled.GetRid());

        var resultRight = Raycast(rayStart, rayEndRight, sledRid);
        var resultLeft = Raycast(rayStart, rayEndLeft, sledRid);

        float xAdjust = 0f;

        if (resultRight.Count > 0 && resultRight.ContainsKey("position")) {
            Vector3 hitPos = (Vector3)resultRight["position"];
            Vector3 offset = hitPos - GlobalPosition;
            float localX = offset.Dot(GlobalBasis.X.Normalized());
            xAdjust -= localX * 2;
        }
        if (resultLeft.Count > 0 && resultLeft.ContainsKey("position")) {
            Vector3 hitPos = (Vector3)resultLeft["position"];
            Vector3 offset = hitPos - GlobalPosition;
            float localX = offset.Dot(GlobalBasis.X.Normalized());
            xAdjust -= localX * 2;
        }

        if (Math.Abs(xAdjust) > 0.01f) {
            float length = _inputVector.Length();
            _inputVector = GlobalBasis.X * xAdjust;
            _inputVector = _inputVector.Normalized() * length;
        }
    }
}

