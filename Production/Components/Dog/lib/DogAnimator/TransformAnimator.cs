using Godot;
using System;

public partial class TransformAnimator : Node3D
{
    [Export] Node3D _target;
    [Export] float _duration = 1;
    public float Duration
    {
        get { return _duration; }
        set { _duration = value; }
    }

    [Export] bool _isPlaying = true;
    [Export] bool _isLerping = false;
    public bool IsLerping
    {
        get { return _isLerping; }
        set { _isLerping = value; }
    }

    [Export] Vector3 _targetPosition;
    [Export] Curve _positionCurve;

    [Export] Vector3 _targetRotation;
    [Export] Curve _rotationCurve;
    [Export] float _playbackSpeedCap = -1;

    public float PlaybackSpeedCap { get { return _playbackSpeedCap; } }

    Vector3 _originalPosition;
    Vector3 _originalRotation;

    float _playbackTime = 0;
    public float PlaybackTime
    {
        get { return _playbackTime; }
        set
        {
            // Loop
            _playbackTime = value;
            if (_playbackTime >= _duration)
            {
                _playbackTime = _playbackTime - _duration;
            }

            float time = _playbackTime / _duration;

            // Update motion
            if (_positionCurve != null)
            {
                if (!_isLerping)
                {
                    _target.Position = _originalPosition + _positionCurve.Sample(time) * _targetPosition;
                }
                else
                {
                    //DebugDraw2D.SetText("Lerping transform!");
                    _target.Position = _target.Position.Lerp(_originalPosition + _positionCurve.Sample(time) * _targetPosition, value);
                }
            }

            if (_rotationCurve != null)
            {
                if (!_isLerping)
                {
                    _target.Rotation = _originalRotation + _rotationCurve.Sample(time) * _targetRotation;
                }
                else
                {
                    _target.Rotation = _target.Rotation.Lerp(_originalRotation + _rotationCurve.Sample(time) * _targetRotation, time);
                }
            }
        }
    }

    bool _isCanceling = false; // Used for returning back to starting position

    public bool IsPlaying
    {
        get { return _isPlaying; }
        set { _isPlaying = value;  }
    }

    public override void _Ready()
    {
        if (_target == null)
            _target = this;

        _originalPosition = _target.Position;
        _originalRotation = _target.Rotation;

        // Deg to rad
        float x = Mathf.DegToRad(_targetRotation.X);
        float y = Mathf.DegToRad(_targetRotation.Y);
        float z = Mathf.DegToRad(_targetRotation.Z);
        _targetRotation = new Vector3(x, y, z);
    }

    public override void _Process(double delta)
    {
        // Canceling - return to 0 position
        if (_isCanceling)
        {
            if (_playbackTime > 0)
            {
                PlaybackTime -= (float)delta;
            }
            else
            {
                PlaybackTime = 0;
                _isCanceling = false;
                _isPlaying = true;
            }

            return;
        }

        // Playing
        if (_isPlaying)
        {
            PlaybackTime += (float)delta;
        }
    }

    public void CancelAnimation()
    {
        _isCanceling = false;
    }
}
