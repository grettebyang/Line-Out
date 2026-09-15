/*
For all active controllers (players in game) checks if all are ready to play.
E.g. 
    - 3 players are joined
    - wait for all to press A
    - when everybody press A, wait 3 secs to start game
*/

using Godot;
using Godot.Collections;
using System;


public partial class PlayersReadyConfirmator : Node
{
    private const string INPUT_CONFIRM = "confirm";
    const int CONFIRM_DELAY = 3; // Wait x seconds before confirming and closing

    Array<bool> _readyPlayers = new Array<bool>();
    InputSystem _inputSystem;

    float _confirmDelayTimer = 0;

    [Signal] public delegate void OnConfirmEventHandler();
    [Signal] public delegate void OnPlayerConfirmEventHandler(bool confirmed, int id);

    public override void _Ready()
    {
        _inputSystem = InputSystem.GetInstance();
    }

    public override void _Process(double delta)
    {
        // Handle confirm
        InputSystemController[] controllers = _inputSystem.GetActiveControllersSlots();

        for (int i = 0; i < controllers.Length; i++)
        {
            if (controllers[i] == null)
            {
                _readyPlayers[i] = true; // Skip empty slot
                continue;
            }

            // Check confirm action
            if (controllers[i].IsJustPressed(INPUT_CONFIRM))
            {
                _readyPlayers[i] = !_readyPlayers[i];
                EmitSignal(nameof(OnPlayerConfirm), _readyPlayers[i], i);
            }
        }

        // Check all confirmed
        bool allConfirmed = true;

        foreach (bool confirm in _readyPlayers)
        {
            if (!confirm)
            {
                allConfirmed = false;
                break;
            }
        }

        if (allConfirmed)
        {
            if (_confirmDelayTimer > 0)
                _confirmDelayTimer -= (float)delta;
            else
                Confirm();
        }
        else
        {
            _confirmDelayTimer = CONFIRM_DELAY;
        }
    }

    public void SetupConfirmList()
    {
        _readyPlayers.Clear();

        InputSystemController[] controllers = _inputSystem.GetActiveControllersSlots();
        foreach (InputSystemController ctrl in controllers)
        {
            _readyPlayers.Add(false);
        }
    }
    
    /// <summary>
    /// Will deactivate joining system and menu, disable joining and reactivate player input
    /// </summary>
    public void Confirm()
    {
        _inputSystem.ActivateSingleInputMode(EInputSystemMode.PLAYER_MOVEMENT); // TODO: Check this is correct setting?
        EmitSignal(nameof(OnConfirm));
    }
}
