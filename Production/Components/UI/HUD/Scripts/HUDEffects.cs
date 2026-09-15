using Godot;
using System;

public partial class HUDEffects : CanvasLayer
{
    [ExportGroup("Movement")]
    [Export] protected Control _effectSpeed;
    [Export] protected Control _effectSnow;
    [Export] protected float _speedFullDisplay = 5;
    [Export] protected float _speedShake = 1;
    [Export] protected Curve _speedCurve;
    [Export] protected float _speedLerpWeight = 10;
    [Export] protected float _speedLerpShakeWeight = 5;

    [ExportGroup("Rhythm")]
    [Export] protected Control _effectRhythm;
    [Export] protected float _rhythmLerpWeight = 10;
    [Export] protected Curve _rhythmCurve;

    [ExportGroup("Sled")]
    [Export] protected Control _effectDmg;
    [Export] protected Control _effectHealing;
    [Export] protected Control _mudSplash;
    [Export] protected float _mudSplashClearDelay = 2;

    float _mudSplashClearTimer;

    // References
    protected DogSled _sled;
    protected RhythmManager _rhythm;

    public override void _Ready()
    {
        _sled = GameManager.GetInstance().CurrentLevel._sled;
        _rhythm = RhythmManager.GetInstance();
        _effectSpeed.Modulate = new Color(1, 1, 1, 0);

        _mudSplash.Modulate = new Color(1, 1, 1, 0);
        _sled.OnMudHit += SplashMud;
    }

    public override void _Process(double delta)
    {
        ProcessSpeedEffect((float)delta);
        ProcessSnowEffect((float)delta);
        ProcessRhythmEffect();

        // Mud 
        if (_mudSplash.Modulate.A > 0)
        {
            if (_mudSplashClearTimer <= 0)
                _mudSplash.Modulate = _mudSplash.Modulate.Lerp(new Color(1, 1, 1, 0), (float)delta);
            else
                _mudSplashClearTimer -= (float)delta;
        }
    }

    // Custom
    protected void ProcessSpeedEffect(float delta)
    {
        float speed = _sled.LinearVelocity.Length();
        float a = _speedCurve.Sample(_sled.LinearVelocity.Length() / _speedFullDisplay);
        _effectSpeed.Modulate = _effectSpeed.Modulate.Lerp(new Color(1, 1, 1, a), delta * _speedLerpWeight);

        // Shake
        if (a > 0)
        {
            float randRot = (float)GD.RandRange(-_speedShake, _speedShake) * a;
            _effectSpeed.Rotation = Mathf.Lerp(0, randRot, delta * _speedLerpShakeWeight);

            // Cam shake
            _sled._camera.ActivateCamShake(speed/3f);
        }
    }

    protected void ProcessSnowEffect(float delta)
    {
        float a = 0;

        if (_rhythm._sequenceGo)
        {
            a = 1;

            if (_effectRhythm.Modulate.A < 1)
                a = _rhythmCurve.Sample(_effectRhythm.Modulate.A + delta * _rhythmLerpWeight);
        }
        else
        {
            if (_effectRhythm.Modulate.A > 0)
                a = _rhythmCurve.Sample(_effectRhythm.Modulate.A - delta * _rhythmLerpWeight);
        }


        _effectRhythm.Modulate = _effectRhythm.Modulate.Lerp(new Color(1, 1, 1, a), delta);
    }

    protected void ProcessRhythmEffect()
    {

    }

    public void SplashMud()
    {
        _mudSplash.Modulate = new Color(1, 1, 1, 1);
        _mudSplashClearTimer = _mudSplashClearDelay;
    }
}
