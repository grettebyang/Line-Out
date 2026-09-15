using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerTagsHUD : Control
{
    [Export] protected PackedScene _tagScene;
    protected List<DogTag> _tags = new List<DogTag>();

    protected ISDogSpawner _dogSpawner;    
    protected Camera3D _cam;
    protected Node3D _dog;

    protected Vector2 _offset = new Vector2(0.0f, -100f);

    public override void _Ready(){
        DogSled sled = GameManager.GetInstance().CurrentLevel._sled;
        _dogSpawner = sled.DogSpawner;
        _cam = sled._camera._sledCamera;

        _dogSpawner.OnDogSpawned += OnDogSpawned;
        _dogSpawner.OnDogDespawned += OnDogDespawned;

        for (int i = 0; i < _dogSpawner.DogList.Count; i++){
            OnDogSpawned(i);
        }
    }

    public override void _ExitTree(){
        _dogSpawner.OnDogSpawned -= OnDogSpawned;
    }

    protected void OnDogSpawned(int id){
        DogTag tag = ResourceLoader.Load<PackedScene>(_tagScene.ResourcePath).Instantiate() as DogTag;
        AddChild(tag);
        _tags.Add(tag);

        int playerCharacter = GameManager.GetInstance().GetPlayerCharacter(id);
        if(playerCharacter == -1)
        {
            tag.NameLabel.Text = UIConstants.PLAYER_NAMES[id];
        }
        else
        {
            tag.NameLabel.Text = UIConstants.CHARACTER_NAMES[playerCharacter];
        }
        tag.Modulate = UIConstants.PLAYER_COLORS[id];
    }

    protected void OnDogDespawned(int id){
        Control tag = _tags[id];
        _tags.RemoveAt(id);
        tag.QueueFree();
    }

    public override void _PhysicsProcess(double delta){
        for (int i = 0; i < _dogSpawner.DogList.Count; i++){
            _dog = _dogSpawner.DogList[i] as Node3D;
            _tags[i].GlobalPosition = _cam.UnprojectPosition(_dog.GlobalPosition) + _offset;
        }
    }

}
