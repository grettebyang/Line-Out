using Godot;
using System;

public partial class LoadingIcon : TextureRect
{
    public bool _entering = false;
    public Vector2 _distance = new Vector2(412, 0);
    public Vector2 _enterToPos = new Vector2();
    public Vector2 _offsetEnter = new Vector2(332, 288);
    public Vector2 _offsetExit = new Vector2(-80, 288);
    [Export] Control _screen;

    public override void _Ready()
    {
        Position = _screen.Size - _offsetEnter;
        _enterToPos = Position;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if(Position != _enterToPos)
            Position = Position.Lerp(_enterToPos, 10f * (float)delta);
        //GD.Print("cup position: " + Position);
    }

    public void OnEnter()
    {
        _entering = true;
        Position = _screen.Size - _offsetExit;
        _enterToPos = _screen.Size - _offsetEnter;
    }

    public void OnHide()
    {
        _entering = false;
        _enterToPos = _screen.Size - _offsetExit;
    }
}
