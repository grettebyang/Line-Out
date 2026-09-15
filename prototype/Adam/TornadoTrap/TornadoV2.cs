using Godot;
using System;
using System.Net;

public partial class TornadoV2 : Node3D
{
    [Export] private float tangentForce = 400f;
    [Export] private float tangentAmplifier = 4f;
    [Export] private float upforce = 30000f;
    [Export] private float suctionForce = 15000f;
    [Export] private float tornadoRadius = 10;
    [Export] private float flungFromCenterDistance = 2.0f;
    [Export] private float stopTimer = 2.0f; // how long the tornado stops at A or B
    [Export] float tornadoMoveSpeed = 5.0f;
    [Export] private Node3D endpointA;
    [Export] private Node3D endpointB;
    [Export] private AudioStreamPlayer3D tornadoSwirlingSFX;
    private Node3D currGoal;
    DogSled ds;
    Vector3 _tagentForce = Vector3.Zero;
    Vector3 _targetForce = Vector3.Zero;    
    private float cooldownTimer = 0;
    private bool shouldMove;
	BasicTimeManager _timeManager;

    public override void _Ready()
    {
		_timeManager = BasicTimeManager.GetInstance();

        ds = GameManager.GetInstance().CurrentLevel._sled;
        currGoal = endpointB; // Initially starts at endpointA with goal being endpointB
        shouldMove = true;

        cooldownTimer = stopTimer;
    }

    public override void _PhysicsProcess(double delta)
    {
        delta *= _timeManager.GameSpeed;
        if (_timeManager.State != BasicTimeManager.TIMESTATE.MENU)
        {
            TornadoTorque(delta);
            MoveTornado(delta);
        }
    }

    private void MoveTornado(double delta)
    {
        // hardcoded magic number justified by the fact that you can just move the node3d instead of adjusting the '1'.
        if(GlobalPosition.DistanceTo(currGoal.GlobalPosition) <= 1)
        {
            if(cooldownTimer <= 0)
            {
                if(currGoal == endpointA)
                {
                    currGoal = endpointB;                
                }
                else if(currGoal == endpointB)
                {
                    currGoal = endpointA;                
                }
                // start timer
                cooldownTimer = stopTimer;
            }
            else
            {
                cooldownTimer -= (float)delta;              
            }
            return;
        }

        Vector3 dir = (currGoal.GlobalPosition - GlobalPosition).Normalized();
        GlobalPosition += dir * tornadoMoveSpeed * (float)delta;
    }

    public void TornadoTorque(double delta)
    {
        float distance = GlobalPosition.DistanceTo(ds.GlobalPosition);

            foreach (BaseCharacter dog in ds._attachedCharacters)
            {
                // for game feel i believe it is good that dogs are affected before sled so you get some margin to prepare
                if(dog.GlobalPosition.DistanceTo(GlobalPosition) < tornadoRadius * 1.3f)
                {
                    Vector3 toDog = (dog.GlobalPosition - GlobalPosition).Normalized();

                    float suctionIntensity = 1f - (distance / tornadoRadius);
                    Vector3 suction = -toDog * suctionForce * suctionIntensity;

                    float tangentIntensity = distance / tornadoRadius * (1f - distance / tornadoRadius) * tangentAmplifier / 4;
                    Vector3 tangent = toDog.Cross(Vector3.Up).Normalized() * tangentForce * tangentIntensity;
                    dog.GlobalPosition += new Vector3(Mathf.Clamp(tangent.X, 0, 0.09f), 0, Mathf.Clamp(tangent.Z, 0, 0.09f));
                }
            }

        if (distance < tornadoRadius)
        {
            PlaySFX();
            // ensure sled can't flip when in tornado
            ds.GravityHover();

            Vector3 toSled = (ds.GlobalPosition - GlobalPosition).Normalized();

            float suctionIntensity = 1f - (distance / tornadoRadius);
            Vector3 suction = -toSled * suctionForce * suctionIntensity;

            float tangentIntensity = distance / tornadoRadius * (1f - distance / tornadoRadius) * tangentAmplifier;
            Vector3 tangent = toSled.Cross(Vector3.Up).Normalized() * tangentForce * tangentIntensity;

            float updraftIntensity = 1f - (distance / tornadoRadius);
            Vector3 updraft = Vector3.Up * upforce * updraftIntensity;

            Vector3 finalForce = suction + tangent + updraft;

            if(distance <= flungFromCenterDistance)
            {
                ds.ApplyImpulse(finalForce * 0.3f, ds.GlobalPosition);
            }

            // ds.ApplyForce(finalForce, ds.GlobalPosition);
            ds.ApplyCentralForce(finalForce);
        }
        else if (GlobalPosition.DistanceTo(ds.GlobalPosition) > tornadoRadius + 3)
        {
            ds.HoverEnd();
            StopSFX();
        }
    }

    public void PlaySFX()
    {
        if(!tornadoSwirlingSFX.IsPlaying())
            tornadoSwirlingSFX.Play();
    }

    public void StopSFX()
    {
        if(tornadoSwirlingSFX.IsPlaying())
            tornadoSwirlingSFX.Stop();
    }
}
