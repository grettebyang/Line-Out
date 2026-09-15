using Godot;
using System;

public partial class SimplePlayerController : CharacterBody3D
{
    [Export]
    public int Speed { get; set; } = 14;
    // The downward acceleration when in the air, in meters per second squared.
    [Export]
    public int FallAcceleration { get; set; } = 75;
    [Export] public HealthComponent _health;

    private Vector3 _targetVelocity = Vector3.Zero;

    // Input vars
    public Vector2 _inputMovement;

    // Controller variables 
    protected int _id;
    protected InputSystemController _intputController;

    //------------------------------------------
    // Override
    //------------------------------------------

    public override void _Ready()
    {
        //SetInputController(new InputSystemController(), 0);

        InputSystem inpSys = InputSystem.GetInstance();
        inpSys.ActivateSingleInputMode(EInputSystemMode.PLAYER_MOVEMENT);
        //SetInputController(inpSys.GetActiveControllerSlot(0), 0);

        inpSys.OnControllerActivate += SetInputController;
        //inpSys.
    }
    public override void _Process(double delta)
    {
        if (_health.IsDead())
        {
            // Test death
            Scale = new Vector3(0.75f, 0.25f, 0.5f);
            _inputMovement = new Vector2(0, 0);
            return;
        }

        float x = _intputController.GetInputAxis("left", "right");
        float y = _intputController.GetInputAxis("back", "front");

        _inputMovement = new Vector2(x, y);
    }

    public override void _PhysicsProcess(double delta)
    {
        // We create a local variable to store the input direction.
        var direction = Vector3.Zero;

        direction.X = _inputMovement.X;
        direction.Z = _inputMovement.Y;

        Velocity = direction * Speed;
        MoveAndSlide();
    }

    //------------------------------------------
    // Public API
    //------------------------------------------

    public void SetInputController(InputSystemController controller, int id)
    {
        _intputController = controller;
        _id = id;
    }
}
