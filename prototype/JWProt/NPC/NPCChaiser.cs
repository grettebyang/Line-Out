using Godot;
using System;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Runtime.CompilerServices;

public partial class NPCChaiser : CharacterBody3D, ICrashObstacle
{
    [Export]
    private bool _showDebugGizmos = false;
    [Export]
    private Node3D _dogSled;
    [Export]
    private Node3D _checkpointManager;
    [Export] private HealthComponent _health;
    
    [ExportGroup("Movement")]
    [Export]
    private float _moveSpeed;
    [Export]
    private float _fastMoveSpeed;
    [Export]
    private float _sprintTimerMax = 2;
    [Export]
    private float _considerSprintTimerMax = 3;
    [Export(PropertyHint.Range, "0, 1")]
    private float _sprintActivationChance = 0.5f;

    [ExportGroup("Player detection")]
    [Export]
    private Node3D _target;
    [Export]
    private float _triggerChaiseDistance = 10;
    [Export]
    private float _giveUpDistance = 50;
    [Export]
    private Area3D _triggerChaiseArea;

    [ExportGroup("Obstacle avoiding")]
    [Export]
    private RayCast3D _obstacleRay;
    [Export]
    private int _obstacleRaysCount = 3; // Number for single side = total count of rays will be *2
    [Export]
    private float _obstacleRayAngle = 30; // Angle difference between rays in degrees
    [Export]
    private float _obstacleRayDistance = 3;
    
    private Godot.Vector3 _lastKnownTargetPos;

    private bool _isStuck = false;
    private int _obstacleRayCheckNumber = 0;

    private float _sprintTimer = 0f;
    private float _considerSprintTimer = 0f;
    private bool _crashedInto = false;
    private Godot.Vector3 _crashVel;

    // Invincibility crash code

    public void Crashed(){
        _crashedInto = true;
        _crashVel = new Godot.Vector3(10*GD.Randf(), Math.Abs(10*GD.Randf()), 10*GD.Randf());
    }

    //

    public override void _Ready(){
        // Setup chaise trigger radius
        SphereShape3D chaiseTrigger = _triggerChaiseArea.ShapeOwnerGetShape(0, 0) as SphereShape3D;
        chaiseTrigger.Radius = _triggerChaiseDistance;

        _obstacleRay.TargetPosition = new Godot.Vector3(0, 0, -_obstacleRayDistance);
    }

    public override void _Process(double delta)
    {
        // Editor setup
        if (Engine.IsEditorHint()){
            /*
            Godot.Collections.Array<Node> nodes = EditorInterface.Singleton.GetSelection().GetSelectedNodes();
            
            // Draw if selected
            if (nodes.Contains(this)){
                DebugDraw3D.DrawCylinderAb(GlobalPosition, GlobalPosition + new Godot.Vector3(0,1,0), _triggerChaiseDistance, Colors.GreenYellow);
                DebugDraw3D.DrawCylinderAb(GlobalPosition, GlobalPosition + new Godot.Vector3(0,1,0), _giveUpDistance, Colors.Red);
            }
            */
        }
        else
        {
            // Debug gizmos
            if (_showDebugGizmos)
            {
                DebugDraw3D.DrawCylinderAb(GlobalPosition, GlobalPosition + new Godot.Vector3(0,1,0), _triggerChaiseDistance, Colors.GreenYellow);
                DebugDraw3D.DrawCylinderAb(GlobalPosition, GlobalPosition + new Godot.Vector3(0,1,0), _giveUpDistance, Colors.Red);
            }

            // Draw obstacle check ray
            var angle = _obstacleRay.GlobalRotation.Y - Mathf.DegToRad(90);
            Godot.Vector3 rot = new Godot.Vector3(_obstacleRayDistance * -(float)Math.Cos(angle), 0, _obstacleRayDistance * (float)Math.Sin(angle));
            //DebugDraw3D.DrawArrow(Position, Position + rot, Colors.Red, 0.1f);
        }

        // Process sprinting time
        HandleSprintActivation((float)delta);
    }

    private void HandleSprintActivation(float delta){

        //GD.Print("_considerSprintTimer: " + _considerSprintTimer);

        // Decrease consideration time
        if (_considerSprintTimer > 0){
            _considerSprintTimer -= delta;
            return;
        }
        else{
            if (_sprintTimer <= 0)
            {
                // Consider activating sprint
                float rand = GD.RandRange(0, 1);
                if (rand >= _sprintActivationChance){
                    //GD.Print("Start sprint");
                    _sprintTimer = _sprintTimerMax;
                }
                else{
                    // Restart
                    _considerSprintTimer = _considerSprintTimerMax;
                    return;
                }
            }
        }

        // Sprint
        if (_sprintTimer > 0)
        {
            _sprintTimer -= delta;
        }
        else
        {
            // Restart on sprint depleted
            _considerSprintTimer = _considerSprintTimerMax;
        }
    }

    public override void _PhysicsProcess(double delta){

        Testing(delta);

        //GD.Print("process: " + _target);

        // Check line of sight and update last know position
        if (_target != null){
            _lastKnownTargetPos = _target.GlobalPosition;
            
            float dist = GlobalPosition.DistanceTo(_target.Position);

            if (dist > _giveUpDistance){
                _target = null;
                GD.Print("Give up!");
            }
        }

        // Follow target
        if (_target != null){

            //GD.Print("Rot: " + _obstacleRay.GlobalRotation.Y);
            //AvoidObstacle();
            if (!_isStuck)
                _isStuck = IsStuck();

            if (_isStuck)
                AvoidObstacleCheck();
            else
                FollowTarget(delta);
        }
    }

    public void Testing(double delta){
        // Test hp
        /*
        if (_health.IsDead())
            this.QueueFree();
        */

        // Test DMG 
        if (_target != null && _obstacleRay.GetCollider() == _target){
            SimplePlayerController testChar = _target as SimplePlayerController;
            if (testChar != null){
                testChar._health.Health -= 1;
            }
        }
    }

    private void FollowTarget(double delta){
        // Rorate towards target
        LookAt(_lastKnownTargetPos);
        Rotation = new Godot.Vector3(0, Rotation.Y, 0);

        // Move
        float speed = _moveSpeed;
        if (_sprintTimer > 0)
             speed = _fastMoveSpeed;

        if(_crashedInto){
            Velocity = _crashVel;
        }
        else{
            Velocity = -GlobalTransform.Basis.Z * speed * (float)delta;
            Velocity = Velocity + new Godot.Vector3(0, -10, 0);
        }
        MoveAndSlide();
    }

    private bool IsStuck(){
        // Check front
        _obstacleRay.Rotation = Godot.Vector3.Zero;

        if (!_obstacleRay.IsColliding() || _obstacleRay.GetCollider() == _target){
            //DebugDraw3D.DrawSphere(GlobalPosition, 1, Colors.Green);
            return false;
        }

        _obstacleRay.TargetPosition = new Godot.Vector3(0, 0, -_obstacleRayDistance * 2);
        _obstacleRayCheckNumber++;
        return true;
        /*
        DebugDraw3D.DrawSphere(GlobalPosition, 1, Colors.Red);

        // Check sides if front is blocked
        for (int i = 0; i < _obstacleRaysCount; i++){
            float angle = _obstacleRayAngle * i;

            // Left
            _obstacleRay.Rotation = new Godot.Vector3(0, _obstacleRayAngle, 0);

            if (!_obstacleRay.IsColliding()){
                // Teleport at ray
                Godot.Vector3 jump = new Godot.Vector3(_obstacleRayDistance * (float)Math.Cos(_obstacleRay.Rotation.Y), 0, _obstacleRayDistance * (float)Math.Sin(_obstacleRay.Rotation.Y));
                GD.Print("jump: " + jump);

                GlobalPosition += new Godot.Vector3(_obstacleRayDistance * (float)Math.Cos(_obstacleRay.Rotation.Y), 0, _obstacleRayDistance * (float)Math.Sin(_obstacleRay.Rotation.Y));
                return;
            }
            
            // Right
            _obstacleRay.Rotation = new Godot.Vector3(0, -_obstacleRayAngle, 0);

            if (!_obstacleRay.IsColliding()){
                // Teleport at ray
                Godot.Vector3 jump = new Godot.Vector3(_obstacleRayDistance * (float)Math.Cos(_obstacleRay.Rotation.Y), 0, _obstacleRayDistance * (float)Math.Sin(_obstacleRay.Rotation.Y));
                GD.Print("jump: " + jump);

                GlobalPosition += new Godot.Vector3(_obstacleRayDistance * (float)Math.Cos(_obstacleRay.Rotation.Y), 0, _obstacleRayDistance * (float)Math.Sin(_obstacleRay.Rotation.Y));
                return;
            }
        }
        */
    }

    private void AvoidObstacleCheck()
    {
        // Check current ray rotation
        if (!_obstacleRay.IsColliding()){
            var ang = _obstacleRay.GlobalRotation.Y - Mathf.DegToRad(90);
            Godot.Vector3 jump = new Godot.Vector3(_obstacleRayDistance * -(float)Math.Cos(ang), 0, _obstacleRayDistance * (float)Math.Sin(ang));
            //Godot.Vector3 jump = new Godot.Vector3(_obstacleRayDistance * (float)Math.Cos(_obstacleRay.GlobalRotation.Y), 0, _obstacleRayDistance * (float)Math.Sin(_obstacleRay.GlobalRotation.Y));

            //float ang = _obstacleRayAngle * Mathf.Ceil((float)_obstacleRayCheckNumber * 0.5f);
            //GD.Print("jump: " + jump);
            //GD.Print("Rot: " + Mathf.RadToDeg(_obstacleRay.GlobalRotation.Y));
            //GD.Print("ang: " + ang);

            GlobalPosition += jump;

            // Restart
            _obstacleRay.TargetPosition = new Godot.Vector3(0, 0, -_obstacleRayDistance);
            _obstacleRayCheckNumber = 0;
            _isStuck = false;
            return;
        }
        
        // Update side check
        _obstacleRayCheckNumber++;
        bool even = _obstacleRayCheckNumber % 2 == 0;

        float angle = _obstacleRayAngle * Mathf.Ceil((float)_obstacleRayCheckNumber * 0.5f);

        if (even)
            _obstacleRay.Rotation = new Godot.Vector3(0, -_obstacleRayAngle, 0);
        else
            _obstacleRay.Rotation = new Godot.Vector3(0, _obstacleRayAngle, 0);
    }

    //

    private void OnTriggerChaiseAreaBodyEntered(Node3D body){
        // Set dog as target
        DogController dog = body as DogController;

        if (dog != null){
            _target = body;
            return;
        }

        SimplePlayerController testChar = body as SimplePlayerController;
        if (testChar != null)
            _target = body;
    }
}
