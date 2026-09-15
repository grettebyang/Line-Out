using Godot;
using System;
using System.Drawing;
using System.Numerics;

public partial class Collectable : Area3D
{
	public enum PowerUpType {
        SpeedBoost,
        Hover,
        Shield
    }

	// Called when the node enters the scene tree for the first time.
	[Export]
	public Godot.Color _col;

	[Export]
	public PowerUpType PuType;

	[Export]
	public MeshInstance3D _mesh;

	[Export]
	public string _feedbackText;

	public double _val;

	//public RhythmManager _rm;

	public float _sinX = 0.0f;

	public float _yVal;

	public float _scale;

	public float _respawnTimer = 0.0f;

	[Export] public float _respawnTime = 10f;

    [Signal]
	public delegate void OnCollectableEnteredEventHandler(int puType, Godot.Color col, double val);

	[Signal]
	public delegate void OnFikaEnteredEventHandler();

	public delegate void FillGauge();
	public override void _Ready()
	{
		switch((int)PuType){
			case 0 : _col = new Godot.Color("db88c0"); break;
			case 1 : _col = new Godot.Color("f0a600"); break;
			case 2 : _col = new Godot.Color("4c7bd6"); break;
			default : break;
		}
		Godot.Collections.Array<Node> children = GetChildren();
		foreach(Node3D child in children){
			if(child is UnderShadow)
				continue;
			child.Visible = false;
		}
		GetChild<MeshInstance3D>((int)PuType + 1).Visible = true;
		//_rm = RhythmManager.GetInstance();
		_val = 100;
		_yVal = Position.Y;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta){
		// Animate up and down
		_sinX = (_sinX + 5f*(float)delta) % 360f;
		float _newY = .1f*(float)Math.Sin(_sinX);
		Position = new Godot.Vector3(Position.X, _yVal + _newY, Position.Z);
		Rotation += new Godot.Vector3(Rotation.X, .02f, Rotation.Z);

		// Respawn
		if(_respawnTimer > 0.0f)
		{
			_respawnTimer -= (float)delta;
			if(_respawnTimer <= 0.0f)
			{
				Visible = true;
			}
			return;
		}
		if(Scale.X < 1.0f)
		{
			_scale = Math.Min(1.0f - Scale.X, (float)delta);
			Scale += Godot.Vector3.One * _scale;
			return;
		}
		
		IsDogColliding();
	}

	public void IsDogColliding(){
		var bodies = GetOverlappingBodies();
		for(int i = 0; i < bodies.Count; i++){
			if(bodies[i].GetType() == typeof(DogController)){
				EmitSignal(nameof(OnCollectableEntered), (int)PuType, _col, _val);
				EmitSignal(nameof(OnFikaEntered));
				DogController dog = bodies[i] as DogController;
				dog._crunch.Play();
				dog.GetNode<Label>("FeedbackText").Modulate = new Godot.Color(dog.GetNode<Label>("FeedbackText").Modulate, 1.0f);
				dog.GetNode<Label>("FeedbackText").Text = _feedbackText;
				Scale = Godot.Vector3.One * .01f;
				Visible = false;
				//Monitoring = false;
				_respawnTimer = _respawnTime;
				//this.QueueFree();
				break;
			}
		}
	}

	
}
