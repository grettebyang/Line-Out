using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public enum EDeviceType{
    KEYBOARD,
    GAMEPAD
}

/*
Handles basic in-game inputs of specific controller (1 player = 1 controller ideally).

HOW TO ADD NEW ACTION? 
- Open 'InputSystem' singleton scene or any custom InputSystem scene you are using
- Click InputSystemController node under 'Schemes' nodes
- Add item to input list under "Input event groups" (Player movement, Rythm event, etc.)
- Choose input
- Set input name in property 'Resources' -> 'Name'

Input is automatically defined in 'Project -> Project settings -> Input map'

HOW TO USE ACTION?
- Have InputSystemController reference in your player character object (or other input target)
- MAKE SURE INPUT GROUP IS ACTIVE
    - e.g. When using input from _inputsRythmEvent, call 'InputSystem.GetInstance().AddActiveInputMode(EInputSystemMode. ...)'
    - Make sure colliding input group is supressed using 'InputSystem.GetInstance().RemoveActiveInputMode(EInputSystemMode. ...)' or 'InputSystem.GetInstance().ClearActiveInputMode()'
- Call any methods from "Input methods" while passing string from 'Resources' -> 'Name' of desired input:
    E.g. 

    InputSystemController controller = ...

    public override void _Input(InputEvent @event){
        bool jump = controller.IsPressed("jump");
        float x = controller.GetInputAxis("left", "right")
    }

NOTES:
Name in InputEvent 'Resources' -> 'Name' can be used as reference name only if processing inputs throught InputSystemController. E.g. "jump" can b simply used as 'controller.IsPressed("jump")'.
This input is stored also in 'Project -> Project settings -> Input map' but formated (e.g. "IS_KEYBOARD0_jump", "IS_GAMEPAD1_jump", etc.). InputSystemController input methods handles the prefix automatically.

So input could be accessed directly using e.g. 'Input.IsActionPressed("IS_KEYBOARD0_jump")' but it is not recommend unless you need to hack around it somehow.
*/

public partial class InputSystemController : Node
{
    public const string CUSTOM_PREFIX_IS = "IS_";
    public const string INPUT_ACTIVATE = "activate";
    public const string INPUT_DEACTIVATE = "deactivate";

    [Export]
    private EDeviceType _deviceType;

    [Export]
    private InputSystemPreset _preset;

    // List of actions names which are part of active group
    // e.g. InputSystem._activeInputMode must have EInputSystemMode.PLAYER_MOVEMENT flag active in order for input from InputSystemPreset._inputsPlayerMovemenet to get processed
    private List<StringName> _activeActions = new List<StringName>();

    private int _deviceID = -1;
    private bool _isActive = false;
    private bool _inputFrameCleared = false;

    private string _inputPrefix;

    //------------------------------------------
    // Input getters
    //------------------------------------------

    public string GetActionActivate() { return _preset._eventActivate.ResourceName; }

    public string GetActionDeactivate() { return _preset._eventDeactivate.ResourceName; }

    //------------------------------------------
    // Input methods
    //------------------------------------------

    // Is action pressed template
    /// <summary>
    /// Uses Input.IsActionPressed(action) to detect input but makes sure that used controller is active
    /// </summary>
    /// <param name="action">Name from 'Resources' -> 'Name' define in input scheme</param>
    /// <returns>True if action is pressed (held down)</returns>
    public bool IsPressed(string action){
        StringName formated = _inputPrefix + action;

        if (!_isActive || !_activeActions.Contains(formated) || _inputFrameCleared)
            return false;

        return Input.IsActionPressed(formated);
    }

    // Is action pressed template
    /// <summary>
    /// Uses Input.IsActionJustPressed(action) to detect input but makes sure that used controller is active
    /// </summary>
    /// <param name="action">Name from 'Resources' -> 'Name' define in input scheme</param>
    /// <returns>True if action is pressed once</returns>
    public bool IsJustPressed(string action){
        StringName formated = _inputPrefix + action;


        //GD.Print("Active: " + _isActive);
        //GD.Print("_activeActions.Contains(formated): " + _activeActions.Contains(formated));

        if (!_isActive || !_activeActions.Contains(formated) || _inputFrameCleared)
            return false;

        return Input.IsActionJustPressed(formated);
    }

    // Is action pressed template
    /// <summary>
    /// Uses Input.IsActionJustReleased(action) to detect input but makes sure that used controller is active
    /// </summary>
    /// <param name="action">Name from 'Resources' -> 'Name' define in input scheme</param>
    /// <returns>True if action is pressed once</returns>
    public bool IsJustReleased(string action){
        StringName formated = _inputPrefix + action;


        //GD.Print("Active: " + _isActive);
        //GD.Print("_activeActions.Contains(formated): " + _activeActions.Contains(formated));

        if (!_isActive || !_activeActions.Contains(formated) || _inputFrameCleared)
            return false;

        return Input.IsActionJustReleased(formated);
    }

    // Is action pressed template
    /// <summary>
    /// Uses Input.GetActionStrength(action) to get how much is input pressed. 
    /// </summary>
    /// <param name="action">Name from 'Resources' -> 'Name' define in input scheme</param>
    /// <returns>From 0 to 1 based on axis position. Return just 0 or 1 for regular buttons.</returns>
    public float GetInputAxis(string action){
        StringName formated = _inputPrefix + action;

        if (!_isActive || !_activeActions.Contains(formated) || _inputFrameCleared)
            return 0;

        return Input.GetActionStrength(formated);
    }

    // Is action pressed template
    /// <summary>
    /// Uses Input.GetAxis(negativeAction, positiveAction) to get how much is input pressed towards one or another axis 
    /// </summary>
    /// <param name="negativeAction">Name from 'Resources' -> 'Name' define in input scheme for -1 value</param>
    /// <param name="positiveAction">Name from 'Resources' -> 'Name' define in input scheme for +1 value</param>
    /// <returns>From -1 to 1 based on axis position. Return just -1 or 0 or 1 for regular buttons.</returns>
    public float GetInputAxis(string negativeAction, string positiveAction){
        string neg = _inputPrefix + negativeAction;
        string pos = _inputPrefix + positiveAction;

        if (!_isActive || !_activeActions.Contains(neg) || !_activeActions.Contains(pos) || _inputFrameCleared)
            return 0;

        return Input.GetAxis(neg, pos);
    }

    //------------------------------------------
    // Custom
    //------------------------------------------

    private void Init(){
         _inputPrefix = CUSTOM_PREFIX_IS + _deviceType.ToString() + _deviceID.ToString() + "_";

        // Add activate/deactivate actions
        AddInputAction(_preset._eventActivate, INPUT_ACTIVATE);
        AddInputAction(_preset._eventDeactivate, INPUT_DEACTIVATE);

        // Grouped actions
        foreach (InputEvent input in _preset._inputsPlayerMovemenet){
            AddInputAction(input, input.ResourceName);
        }

        foreach (InputEvent input in _preset._inputsRythmEvent){
            AddInputAction(input, input.ResourceName);
        }

        foreach (InputEvent input in _preset._inputsMenu){
            AddInputAction(input, input.ResourceName);
        }
    }

    private void AddInputAction(InputEvent input, string name){
        // Setup name
        StringName actionName = _inputPrefix + name;
        input.ResourceName = actionName;

        // Prevent adding redundant actions
        if (InputMap.HasAction(actionName))
            return;

         // Id
        if (_deviceType == EDeviceType.KEYBOARD)
                input.Device = 0;
        else
            input.Device = _deviceID;

        //GD.Print("Add: " + actionName + ", input: " + input.)

        // Add 
        InputMap.AddAction(input.ResourceName);
        InputMap.ActionAddEvent(input.ResourceName, input);
    }

    //------------------------------------------
    // Public API
    //------------------------------------------

    /// <summary>
    /// Set device ID. Should match Godot device ID to recognize it whitin the engine. ID can be set only once.
    /// Will also call Init function to setup inputs into input map
    /// </summary>
    public void SetDeviceId(int value) {
        if (_deviceID != -1)
            return;
        
        _deviceID = value;
        Init();
    }

    /// <summary>
    /// Get device ID of current device which match to Godot device ID recognition. Warning: Keyboard (all keyboard parts) and first Gamepad has both device ID 0
    /// </summary>
    public int GetDeviceId() { return _deviceID; }

    /// <summary>
    /// Set controller active so it can process inputs. Inactive input will return FALSE when using input handling actions.
    /// WARNING: This function should be predominatly used by InputSystem. Use with caution in case of need and don't break InputSystem activation handling.
    /// </summary>
    public void Activate(bool activate) { _isActive = activate; }

    /// <summary>
    /// Active TRUE = can handle input, otherwise input handling won't be processed
    /// </summary>
    public bool IsActive() { return _isActive; }

    /// <summary>
    /// Sets _inputFrameCleared to true, so no input will be registered until it is set to false again
    /// </summary>
    public void ClearInput(bool clear) { _inputFrameCleared = clear; }

    public EDeviceType GetDeviceType() { return _deviceType; }

    /// <summary>
    /// Copy all input events objects to make sure each controller has unique input with unique names (avoid overriding event inputs of other controllers)
    /// </summary>
    /// <param name="controller">Controller reference from which inputs should be copied</param>
    public void CopyInputs(InputSystemController controller){
        InputSystemPreset ctrlPreset = controller._preset;

        // Activation
        _preset._eventActivate = ctrlPreset._eventActivate.Duplicate() as InputEvent;
        _preset._eventDeactivate = ctrlPreset._eventDeactivate.Duplicate() as InputEvent;

        // Grouped inputs
        _preset = ctrlPreset.Duplicate() as InputSystemPreset;
        
        // Player movement
        _preset._inputsPlayerMovemenet = new InputEvent[ctrlPreset._inputsPlayerMovemenet.Length];

        for (int i = 0; i < _preset._inputsPlayerMovemenet.Length; i++){
            _preset._inputsPlayerMovemenet[i] = ctrlPreset._inputsPlayerMovemenet[i].Duplicate() as InputEvent;
        }

        // Rythm event
        _preset._inputsRythmEvent = new InputEvent[ctrlPreset._inputsRythmEvent.Length];

        for (int i = 0; i < _preset._inputsRythmEvent.Length; i++){
            _preset._inputsRythmEvent[i] = ctrlPreset._inputsRythmEvent[i].Duplicate() as InputEvent;
        }

        // Menu
        _preset._inputsMenu = new InputEvent[ctrlPreset._inputsMenu.Length];

        for (int i = 0; i < _preset._inputsMenu.Length; i++){
            _preset._inputsMenu[i] = ctrlPreset._inputsMenu[i].Duplicate() as InputEvent;
        }
    }

    /// <summary>
    /// Updates list based on currently active inputs group
    /// </summary>
    public void UpdateActiveInputs(InputSystem system){
        _activeActions.Clear();

        // Player movement
        if (system.IsInputModeActive(EInputSystemMode.PLAYER_MOVEMENT)){
            foreach (InputEvent inpEvent in _preset._inputsPlayerMovemenet){
                _activeActions.Add(inpEvent.ResourceName);
            }
        }

        // Actions
        if (system.IsInputModeActive(EInputSystemMode.RYTHM_EVENT))
        {
            foreach (InputEvent inpEvent in _preset._inputsRythmEvent)
            {
                _activeActions.Add(inpEvent.ResourceName);
            }
        }

        // Menu
        if (system.IsInputModeActive(EInputSystemMode.MENU)){
            foreach (InputEvent inpEvent in _preset._inputsMenu){
                _activeActions.Add(inpEvent.ResourceName);
            }
        }

        // Universal
        if (system.IsInputModeActive(EInputSystemMode.UNIVERSAL)){
            foreach (InputEvent inpEvent in _preset._inputsUniversal){
                _activeActions.Add(inpEvent.ResourceName);
            }
        }
    }

    public int GetActiveActionsCount() { return _activeActions.Count; }
}
