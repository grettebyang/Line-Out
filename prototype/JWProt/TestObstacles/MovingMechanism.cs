using Godot;
using System;

public partial class MovingMechanism : Node3D
{
    [Export]
    Node3D movePoint;
    [Export]
    public bool isPlaying;
    [Export]
    public bool loop;
    [Export]
    public float timeScale = 1;
    [Export]
    public bool inverted = false;
    [Export]
    Vector3 targetPosition;
    [Export]
    Vector3 targetRotation;
    [Export]
    Curve moveCurve;
    [Export]
    float curveStartingPoint = 0;
    [Export]
    Node3D motionTrackObject;

    [ExportGroup("Hit")]
    [Export]
    Area3D hitArea;
    [Export]
    int hitForceModifier = 50;
    [Export]
    float minHitSpeed;

    float timer;
    Vector3 originalPosition;
    Vector3 originalRotatition;
    Vector3 direction;
    float motionSpeed;

    //--------------------------------------------------------------------------
    /**
    */
    public override void _Ready()
    {
        if (Engine.IsEditorHint())
        {
            return;
        }
        
        if (this.movePoint == null)
        {
            this.movePoint = this;
        }

        this.originalPosition = this.movePoint.GlobalPosition;
        this.originalRotatition = this.movePoint.GlobalRotation;

        // Degrees conversion
        float x = Mathf.DegToRad(this.targetRotation.X);
        float y = Mathf.DegToRad(this.targetRotation.Y);
        float z = Mathf.DegToRad(this.targetRotation.Z);
        this.targetRotation = new Vector3(x, y, z);

        if (this.hitArea != null)
        {
            this.hitArea.BodyEntered += this.OnHitAreaEnter;
        }

        // Starting point 
        float sample = this.moveCurve.Sample(this.timer);
        this.movePoint.GlobalPosition = this.originalPosition + this.targetPosition * sample;
        this.movePoint.GlobalRotation = this.originalRotatition + this.targetRotation * sample;
    }

    //--------------------------------------------------------------------------
    /**
    */
    public override void _Process(double delta)
    {
        // Engine tool 
        if (Engine.IsEditorHint())
        {
            DebugDraw3D.DrawSphere(this.movePoint.GlobalPosition, 0.5f, Colors.Red);
            DebugDraw3D.DrawSphere(this.movePoint.GlobalPosition + this.targetPosition, 0.5f, Colors.Blue);
            return;
        }
    }

    //--------------------------------------------------------------------------
    /**
    */
    public override void _PhysicsProcess(double delta)
    {
        // Moving
        Vector3 lastPosition = Vector3.Zero;
        
        if (this.motionTrackObject != null)
        {
            lastPosition = this.motionTrackObject.GlobalPosition;
        }

        if (this.isPlaying)
        {
            if (this.timer < this.moveCurve.MaxDomain)
            {
                this.timer += (float)delta * this.timeScale;
                float sample = 0;

                if (this.inverted)
                {
                    sample = this.moveCurve.Sample(this.moveCurve.MaxDomain - this.timer);
                }
                else
                {
                    sample = this.moveCurve.Sample(this.timer);
                }

                this.movePoint.GlobalPosition = this.originalPosition + this.targetPosition * sample;
                this.movePoint.GlobalRotation = this.originalRotatition + this.targetRotation * sample;
            }
            else
            {
                if (this.loop)
                {
                    this.timer = 0;
                }
                else
                {
                    this.isPlaying = false;
                }
            }
        }

        if (this.motionTrackObject != null)
        {
            this.direction = lastPosition - this.motionTrackObject.GlobalPosition;
            this.motionSpeed = this.direction.Length();
        }
    }

    //--------------------------------------------------------------------------
    /**
    */
    private void OnHitAreaEnter(Node body)
    {
        if (this.motionSpeed < this.minHitSpeed)
        {
            return;
        }

        if (body is RigidBody3D rb)
        {
            rb.LinearVelocity -= this.direction * this.hitForceModifier;
        }
    }

    //--------------------------------------------------------------------------
    /**
    */
    public void Play(bool play, float start = 0, bool invert = false)
    {
        this.isPlaying = play;
        this.timer = start;
        this.inverted = invert;
    }

    //--------------------------------------------------------------------------
    /**
    */
    public float CurrentTime(bool getInverted)
    {
        if (getInverted)
        {
            return this.moveCurve.MaxDomain - this.timer;
        }

        return this.timer;
    }
}
