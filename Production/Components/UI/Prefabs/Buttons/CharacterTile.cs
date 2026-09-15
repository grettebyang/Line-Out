using Godot;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public partial class CharacterTile : StaticBody2D
{
	[Export]
	private Area2D _collisionArea;
	[Export]
	private ColorRect _hover;
	[Export]
	private Label _nametag;
	[Export]
	private AudioStreamPlayer _selectSound;
	[Export]
	public int _characterId;
	public bool _selected = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_nametag.Text = UIConstants.CHARACTER_NAMES[_characterId];
		_nametag.Modulate = UIConstants.PLAYER_COLORS[_characterId];

		switch (_characterId)
		{
			case 0: _selectSound.Stream = DogBarksList.DOG_1_BARKS[0]; break;
			case 1: _selectSound.Stream = DogBarksList.DOG_4_BARKS[0]; break;
			case 2: _selectSound.Stream = DogBarksList.DOG_3_BARKS[0]; break;
			case 3: _selectSound.Stream = DogBarksList.DOG_2_BARKS[0]; break;
			default: break;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	public void Hover(bool hover)
	{
		_hover.Visible = hover;
	}

	public void Select(int playerId)
	{
		if(!_selected)
		{
			_selectSound.Play();
		}
		_selected = true;
		GameManager.GetInstance().SetPlayerCharacter(playerId, _characterId);
		_hover.Color = UIConstants.PLAYER_COLORS[playerId];
		Hover(true);
	}

	public void Deselect()
	{
		_selected = false;
		_hover.Color = Colors.Black;
	}

	
}
