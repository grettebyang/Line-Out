using Godot;
using System;

public partial class SpeedArea : Node3D
{
    [Export] float _speedMulitplayer = 5;
    [Export] float _effectDuration = 5;
    [Export] float _airDuration = 2;
    [Export] Area3D area;

    float _sledWeight;
    float _sledGravity;
    float _dogGravity;

    float _effectTimer = 0;
    float _airTimer = 0;

    bool _forceGravityScale = false;

    DogSled sled;

    public override void _Ready()
    {
        sled = GameManager.GetInstance().CurrentLevel._sled;

        area.BodyEntered += OnEnter;
    }

    public override void _Process(double delta)
    {
        DebugDraw3D.DrawBox(GlobalPosition, GlobalBasis.GetRotationQuaternion(), new Vector3(1, 1, 1), Colors.AliceBlue);

        if (_effectTimer > 0)
        {
            _effectTimer -= (float)delta;

            if (_effectTimer <= 0)
            {
                // End effect
                foreach (DogController dog in sled.DogSpawner.DogList)
                {
                    dog._hover = true;
                    dog._hoverHeight = 1f;
                }

                _airTimer = _airDuration;
                sled.Hover();
            }
        }

        // Air time
        if (_airTimer > 0)
        {
            _airTimer -= (float)delta;

            if (_airTimer <= 0)
            {
                // End effect
                foreach (DogController dog in sled.DogSpawner.DogList)
                {
                    dog.SpeedBoostEnd(0);
                    dog._hover = false;
                }

                sled.HoverEnd();
                _forceGravityScale = true;
                //sled.GravityScale = 8;
            }
        }

        // Adjust gravity scale
        if (_forceGravityScale)
        {
            if (sled.isSledGrounded)
            {
                sled.HoverEnd();
                _forceGravityScale = false;
            }
        }
    }


    void OnEnter(Node3D body)
    {
        if (_effectTimer > 0 || _airTimer > 0)
        {
            return;
        }

        if (body is DogController dogCollider)
        {
            DogSled sled = GameManager.GetInstance().CurrentLevel._sled;

            // Dogs
            foreach (DogController dog in sled.DogSpawner.DogList)
            {
                dog.SpeedBoost(_speedMulitplayer);
            }

            _effectTimer = _effectDuration;
        }
    }
}
