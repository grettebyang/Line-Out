using Godot;
using System;
using System.ComponentModel;

public partial class PlayerCurser : CharacterBody2D
{
    private InputSystemController _inputController;
    public int Id;

    public Vector2 inputVelocity;
    [Export]
    public float _cursorSpeed;
    [Export]
    public CollisionShape2D _collisionShape;
    [Export]
    public Control _icon;
    [Export]
    public Control _paw;

    private Color _colorCode;
    private bool _active; // cursor cannot register any input
    private bool _freeze = false; // cursor cannot move, but can press other buttons
    private bool _ready;
    public bool _disableNav = false;
    private int _characterSelected = -1;
    private CursorButton _buttonHovering;
    private Vector2 _lastPos;
    private uint _lastCollisionLayer = 1;

    public bool _justOpened;
    public float _delayTimer = 0;
    public float _delayTime = .1f;

    [Signal] public delegate void OnCharacterSelectedEventHandler();
    public delegate void OnButtonPressed(PlayerCurser cursor);
    public event OnButtonPressed ButtonPressed;
    public event OnButtonPressed ButtonReleased;
    public event OnButtonPressed BackPressed;
    public event OnButtonPressed BackReleased;
    public event OnButtonPressed ButtonUnPressed;

    public override void _Ready()
    {
        ConfirmDialogBase.GetInstance().OpenConfirmDialog += OnOpenConfirmDialog;
        ConfirmDialogBase.GetInstance().OnClosed += ReturnToLastPositionOnConfirmDialogClose;
        SaveScoreDialog.GetInstance().OpenSaveScoreDialog += OnOpenSaveScoreDialog;
        SaveScoreDialog.GetInstance().OnClosed += ReturnToLastPositionOnSaveDialogClose;
    }

    public override void _ExitTree()
    {
        ConfirmDialogBase.GetInstance().OpenConfirmDialog -= OnOpenConfirmDialog;
        ConfirmDialogBase.GetInstance().OnClosed -= ReturnToLastPositionOnConfirmDialogClose;
        SaveScoreDialog.GetInstance().OpenSaveScoreDialog -= OnOpenSaveScoreDialog;
        SaveScoreDialog.GetInstance().OnClosed -= ReturnToLastPositionOnSaveDialogClose;
    }

    public override void _Process(double delta)
    {
        if (_inputController == null)
        {
            GD.Print("no input controller set for cursor " + Id);
            return;
        }
        if(_active && !_justOpened)
        {
            if(!_disableNav)
            {
                HandleInput((float)delta);
            }
            if(!_freeze)
            {
                HandleMovement((float)delta);
            }
        }
        _justOpened = false;
    }
    public void HandleInput(float delta)
    {
        if(_inputController.IsJustPressed("confirm"))
        {
            ButtonPressed?.Invoke(this);
            GD.Print("Player " + (Id + 1) + " confirm pressed");
        }
        else if(_inputController.IsJustPressed("return"))
        {
            // Deselect character if one is selected
            BackPressed?.Invoke(this);
            GD.Print("Player " + (Id + 1) + " back pressed");
        }
        else if(_inputController.IsJustReleased("return"))
        {
            // Deselect character if one is selected
            BackReleased?.Invoke(this);
            GD.Print("Player " + (Id + 1) + " back released");
        }
        else if(_inputController.IsJustReleased("confirm"))
        {
            // Deselect character if one is selected
            ButtonReleased?.Invoke(this);
            GD.Print("Player " + (Id + 1) + " button released");
        }
    }

    public void HandleMovement(float delta)
    {
        inputVelocity.X = _inputController.GetInputAxis("west", "east");
        inputVelocity.Y = _inputController.GetInputAxis("south", "north");

        Position += inputVelocity * _cursorSpeed * delta;
        Position = Position.Clamp(Vector2.Zero, PlayerCurserContainer.GetInstance().Size - _icon.Size);        
    }

    public void DeselectCharacter()
    {
        GameManager.GetInstance().SetPlayerCharacter(Id, -1);
        if(_characterSelected != -1)
        {
            //_characterSelected.Deselect();
        }
    }

    public void SetInputController(InputSystemController controller, int id)
    {
        _inputController = controller;
        Id = id;
    }

    public void SetPlayerId(int id)
    {
        Id = id;
        _colorCode = UIConstants.PLAYER_COLORS[id];
        Modulate = _colorCode;
    }

    public void SetActive(bool active)
    {
        _active = active;
    }

    public void Freeze(bool freeze)
    {
        _freeze = freeze;
    }

    public bool IsFrozen()
    {
        return _freeze;
    }

    public void SetReady(bool ready)
    {
        _ready = ready;
    }

    public InputSystemController GetController()
    {
        return _inputController;
    }

    public void SetCharacterSelected(int characterId)
    {
        _characterSelected = characterId;
    }

    public int GetCharacterSelected()
    {
        if(_characterSelected != -1)
        {
            return _characterSelected;
        }
        return -1;
    }

    public void ChangePlayerId(int id)
    {
        SetPlayerId(id);
        if(_characterSelected != -1 && _ready)
        {
            //_characterSelected.Select(id);
        }
    }

    public bool HasSelection()
    {
        return _ready;
    }

    private void OnOpenConfirmDialog(Vector2 pos) // Sets cursor position to the negative button option on a confirm dialog popup when it opens
    {
        GD.Print("open confirm dialog");
        _lastPos = Position;
        _lastCollisionLayer = CollisionLayer;
        Position = pos;
        SetCollisionLayerValue(1, false);
        SetCollisionLayerValue(2, true);
    }

    private void OnOpenSaveScoreDialog(Vector2 pos) // Sets cursor position to the negative button option on a confirm dialog popup when it opens
    {
        _lastPos = Position;
        _lastCollisionLayer = CollisionLayer;
        Position = pos;
        SetCollisionLayerValue(1, false);
        SetCollisionLayerValue(3, true);
    }

    private void ReturnToLastPositionOnConfirmDialogClose(ConfirmDialogBase dialog)
    {
        GD.Print("confirm dialog close");
        Position = _lastPos;
        SetCollisionLayerValue(1, true);
        SetCollisionLayerValue(2, false);
        // CollisionLayer = _lastCollisionLayer;
        // GD.Print("Collision layer = " + CollisionLayer);
    }

    private void ReturnToLastPositionOnSaveDialogClose(SaveScoreDialog dialog)
    {
        Position = _lastPos;
        SetCollisionLayerValue(1, true);
        SetCollisionLayerValue(3, false);
        // CollisionLayer = _lastCollisionLayer;
        // GD.Print("Collision layer = " + CollisionLayer);
    }

    public void DisableCursorNavigation()
    {
        _collisionShape.SetDeferred("disabled", true);
        _disableNav = true;
        Modulate = Colors.Gray;
        _paw.SelfModulate = _colorCode;
    }

    public void EnableCursorNavigation()
    {
        _collisionShape.SetDeferred("disabled", false);
        _disableNav = false;
        Modulate = _colorCode;
        _paw.SelfModulate = Colors.White;
    }
}
