using Godot;
using System;

public partial class CharacterButton : CursorButton
{
    [Export] protected TextureRect _portrait;
    [Signal] public delegate void UnPressedEventHandler(PlayerCurser cursor, int buttonId);

    public int _selected;
    public override void _Ready()
    {
        base._Ready();

		_text.Text = UIConstants.CHARACTER_NAMES[_buttonId];
        _hover.Set("color", _hover.GetThemeColor("character_icon_hover_color", "Button"));
        //_text.Set("theme_override_colors/font_color", UIConstants.PLAYER_COLORS[_buttonId]);
        _portrait.Texture = UIConstants.CHARACTER_PORTRAITS[_buttonId];
        _selected = GameManager.GetInstance().GetPlayerOfCharacter(_buttonId); // -1 if no player has selected this character
        if(_selected != -1)
        {
            // Set the tile as selected
            _hover.Visible = true;
            _hover.Set("color", UIConstants.PLAYER_COLORS[_selected]);
            _text.Set("theme_override_colors/font_color", _text.GetThemeColor("font_hover_color", "Button"));
        }

		switch (_buttonId)
		{
			case 0: _soundClick = DogBarksList.DOG_1_BARKS[0]; break;
			case 1: _soundClick = DogBarksList.DOG_4_BARKS[0]; break;
			case 2: _soundClick = DogBarksList.DOG_3_BARKS[0]; break;
			case 3: _soundClick = DogBarksList.DOG_2_BARKS[0]; break;
			default: break;
		}

        // Input system listeners
        InputSystem.GetInstance().OnControllerDeactivate += OnControllerDeactivate;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        InputSystem.GetInstance().OnControllerDeactivate -= OnControllerDeactivate;
    }


    protected override void OnPressed(PlayerCurser cursor)
    {
        if(_selected == -1) // Select character
        {
            cursor.BackPressed += OnUnpressed;
            _selected = cursor.Id;
            _audioPlayer.Stream = _soundClick;
            _audioPlayer.Play();
            _hover.Set("color", UIConstants.PLAYER_COLORS[cursor.Id]);
            EmitSignal(nameof(Pressed), cursor, _buttonId);
            //cursor.ButtonPressed += OnUnpressed;
        }
        else if(_selected == cursor.Id) // Deselect character
        {
            _selected = -1;
            _hover.Set("color", _hover.GetThemeColor("character_icon_hover_color", "Button"));
            EmitSignal(nameof(UnPressed), cursor, _buttonId);
        }
    }

    public void OnControllerDeactivate(InputSystemController controller, int id)
    {
        if(_selected == id)
        {
            GameManager.GetInstance().SetPlayerCharacter(id, -1);
            DeselectCharacterButton();
        }
    }

    public void DeselectCharacterButton()
    {
        _selected = -1;
        _hover.Set("color", _hover.GetThemeColor("character_icon_hover_color", "Button"));
        if(_cursorsHovering == 0)
        {
            _hover.Visible = false;
            _text.Set("theme_override_colors/font_color", _text.GetThemeColor("font_color", "Button"));
        }
    }

    public void SetPlayerToButton(int playerId)
    {
        _selected = playerId;
        _hover.Set("color", UIConstants.PLAYER_COLORS[_selected]);
    }

    public void SetCursorOnOpen(PlayerCurser cursor)
    {
        cursor.BackPressed += OnUnpressed;
    }

    public void ResetCursorOnClose(PlayerCurser cursor)
    {
        cursor.BackPressed -= OnUnpressed;
    }

    protected void OnUnpressed(PlayerCurser cursor)
    {
        cursor.BackPressed -= OnUnpressed;
        DeselectCharacterButton();
        EmitSignal(nameof(UnPressed), cursor, _buttonId);
    }
    
    protected override void ExitFocus(Node2D body)
    {
        if(body.GetType() == typeof(PlayerCurser))
        {
            PlayerCurser cursor = body as PlayerCurser;
            cursor.ButtonPressed -= OnPressed;
            _cursorsHovering--;

            GD.Print("exit focus");
            if(_selected == -1)
            {
                _audioPlayer.Stream = _soundFocus;
                _audioPlayer.Play();
                if(_cursorsHovering == 0)
                {
                    _hover.Visible = false;
                    _text.Set("theme_override_colors/font_color", _text.GetThemeColor("font_color", "Button"));
                    EmitSignal(nameof(UnHovered), cursor, _buttonId);
                }
            }
        }

    }

    protected override void OnFocus(Node2D body)
    {
        if(body.GetType() == typeof(PlayerCurser)){
            PlayerCurser cursor = body as PlayerCurser;
            _cursorsHovering++;
            cursor.ButtonPressed += OnPressed;
            _hover.Visible = true;
            _text.Set("theme_override_colors/font_color", _text.GetThemeColor("font_hover_color", "Button"));
            if(_selected == -1)
            {
                _audioPlayer.Stream = _soundFocus;
                _audioPlayer.Play();
            }
            EmitSignal(nameof(Hovered), cursor, _buttonId);
            GD.Print("Cursor entered");
        }
    }

}
