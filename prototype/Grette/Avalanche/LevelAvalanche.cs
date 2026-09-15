using Godot;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public partial class LevelAvalanche : Node3D
{
    [Export] private Area3D _avalancheField;
    [Export] private float _speed;
    [Export] private AudioStreamPlayer3D _avalancheRumble;
    private Vector3 _velocity;
    private Vector3 _startPos;
    private bool _triggered;
    private bool _end;

    [Signal] public delegate void OnAvalancheConsumeEventHandler();

    public override void _Ready()
    {
        base._Ready();
        _velocity = new Vector3(0.0f, 0.0f, _speed); // it will move in the positive z axis
        _startPos = new Vector3(0.0f, 0.0f, 0.0f);
        if(GameManager.GetInstance().CurrentLevel != null)
            GameManager.GetInstance().CurrentLevel.OnRespawn += ResetAvalanche;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        if(GameManager.GetInstance().CurrentLevel != null)
            GameManager.GetInstance().CurrentLevel.OnRespawn -= ResetAvalanche;
    }


    public override void _Process(double delta)
    {
        base._Process(delta);
        if(_triggered && !_end)
        {
            _avalancheField.Position += _velocity;
        }
    }

    public void ResetAvalanche()
    {
        _triggered = false;
        _end = false;
        _avalancheField.Position = _startPos;
        _avalancheRumble.Stop();
    }

    // When a body enters the trigger avalanche area
    public void OnBodyEntered(Node3D body)
    {
        // Trigger avalanche to start moving
        DogController dog = body as DogController;
        if(dog != null && !_triggered)
        {
            _triggered = true;
            _avalancheRumble.Play();
        }
    }

    // When the avalanche reaches the end area
    public void OnAreaEntered(Area3D area)
    {
        if(area == _avalancheField)
        {
            _triggered = false;
            _end = true;
            _avalancheRumble.Stop();
        }
    }

    public void OnAvalancheBodyEntered(Node3D body)
    {
        if(body.GetType() == typeof(DogController) || body.GetType() == typeof(DogSled))
        {
            EmitSignal(nameof(OnAvalancheConsume));
            // alternatively just call level restart from gamemanager.currentlevel
        }
    }
}
