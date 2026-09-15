using Godot;
using System;

[Tool]
public partial class Checkpoint : Node3D {
    [Export] private Area3D _trigger;
    [Export] public Node3D _respawnPoint;
    [Export] private AudioStreamPlayer3D _triggerSound;

    private bool _triggered = false;
    private bool _indicationTriggered = false;

    [Signal]
    public delegate void OnEnteredEventHandler(Checkpoint checkpoint);

    //------------------------------------------
    // Override
    //------------------------------------------

    public override void _Ready()
    {
        _trigger.BodyEntered += OnTriggerBodyEntered;
        ActivatePassedIndication(false);
    }

    //------------------------------------------
    // Custom
    //------------------------------------------
    private void OnTriggerBodyEntered(Node3D body) {

        BaseCharacter dog = body as BaseCharacter;
        if (dog == null)
            return;

        var sled = dog.Sled;
        sled.Checkpoint = this;

        Trigger(dog);
    }

    private void ActivatePassedIndication(bool activate, BaseCharacter dog = null) {
        // TODO: Change with specific indication effects

        if (dog is not null)
        {
            SetFlagColor(dog);
        }
    }

    private void SetFlagColor(BaseCharacter dog) {

        if (_indicationTriggered) {
            return;
        }

        Color color = Colors.White;

        if (dog is DogController) {
            color = Colors.Blue;
        }
        if (dog is RacingNpc) {
            color = Colors.Red;
        }

        StandardMaterial3D material = new StandardMaterial3D();
        material.AlbedoColor = color;

        _indicationTriggered = true;
    }

    public override void _ExitTree() {
        base._ExitTree();
        _trigger.BodyEntered -= OnTriggerBodyEntered;
    }

    //------------------------------------------
    // Public
    //------------------------------------------

    public void Trigger(BaseCharacter dog = null){
        if (_triggered)
            return;

        ActivatePassedIndication(true, dog);

        if (dog is not RacingNpc) {
            EmitSignal(nameof(OnEntered), this);
            _triggered = true;
        }

        _triggerSound.Play();

    }

    public Vector3 GetRespawnPointPosition() { return _respawnPoint.GlobalPosition; }
}
