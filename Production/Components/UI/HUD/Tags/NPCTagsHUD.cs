using Godot;
using System;

public partial class NPCTagsHUD : Node
{
    [Export] protected RacingNPCSled _NPCSled;
    [Export] protected Control _NPCTag;
    [Export] protected Label _lblNPCTagDistance;
    [Export] Vector2 _tagLimitPadding;

    protected DogSled _playerSled;
    protected Camera3D _cam;

    public override void _Ready(){
        _playerSled = GameManager.GetInstance().CurrentLevel._sled;
        _cam = _playerSled._camera._sledCamera;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_NPCSled != null)
        {
            // Dist base
            float dist = _playerSled.GlobalPosition.DistanceTo(_NPCSled.GlobalPosition);
            _lblNPCTagDistance.Text = ((int)dist).ToString();


            // Show position
            bool behind = _cam.IsPositionBehind(_NPCSled.GlobalPosition);
            Vector2 pos = _cam.UnprojectPosition(_NPCSled.GlobalPosition);

            if (!behind)
            {
                // Front
                int posx = Mathf.Clamp((int)pos.X, (int)_tagLimitPadding.X, (int)GetViewport().GetVisibleRect().Size.X - (int)_tagLimitPadding.X);
                int posy = Mathf.Clamp((int)pos.Y, (int)_tagLimitPadding.Y, (int)GetViewport().GetVisibleRect().Size.Y - (int)_tagLimitPadding.Y);
                _NPCTag.GlobalPosition = new Vector2(posx, posy);
            }
            else
            {
                // Behind
                int screenx = (int)GetViewport().GetVisibleRect().Size.X;
                int posx = Mathf.Clamp((int)pos.X, (int)_tagLimitPadding.X, screenx - (int)_tagLimitPadding.X);
                int posy = (int)GetViewport().GetVisibleRect().Size.Y - (int)_tagLimitPadding.Y;
                
                _NPCTag.GlobalPosition = new Vector2(screenx - posx, posy);
            }
        }
    }
}
