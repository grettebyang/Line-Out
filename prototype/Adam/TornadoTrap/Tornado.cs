using Godot;
using System;

public partial class Tornado : Node3D
{
    [Export] Vector3 _tornadoSize;
    [Export] MeshInstance3D _tornadoMesh;
    DogSled ds;
    Vector3 _tagentForce = Vector3.Zero;
    Vector3 _targetForce = Vector3.Zero;
    [Export] private float tangentForce = 400f;
    [Export] private float upforce = 2000f;
    [Export] private float suctionForce = 1200f;
    [Export] private float tornadoRadius = 10;

    public override void _Ready()
    {
        ds = GameManager.GetInstance().CurrentLevel._sled;
        _tornadoMesh.Scale = _tornadoSize;
        tornadoRadius = _tornadoSize.X * 3;
    }

    public override void _PhysicsProcess(double delta)
    {
        TornadoTorque(delta);
    }

    public void TornadoTorque(double delta)
    {
        float distance = GlobalPosition.DistanceTo(ds.GlobalPosition);

        if (distance < tornadoRadius)
        {
            ds.GravityHover();

            Vector3 toSled = (ds.GlobalPosition - GlobalPosition).Normalized();

            float suctionIntensity = 1f - (distance / tornadoRadius);
            Vector3 suction = -toSled * suctionForce * suctionIntensity;

            float tangentIntensity = distance / tornadoRadius * (1f - distance / tornadoRadius) * 4f;
            Vector3 tangent = toSled.Cross(Vector3.Up).Normalized() * tangentForce * tangentIntensity;

            float updraftIntensity = 1f - (distance / tornadoRadius);
            Vector3 updraft = Vector3.Up * upforce * updraftIntensity;

            Vector3 finalForce = suction + tangent + updraft;

            ds.ApplyForce(finalForce, ds.GlobalPosition);

        }
        else if (GlobalPosition.DistanceTo(ds.GlobalPosition) > tornadoRadius + 15)
        {
            ds.HoverEnd();
        }
    }
}
