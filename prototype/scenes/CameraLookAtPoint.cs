using Godot;
using System;
using System.Collections.Generic;

public partial class CameraLookAtPoint : Node3D{
    [Export]
    Godot.Collections.Array<Node3D> _dogs {get; set;}
    [Export]
    Node3D _startPoint; //generally make this sled, except in atlernate gameplay scenarios
    public override void _Process(double delta){
        int count = 0;
        Vector3 offset = new Vector3();
        GlobalPosition = _startPoint.GlobalPosition;
        foreach(Node3D dog in _dogs){
            count++;
            offset +=  dog.GlobalPosition - _startPoint.GlobalPosition;
        }
        if(count > 0)
            offset /= count;
        DebugDraw3D.DrawLine(_startPoint.GlobalPosition, GlobalPosition);
        GlobalPosition = GlobalPosition.Slerp( GlobalPosition + offset, 0.7f);
        

    }

}
