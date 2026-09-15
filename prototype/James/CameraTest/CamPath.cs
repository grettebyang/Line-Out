// using Godot;
// using System;
// using System.ComponentModel.DataAnnotations;



// public partial class CamPath : Path3D{

//     [Export]
//     PathFollow3D TrackedPoint;
//     [Export]
//     Camera _sledCam;
//     [Export]
//     Node3D VisualIndicator;
//     [Export]
//     float TestAmount;
    
//     public override void _PhysicsProcess(double delta)
//     {
//         //what this should do is lerp the position of the tracked point to the sled, or towards the furtherest back dog.
//         //float desOffset = Curve.GetClosestOffset(DogSled.GlobalPosition - GlobalPosition);
//         //TrackedPoint.Progress =  TrackedPoint.Progress + (desOffset - TrackedPoint.Progress) * (float)delta * 3;

//         TrackedPoint.Progress = Curve.GetClosestOffset(_sledCam.FurtherestBackPos() - GlobalPosition);
//         TrackedPoint.Progress = TrackedPoint.Progress + TestAmount;
//         VisualIndicator.GlobalPosition = VisualIndicator.GlobalPosition.Slerp(TrackedPoint.GlobalPosition, (float)delta * 2);
//         VisualIndicator.GlobalRotation = VisualIndicator.GlobalRotation.Slerp(TrackedPoint.GlobalRotation, (float)delta * 2);
//     }

// }
