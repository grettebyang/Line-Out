using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// IS = Input systems
/// Spawn dog characters (dogs) based on active cotrollers in InputSystem class
/// </summary>
public partial class ISDogSpawner : Node3D {

    // Public variables
    [Export] public DogSled sledReference;

    // Paired set up means dogs are spawned as 2-2
    public List<Node> DogList = new List<Node>();
    
    // Private variables
    [Export] private PackedScene _dogCharacterScene;
    [Export] private float _randOffset = 2;

    public float sledMassIncreaseFactor = 20;

    [Signal] public delegate void OnDogSpawnedEventHandler(int id);
    [Signal] public delegate void OnDogDespawnedEventHandler(int id);

    //------------------------------------------
    // Override
    //------------------------------------------
    private InputSystem _inputSystem = InputSystem.GetInstance();

    public override void _Ready() {
        // Spawn dogs on start
        InputSystemController[] controllers = _inputSystem.GetActiveControllersSlots();
        for (int i = 0; i < controllers.Length; i++){
            if (controllers[i] == null)
                continue;

            // Spawn
            SpawnCharacter(controllers[i], i);
        }

        // Listen to controller activation
        _inputSystem.OnControllerActivate += SpawnCharacter;
        _inputSystem.OnControllerActivate += SpawnSimulatedDogs;
        _inputSystem.OnControllerDeactivate += DespawnCharacter;
    }

    public override void _ExitTree(){
        _inputSystem.OnControllerActivate -= SpawnCharacter;
        _inputSystem.OnControllerDeactivate -= DespawnCharacter;
    }


    //------------------------------------------
    // Custom
    //------------------------------------------
    public void SpawnCharacter(InputSystemController controller, int id)
    {
        DogController plChar = ResourceLoader.Load<PackedScene>(_dogCharacterScene.ResourcePath).Instantiate() as DogController;
        DogList.Add(plChar);
        plChar.Id = id;
        int charId = GameManager.GetInstance().PlayerCharacters[id];
        if(charId == -1)
        {
            charId = id;
        }
        plChar._characterId = charId;

        plChar.TopLevel = true;
        plChar.AddSled(sledReference);

        AddChild(plChar);
        sledReference.AddDog(plChar);
        sledReference.PositionCharacters();

        SetupSimpleController(plChar, id);
        sledReference.AttatchRope(plChar);
        sledReference.AdjustSledToPlayerChange(1);

        EmitSignal(nameof(OnDogSpawned), id);
    }

    protected void DespawnCharacter(InputSystemController controller, int id) {
        foreach (Node dog in DogList) {
            if (dog == null)
                continue;
            if (dog is DogController dogController) {
                if (dogController.Id == id) {
                    sledReference.DetatchRope(dogController);
                    dogController.QueueFree();
                    DogList.Remove(dog);
                    sledReference.DeleteDog(dogController);
                    sledReference.AdjustSledToPlayerChange(-1);
                    EmitSignal(nameof(OnDogDespawned), id);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Spawn extra dogs which are controlled by controller 0
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="id"></param>
    void SpawnSimulatedDogs(InputSystemController controller, int id)
    {
        _inputSystem.OnControllerActivate -= SpawnSimulatedDogs;

#if TOOLS
        if (!GameManager.GetInstance().DebugMode)
            return;

        // Add simulated dogs
        GameDebugTool debugTool = GameManager.GetInstance().DebugTool;
        InputSystemController ctrl0 = InputSystem.GetInstance().GetActiveControllerSlot(0);

        for (int i = 0; i < debugTool.DogsToSimlate; i++)
        {
            SpawnCharacter(ctrl0, 0);
        }

#endif
    }

    //------------------------------------------
    // Test methods - TODO: Remove when not needed
    //------------------------------------------
    protected void SetupSimpleController(Node spawned, int id) {
        DogController dog = spawned as DogController;
        if (dog == null)
            return;

        dog.SetInputController(InputSystem.GetInstance().GetActiveControllerSlot(id), id);
    }
}