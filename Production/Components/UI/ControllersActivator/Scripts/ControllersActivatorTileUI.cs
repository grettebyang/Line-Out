using Godot;
using Godot.Collections;
using System;
using System.Linq;

public enum EControllerActivationState{
    DISCONNECTED,
    JOIN,
    CONFIRM,
    ACTIVE
}

public partial class ControllersActivatorTileUI : Control
{
    /*
    [Export]
    private TextureRect _background;
    [Export]
    private Control _joinFace;
    [Export]
    private Control _activeFace;
    [Export]
    private Label _activeHint;
    [Export]
    private TextureRect _colorIndicator;
    */

    [Export] protected Control _parentModulate;
    [Export] protected Label _lblName;
    [Export] protected Control _joinHint;
    [Export] protected Control _faceDisconnected;
    [Export] protected Control _faceJoin;
    [Export] protected Control _faceConfirm;
    [Export] protected Control _faceActive;

    [Export] private int _playerId = -1;
    private Color _colorCode;
    private EControllerActivationState _state;

    //------------------------------------------
    // Override methods
    //------------------------------------------

    public override void _Ready(){
        // Apply style
        if (_playerId != -1)
            _colorCode = UIConstants.PLAYER_COLORS[_playerId];
        _parentModulate.Modulate = _colorCode;
        _joinHint.SelfModulate = _colorCode;
        SetState(EControllerActivationState.DISCONNECTED);
    }

    //------------------------------------------
    // Public API
    //------------------------------------------

    public void SetState(EControllerActivationState state){
        _state = state;

        // Adjust UI
        switch (_state)
        {
            // Disconnected
            case EControllerActivationState.DISCONNECTED:
            _faceDisconnected.Visible = true;
            _faceJoin.Visible = false;
            _faceConfirm.Visible = false;
            _faceActive.Visible = false;

            _joinHint.Visible = false;
            break;

            // Join
            case EControllerActivationState.JOIN:
            _faceDisconnected.Visible = false;
            _faceJoin.Visible = true;
            _faceConfirm.Visible = false;
            _faceActive.Visible = false;

            _joinHint.Visible = true;
            break;

            // Confirm
            case EControllerActivationState.CONFIRM:
            _faceDisconnected.Visible = false;
            _faceJoin.Visible = false;
            _faceConfirm.Visible = true;
            _faceActive.Visible = false;

            _joinHint.Visible = true;
            break;

            // Active
            case EControllerActivationState.ACTIVE:
            _faceDisconnected.Visible = false;
            _faceJoin.Visible = false;
            _faceConfirm.Visible = false;
            _faceActive.Visible = true;

            _joinHint.Visible = false;
            break;
        }
    }

    public void SetPlayerId(int id){
        _playerId = id;
        _colorCode = UIConstants.PLAYER_COLORS[_playerId];
        _parentModulate.Modulate = _colorCode;
        _joinHint.SelfModulate = _colorCode;

        _lblName.Text = UIConstants.PLAYER_NAMES[_playerId];
    }

    public EControllerActivationState GetState()
    {
        return _state;
    }
}
