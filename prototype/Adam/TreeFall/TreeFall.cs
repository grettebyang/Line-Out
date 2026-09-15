using Godot;
using System;

public partial class TreeFall : Node3D
{
    [Export] FallingTree treeRB;
    [Export] AudioStreamPlayer3D sfx;
    // The Area3d radius is really big by default and we use an int for actual radius detection since it's more explicitly clear
    [Export] int detectionDistance = 15;
    [Export] int fallForce = 500;
    [Export] Node3D targetFall; // where should the tree fall
    [Export] private bool randomizeTreeFallDistance = false; // do we want to ensure this tree has default detection dist (useful for end of tutorial level)
    Vector3 fallDirection = Vector3.Zero;
    [Export] private int forceIterations = 5; // sometimes on slopes the tree might fall the direction we want and thus increase how much we force the tree in our desired direction 
    private bool fallen;

    void determineTreeFallArea()
    {
        var random = new RandomNumberGenerator();
        random.Seed = 123454321;
        var num = GD.RandRange(0, 1);

        if(num == 0)
        {
            detectionDistance = 5;
        }        
    }
    
    public override void _Ready()
    {
        treeRB.Freeze = true;
        fallen = false;
        determineTreeFallArea();
        GetNode<CollisionShape3D>("Area3D/CollisionShape3D").Shape = new SphereShape3D { Radius = detectionDistance };
    }

    void BodyEntered(Node3D body)
    {
        if (body is DogSled sled && !fallen)
        {
            treeFall(sled);
        }
    }

    void treeFall(DogSled ds)
    {
        fallen = true;
        sfx.Play();
        if(!GodotObject.IsInstanceValid(targetFall))
        {
            fallDirection = ds.GlobalPosition - GlobalPosition;
            fallDirection = new Vector3(fallDirection.X, 0.0f, fallDirection.Z).Normalized();
        }
        else
        {
            fallDirection = targetFall.GlobalPosition - GlobalPosition;
            fallDirection = new Vector3(fallDirection.X, 0.0f, fallDirection.Z).Normalized();
        }
        treeRB.Freeze = false;
        for(int i = 0; i < 5; i++)
        {
            treeRB.ApplyForce(fallDirection * fallForce, new Vector3(0, 6, 0));
        }
    }

    void OnSFXFinished()
    {
        treeRB.fallen = true;
    }
}
