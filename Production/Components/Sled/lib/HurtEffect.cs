using Godot;
using System;

public partial class HurtEffect : Control
{
    private float _maxHurtTime = 1f;
    private float _hurtTime;
    private float _hurtTimeCounter;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _hurtTime = _maxHurtTime;
        _hurtTimeCounter = _maxHurtTime;
        ZIndex = -1;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        if (_hurtTimeCounter > 0) {
            _hurtTimeCounter -= (float)delta;
            // Gradually reduce alpha over time
            float alpha = Mathf.Clamp(_hurtTimeCounter / _hurtTime, 0, 1f);
            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, alpha);
        }
        else {
            // Reset hurt effect
            GetParent().RemoveChild(this);
            QueueFree();
        }
    }
}
