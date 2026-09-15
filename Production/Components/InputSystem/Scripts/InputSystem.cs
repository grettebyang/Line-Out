using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

/*
New flags needs to be implemented in InputSystemController under "Input event groups" and methods Init(), CopyInputs() and UpdateActiveInputs()
*/
[Flags]
public enum EInputSystemMode
{
    NONE = 0,
    PLAYER_MOVEMENT = 1 << 0,
    RYTHM_EVENT = 1 << 1,
    MENU = 1 << 2,
    UNIVERSAL = 1 << 3
    // Use = '1 << 3', '1 << 4', '1 << x', ... when adding new flag
    // INPUT_FLAG_X = 1 << 3,
    // INPUT_FLAG_y = 1 << 4,
}

/*
Handles multiple players input mainly for gameplay part of the game.
Tracks list of all available input devices and handles their activation and deactivation.

Use AllowControllerActivation to allow players activate their input and join to the game.
*/

public partial class InputSystem : Node
{
    public const string CONTROLLER_NAME_KEYBOARD = "KeyboardController_";
    public const string CONTROLLER_NAME_GAMEPAD = "GamepadController_";

    public const int MAX_ACTIVE_CONTROLLER_COUNT = 4;

    [Export]
    private bool _devModeActive = true; // TODO: Set this false when building official version
    [Export]
    private InputSystemController[] _keyboardSchemes;
    [Export]
    private InputSystemController _gamepadScheme;
    [Export]
    private Node _contollersContainer;

    private bool _controllerActivationAllowed = false;

    public bool IscontrollerActivationAllowed
    {
        get { return _controllerActivationAllowed; }
    }

    private EInputSystemMode _activeInputMode = EInputSystemMode.PLAYER_MOVEMENT;

    private List<InputSystemController> _connectedControllers = new List<InputSystemController>(); // List of all recognized controllers, they are not intended for controls unless they are in "_activeControllers" list
    private InputSystemController[] _activeControllers = new InputSystemController[MAX_ACTIVE_CONTROLLER_COUNT];
    private bool _autoDetectDone = false;
    private float _frameInputClearTimer = 0;
    private float _inputClearTime = .1f;

    // Signals
    [Signal]
    public delegate void OnNewControllerConnectEventHandler(InputSystemController controller);
    [Signal]
    public delegate void OnControllerDisconnectEventHandler(InputSystemController controller); // Should be when connection is lost and before controller is removed from list
    [Signal]
    public delegate void OnControllerActivateEventHandler(InputSystemController controller, int slotId);
    [Signal]
    public delegate void OnControllerDeactivateEventHandler(InputSystemController controller, int slotId);
    [Signal] public delegate void OnControllerActivationAllowedEventHandler(bool allowed);


    //------------------------------------------
    // Singleton
    //------------------------------------------

    private static InputSystem _instance;

    private InputSystem()
	{
		if (_instance != null)
            return;

		_instance = this;
	}

    public static InputSystem GetInstance()
	{
		return _instance;
	}

    //------------------------------------------
    // Override methods
    //------------------------------------------

    public override void _Ready()
    {
        // Setup keyboard parts and gamepads
        // for (int i = 0; i < _keyboardSchemes.Length; i++)
        // {
        //     ConnectController(i, _keyboardSchemes[i], CONTROLLER_NAME_KEYBOARD);
        // }

        Input.JoyConnectionChanged += OnJoyConnectionChanged;
    }

    public override void _Process(double delta)
    {
        if(_frameInputClearTimer > 0)
        {
            _frameInputClearTimer -= (float)delta;
            if(_frameInputClearTimer <= 0.0f)
            {
                CallDeferred("ActivateFrameInput");
            }
        }
        // Auto activate player 1 controller or keyboard scheme
        if (_autoDetectDone)
            return;
        
        bool controllerFound = false;

        foreach (InputSystemController controller in _connectedControllers)
        {
            if (controller.GetDeviceType() == EDeviceType.GAMEPAD)
            {
                ActivateController(controller);
                controllerFound = true;
                break;
            }
        }

        // if (!controllerFound)
        // {
        //     ActivateController(_connectedControllers[0]); // Activate keyboard as backup
        // }

        if (controllerFound)
        {
            _autoDetectDone = true;
        }
    }

    public override void _Input(InputEvent @event)
    {
        // Debug - Show active input modes
        if (Input.IsKeyPressed(Key.Semicolon)){
            GD.Print("[InputSystem DEBUG] Active input modes: " + _activeInputMode);
        }

        if (!_controllerActivationAllowed) // Prevents from controllers activation - e.g. could be TRUE in menu and FALSE during gameplay
            return;

        // Handle activation
        foreach (InputSystemController controller in _connectedControllers){
            //bool deviceCheck = controller.GetDeviceType() == EDeviceType.KEYBOARD ||  @event.Device == controller.GetDeviceId();
            bool deviceCheck = @event.Device == controller.GetDeviceId();

            // Add only if controller join input is pressed and is not yet active
            if (Input.IsActionJustPressed(controller.GetActionActivate()) && deviceCheck)
            {
                ClearFrameInput(); // Prevent joining and unjoining registering in the same frame
                if(!IsControllerActive(controller))
                {
                    ActivateController(controller);
                }
                else if(ActiveControllersCount() > 1 && controller.GetDeviceId() > 0) // Player 1 cannot unjoin
                {
                    // Deactivate active controller
                    DeactivateController(controller);    
                }
            }

        }
    }

    //------------------------------------------
    // Custom
    //------------------------------------------

    // Register controllers on onecting and unregister on disconecting
    private void OnJoyConnectionChanged(long device, bool connected)
    {
        // Connect
        if (connected)
        {
            ConnectController((int)device, _gamepadScheme, CONTROLLER_NAME_GAMEPAD);

            // Dev setup
            DevModeSetup();
            return;
        }

        // Disconnect
        DisconnectController((int)device);
    }

    /// <summary>
    /// Register connected scheme or device into list of available controllers - Means device is physicaly available, but not used in game
    /// </summary>
    /// <param name="id"></param>
    /// <param name="scheme"></param>
    /// <param name="name"></param>
    private void ConnectController(int id, InputSystemController scheme, string name = "")
    {
        InputSystemController controller = scheme.Duplicate() as InputSystemController;
        controller.CopyInputs(scheme);
        controller.SetDeviceId(id);

        _contollersContainer.AddChild(controller);
        _connectedControllers.Add(controller);
        controller.Name = name + id.ToString();

        EmitSignal(nameof(OnNewControllerConnect), controller);
        //GD.Print("ConnectController: " + controller.Name + ", status: " + _connectedControllers.Count);
        // Activate controller if it is the only controller connected
        // if (controller.GetDeviceType() == EDeviceType.GAMEPAD && id == 0 && _activeControllers[0] == null){
        //     ActivateController(controller);
        // }
    }

    // Unregisted disconnected scheme or device from list of available controllers
    private void DisconnectController(int id)
    {
        // Find device to remove
        InputSystemController controller = null;

        // Ignore keyboard controllers when removing
        //for (int i = _keyboardSchemes.Length; i < _connectedControllers.Count; i++)
        for (int i = 0; i < _connectedControllers.Count; i++)
        {
            if (_connectedControllers[i].GetDeviceId() == id)
            {
                controller = _connectedControllers[i];
                break;
            }
        }

        // Fail to find
        if (controller == null)
            return;

        // Remove from active
        int controllerPosition = ActiveControllerPositon(controller);
        if (controllerPosition != -1)
        {
            DeactivateController(controller);
        }

        // Reorder active controllers
        Array<InputSystemController> onlyActiveControllers = GetOnlyActiveControllers();
        for(int i = 0; i < _activeControllers.Length; i++)
        {
            if(i < onlyActiveControllers.Count)
            {
                _activeControllers[i] = onlyActiveControllers[i];
            }
            else
            {
                _activeControllers[i] = null;
            }
        }

        EmitSignal(nameof(OnControllerDisconnect), controller);

        // Remove
        _connectedControllers.Remove(controller);
        _contollersContainer.RemoveChild(controller);

        if(_connectedControllers.Count == 0)
        {
            _autoDetectDone = false;
        }
    }

    // Make available controller active = player can use it
    // Return true if was activated, false if active cotroller list is full
    private bool ActivateController(InputSystemController controller){

        for (int i = 0; i < _activeControllers.Length; i++){
            // Take first free slot
            if (_activeControllers[i] == null)
            {
                controller.Activate(true);
                _activeControllers[i] = controller;
                controller.UpdateActiveInputs(this);
                EmitSignal(nameof(OnControllerActivate), controller, i);
                //GD.Print("Activate: " + controller.Name + ", currently active: " + _activeControllers.Length);
                return true;
            }
        }

        //GD.Print("Active controllers limit reached: " + _activeControllers.Length);
        return false;
    }

    // Stop using selected controller
    private void DeactivateController(InputSystemController controller){
        for (int i = 0; i < _activeControllers.Length; i++){
            if (_activeControllers[i] == controller)
            {
                controller.Activate(false);
                _activeControllers[i] = null;
                EmitSignal(nameof(OnControllerDeactivate), controller, i);
                //GD.Print("Deactivate: " + controller.Name + "currently active: " + _activeControllers.Length);
                return;
            }
        }
    }

    // Will make first gamepad active at the start no matter so no joining is required
    // NOTE: Ignores if joining is allowed on not
    private void DevModeSetup(){
        if (!_devModeActive)
            return;

        foreach (InputSystemController controller in _connectedControllers){
            if (controller.GetDeviceType() == EDeviceType.GAMEPAD && controller.GetDeviceId() == 0 && _activeControllers[0] == null){
                ActivateController(controller);
                break;
            }
        }
    }

    //------------------------------------------
    // Public API
    //------------------------------------------

    /// <summary>
    /// Clears the input for the rest of the frame
    /// </summary>
    /// <returns> none
    public void ClearFrameInput()
    {
        foreach(InputSystemController controller in _activeControllers){
            if (controller != null)
                controller.ClearInput(true);
        }
        _frameInputClearTimer = _inputClearTime;
    }

    /// <summary>
    /// Clears the input for the rest of the frame
    /// </summary>
    /// <returns> none
    private void ActivateFrameInput()
    {
        foreach(InputSystemController controller in _activeControllers){
            if (controller != null)
                controller.ClearInput(false);
        }
        _frameInputClearTimer = _inputClearTime;
    }

    /// <summary>
    /// Return list of connected controllers
    /// </summary>
    /// <returns>Return connected controllers list</returns>
    public List<InputSystemController> GetConnectedControllers() { return _connectedControllers; }

    /// <summary>
    /// Return number of connected controllers including keyboards
    /// </summary>
    /// <returns>Return number of connected controllers including keyboards</returns>
    public int GetNumberOfConnectedControllers() 
    { 
        //return _connectedControllers.Count - (_keyboardSchemes.Length - 1); 
        return _connectedControllers.Count;
    }

    /// <summary>
    /// Return list of active controllers slots - will provide also all empty slots
    /// </summary>
    /// <returns>Return active controllers list including empty slots</returns>
    public InputSystemController[] GetActiveControllersSlots() { return _activeControllers; }

    /// <summary>
    /// Select specific active controller slot on selected position. Empty slot and out of bounce position will return null.
    /// </summary>
    /// <returns>Return active controller slot object on selected position</returns>
    public InputSystemController GetConnectedControllerSlot(int position)
    {
        if (position < 0 || position >= _connectedControllers.Count){
            GD.PrintErr("position: " + position + " is not within slots length of: " + _connectedControllers.Count);
            return null;
        }

        return _connectedControllers[position];
    }

    /// <summary>
    /// Select specific active controller slot on selected position. Empty slot and out of bounce position will return null.
    /// </summary>
    /// <returns>Return active controller slot object on selected position</returns>
    public InputSystemController GetActiveControllerSlot(int position)
    {
        if (position < 0 || position >= _activeControllers.Length){
            GD.PrintErr("position: " + position + " is not within active slots lenght of: " + _activeControllers.Length);
            return null;
        }

        return _activeControllers[position];
    }

    /// <summary>
    /// Return list of all active controllers. Empty slots in active controllers list are not included.
    /// </summary>
    /// <returns>Return dynamic list of active controllers</returns>
    public Array<InputSystemController> GetOnlyActiveControllers()
    {
        Array<InputSystemController> active = new Array<InputSystemController>();

        for (int i = 0; i < _activeControllers.Length; i++)
        {
            if (_activeControllers[i] != null)
                active.Add(_activeControllers[i]);
        }

        return active;
    }
    
    public int ActiveControllersCount()
    {
        int count = 0;
        for (int i = 0; i < _activeControllers.Length; i++)
        {
            if (_activeControllers[i] != null)
                count++;
        }

        return count;
    }

    /// <summary>
    /// Check if selected controller is active = stored in active controllers list
    /// </summary>
    /// <returns>Return True if selected controller is stored in active controllers list</returns>
    public bool IsControllerActive(InputSystemController controller)
    {
        for (int i = 0; i < _activeControllers.Length; i++){
            if (_activeControllers[i] == controller)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Check if selected controller is active = stored in active controllers list, but provides exact controller slot
    /// </summary>
    /// <returns>Return slot position if controller is active. Return -1 if controller is not activ .</returns>
    public int ActiveControllerPositon(InputSystemController controller)
    {
        for (int i = 0; i < _activeControllers.Length; i++){
            if (_activeControllers[i] == controller)
                return i;
        }

        return -1;
    }

    /// <summary>
    /// Enable or disable controller activation
    /// While activation is allowed, players can activate one of available devices to use it for in-game controls
    /// WARNING: Disable activate when not neeeded
    /// </summary>
    public void AllowControllerActivation(bool allow)
    {
        _controllerActivationAllowed = allow;
        EmitSignal(nameof(OnControllerActivationAllowed), allow);
    }

    public void DeactivateAllControllers(){
        foreach (InputSystemController controller in _activeControllers){
            if (controller != null)
                DeactivateController(controller);
        }
    }

    // <summary>
    /// Activate only selected input mode. Other modes are deactivated.
    /// </summary>
    public void ActivateSingleInputMode(EInputSystemMode mode) {
        _activeInputMode = 0;
        _activeInputMode = mode;

        foreach(InputSystemController controller in _activeControllers){
            if (controller != null)
                controller.UpdateActiveInputs(this);
        }
    }

    // <summary>
    /// Makes selected input mode active along with any other previously active modes
    /// </summary>
    public void AddActiveInputMode(EInputSystemMode mode){
        _activeInputMode |= mode;

        foreach(InputSystemController controller in _activeControllers){
            if (controller != null)
                controller.UpdateActiveInputs(this);
        }
    }

    // <summary>
    /// Makes selected input mode inactive
    /// </summary>
    public void RemoveActiveInputMode(EInputSystemMode mode){
        _activeInputMode &= ~mode;

        foreach(InputSystemController controller in _activeControllers){
            if (controller != null)
                controller.UpdateActiveInputs(this);
        }
    }

    // <summary>
    /// Makes all input modes inactive
    /// </summary>
    public void ClearActiveInputMode(){
        _activeInputMode = 0;

        foreach(InputSystemController controller in _activeControllers){
            if (controller != null)
                controller.UpdateActiveInputs(this);
        }
    }

    // <summary>
    /// Return true if selected mode is active
    /// </summary>
    public bool IsInputModeActive(EInputSystemMode mode) { return (_activeInputMode & mode) == mode; }

    // <summary>
    /// Moves an active controller to a different open slot in the list if available and sets its old position to empty
    /// </summary>
    public void MoveActiveControllerInList(int oldPosition, int newPosition) 
    { 
        InputSystemController controllerToReplace = GetActiveControllerSlot(oldPosition);
        if(controllerToReplace != null &&  _activeControllers[newPosition] == null) // make sure we are not overwriting an existing controller
        {
            _activeControllers[newPosition] = controllerToReplace;
            _activeControllers[oldPosition] = null;
        }
    }

    public EInputSystemMode GetActiveInputModes() { return _activeInputMode; }
}
