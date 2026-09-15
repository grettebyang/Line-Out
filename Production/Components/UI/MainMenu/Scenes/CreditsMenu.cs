using Godot;
using System;
using System.Numerics;

public partial class CreditsMenu : SubmenuBase
{
    [Export] private float _rollSpeed;
    [Export] private float _rollSpeedMax;
    private Godot.Vector2 _rollVelocity;
    private Godot.Vector2 _inputRollVelocity;
    private Godot.Vector2 _creditsStartPos;
    private Godot.Vector2 _creditsRestartPos;

    private PlayerCurserContainer _playerCursorContainer;

    public override void _Ready()
    {
        base._Ready();
        _rollVelocity = new Godot.Vector2(0.0f, _rollSpeed);
        _inputRollVelocity = new Godot.Vector2(0.0f, 0.0f);
        _creditsStartPos = _mainButtonsContainer.Position;
        _creditsRestartPos = new Godot.Vector2(_mainButtonsContainer.Position.X, GetWindow().Size.Y);
        _playerCursorContainer = PlayerCurserContainer.GetInstance();
    }

    public override void Open()
    {
        base.Open();
        _playerCursorContainer.Visible = false;
        _playerCursorContainer.SetCursorsFrozen(true);
        _mainButtonsContainer.Position = _creditsStartPos;
        _openDelayTimer = 2f;
    }

    public override void Close()
    {
        _playerCursorContainer.Visible = true;
        _playerCursorContainer.SetCursorsFrozen(false);
        base.Close();
    }

    public void UpdateRollSpeed(float newSpd)
    {
        _rollSpeed = newSpd;
        _rollVelocity.Y = _rollSpeed;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if(_openDelayTimer > 0)
        {
            _openDelayTimer -= (float)delta;
            return;
        }   
        
        // Input
        float maxInput = 0.0f;
        foreach(PlayerCurser cursor in _playerCursorContainer.CurserList)
        {
            float inputStrength = cursor.GetController().GetInputAxis("south", "north");
            if(maxInput <= Math.Abs(inputStrength))
            {
                _inputRollVelocity.Y = inputStrength * _rollSpeedMax;
                maxInput = Math.Abs(inputStrength);
            }
        }

        _mainButtonsContainer.Position -= (_rollVelocity + _inputRollVelocity) * (float)delta;

        // Loop roll
        if(_mainButtonsContainer.Position.Y < -_mainButtonsContainer.Size.Y)
        {
            _mainButtonsContainer.Position = new Godot.Vector2(_mainButtonsContainer.Position.X, Size.Y);
        }

    }

}
