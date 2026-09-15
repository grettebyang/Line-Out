using Godot;
using System;
using System.ComponentModel;

public partial class CursorButton : Control
{
    [Export] protected AudioStreamPlayer _audioPlayer;
    [Export] protected AudioStream _soundFocus;
    [Export] protected AudioStream _soundClick;
    [Export] protected ColorRect _hover;
    [Export] protected CollisionShape2D _collisionShape;
    [Export] protected TextureRect _texture;
    [Export] protected Label _text;
    [Export] protected int _buttonId;
    [Export] protected bool _staySelected;
    [Export] protected bool _canDeselect;
    protected bool _selected = false;

    protected int _cursorsHovering;
    [Export] protected bool _disabled = false;

    [Signal] public delegate void PressedEventHandler(PlayerCurser cursor, int buttonId);
    [Signal] public delegate void HoveredEventHandler(PlayerCurser cursor, int buttonId);
    [Signal] public delegate void UnHoveredEventHandler(PlayerCurser cursor, int buttonId);

    public override void _Ready()
    {
        ResizeCollisionShape();
        if(_disabled)
        {
            SetDisabled();
        }
    }

    public virtual void ResizeCollisionShape()
    {
        RectangleShape2D shape = new RectangleShape2D();
        shape.Size = Size;
        _collisionShape.Shape = shape;
        _collisionShape.Position = new Vector2(Size.X/2, Size.Y/2);
        //GD.Print(shape.Size);
    }

    protected virtual void OnFocus(Node2D body){
        if(_disabled)
            return;
            
        if(body.GetType() == typeof(PlayerCurser)){
            _audioPlayer.Stream = _soundFocus;
            _audioPlayer.Play();
            SetFocused((body as PlayerCurser));
            //GD.Print("Cursor entered");
            EmitSignal(nameof(Hovered), (body as PlayerCurser), _buttonId);
        }
    }

    public virtual void SetFocused(PlayerCurser cursor)
    {
        if(_disabled)
            return;
        
        Hover();
        _cursorsHovering++;
        cursor.ButtonPressed += OnPressed;
    }

    public virtual void Hover()
    {
        _hover.Visible = true;
        _text.Set("theme_override_colors/font_color", _text.GetThemeColor("font_hover_color", "Button"));
    }

    public virtual void UnHover()
    {
        _hover.Visible = false;
        _text.Set("theme_override_colors/font_color", _text.GetThemeColor("font_color", "Button"));
    }

    protected virtual void ExitFocus(Node2D body){
        if(_disabled)
            return;

        if(body.GetType() == typeof(PlayerCurser))
        {
            (body as PlayerCurser).ButtonPressed -= OnPressed;
            _audioPlayer.Stream = _soundFocus;
            _audioPlayer.Play();
            _cursorsHovering--;

            if(!_selected && _cursorsHovering == 0)
            {
                UnHover();
                EmitSignal(nameof(UnHovered), (body as PlayerCurser), _buttonId);
            }
        }
    }

    protected virtual void OnPressed(PlayerCurser cursor){
        if(_disabled)
            return;
            
        if(_staySelected)
        {
            if(_canDeselect)
            {
                _selected = !_selected;
            }
            else
            {
                _selected = true;
            }
        }
        //cursor.ButtonPressed -= OnPressed;
        _audioPlayer.Stream = _soundClick;
        _audioPlayer.Play();
        EmitSignal(nameof(Pressed), cursor, _buttonId);
    }

    public void SetText(string txt)
    {
        _text.Text = txt;
    }

    public void SetDisabled()
    {
        _disabled = true;
        _text.Set("theme_override_colors/font_color", _text.GetThemeColor("font_disabled_color", "Button"));
    }

    public void ResetSelected()
    {
        if(_staySelected)
        {
            _selected = false;
        }
        if(_cursorsHovering == 0)
        {
            UnHover();          
        }
    }

    public void Select()
    {
        if(!_staySelected)
            return;

        _selected = true;
        Hover();
    }

    public void SetEnabled()
    {
        _disabled = false;
        _text.Set("theme_override_colors/font_color", _text.GetThemeColor("font_color", "Button"));
    }

    public int GetButtonId()
    {
        return _buttonId;
    }
}
