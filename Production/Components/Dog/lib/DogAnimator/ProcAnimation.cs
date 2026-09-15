using Godot;
using Godot.Collections;
using System;

public partial class ProcAnimation : Node3D
{
    [Export] public string Tag;
    [Export] float _transitionTime;

    public float TransitionTime
    {
        get { return _transitionTime; }
    }

    Array<TransformAnimator> _animators = new Array<TransformAnimator>();

    private float _duration;

    public float Duration
    {
        get { return _duration; }
        set
        {
            _duration = value;
            foreach (TransformAnimator anim in _animators)
            {
                anim.Duration = _duration;
            }
        }
    }

    bool _isLerping = false;
    public bool IsLerping
    {
        get { return _isLerping; }
        set
        {
            _isLerping = value;
            foreach (TransformAnimator anim in _animators)
            {
                anim.IsLerping = _isLerping;
            }
        }
    }

    public override void _Ready()
    {
        foreach (Node node in GetChildren())
        {
            if (node is TransformAnimator animator)
            {
                animator.IsPlaying = false;
                _animators.Add(animator);
            }
        }
    }

    public void ProgressAnimation(float add)
    {
        foreach (TransformAnimator anim in _animators)
        {
            float move = add;
            if (anim.PlaybackSpeedCap != -1)
            {
                move = Mathf.Clamp(add, -Mathf.Abs(add), anim.PlaybackSpeedCap);
                //DebugDraw2D.SetText("Anim: " + Name + ", move: " + move);
            }

            anim.PlaybackTime += move;
        }
    }

    public void CancelAnimation()
    {
        foreach (TransformAnimator anim in _animators)
        {
            anim.CancelAnimation();
        }
    }
}
