using Godot;
using System;

public partial class ThinIce : Node
{
    [Export] float _timer = 3;
    [Export] int _objetsToCrack = 2;
    [Export] Area3D _trigger;

    int _objectsOn = 0;

    public override void _Ready()
    {
        _trigger.BodyEntered += OnTriggerEnter;
        _trigger.BodyExited += OnTriggerExit;
    }

    public override void _Process(double delta)
    {
        if (_objectsOn >= _objetsToCrack)
        {
            int mutliplicator = 1 + _objectsOn - _objetsToCrack;
            _timer -= (float)delta * mutliplicator;
        }

        if (_timer <= 0)
        {
            this.QueueFree();
        }
    }


    void OnTriggerEnter(Node body)
    {
        if (body is DogController dog)
        {
            _objectsOn++;
        }

        if (body is DogSled sled)
        {
            _objectsOn++;
        }
    }

    void OnTriggerExit(Node body)
    {
        if (body is DogController dog)
        {
            _objectsOn--;
        }

        if (body is DogSled sled)
        {
            _objectsOn--;
        }
    }
}
