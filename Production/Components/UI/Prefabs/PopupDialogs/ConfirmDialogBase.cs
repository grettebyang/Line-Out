using Godot;
using System;
using System.Data.SqlTypes;

public enum EConfirmDialogStyle{
    NONE,
    OK,
    YES_NO,
    OK_CANCEL,
}

public partial class ConfirmDialogBase : Control
{
    public const String BTN_CONTROLS_POSITIVE = "A";
    public const String BTN_CONTROLS_NEGATIVE = "B";

    public const String BTN_TEXT_OK = "Ok";
    public const String BTN_TEXT_CANCEL = "Cancel";
    public const String BTN_TEXT_YES = "Yes";
    public const String BTN_TEXT_NO = "No";

    [Export] protected Label _lblMessage;
    [Export] protected CursorButton _btnPositive;
    [Export] protected CursorButton _btnNegative;
    [Export] protected float _closeDelay = 0.1f;
    [Export] protected AudioStreamPlayer _openAudioPlayer;

    protected float _closeTimer;
    protected float _openTimer;
    protected int _selected;

    [Signal] public delegate void OnPositiveConfirmEventHandler(ConfirmDialogBase dialog);
    [Signal] public delegate void OnNegativeConfirmEventHandler(ConfirmDialogBase dialog);
    [Signal] public delegate void OnClosedEventHandler(ConfirmDialogBase dialog);
    public delegate void GrabCursors(Vector2 pos);
    public event GrabCursors OpenConfirmDialog;

    // Properties
    public string Message {
        get { return _lblMessage.Text; }
        set{
            _lblMessage.Text = value;
        }
    }

    public override void _Ready(){
        Visible = false;
        //Position = GetWindow().Size/2 - Size/2;

        foreach(PlayerCurser cursor in PlayerCurserContainer.GetInstance().CurserList)
        {
            CursorInputHookup(cursor);   
        }
        PlayerCurserContainer.GetInstance().OnPlayerJoined += CursorInputHookup;
    }

    public void CursorInputHookup(PlayerCurser cursor)
    {
        cursor.BackPressed += OnReturnButtonPressed;
    }

    public void ResizeButtons()
    {
        _btnNegative.ResizeCollisionShape();
        _btnPositive.ResizeCollisionShape();
    }

    public void DisableButtons()
    {
        _btnNegative.ProcessMode = ProcessModeEnum.Disabled;
        _btnPositive.ProcessMode = ProcessModeEnum.Disabled;        
    }

    public override void _Process(double delta){
        if (_closeTimer > 0){
            _closeTimer -= (float)delta;
            if (_closeTimer <= 0){
                 EmitSignal(nameof(OnClosed), this);
                 Visible = false;
                 //DisableButtons();
            }

        }

        if (_openTimer > 0){
            _openTimer -= (float)delta;
            if (_openTimer <= 0){
                // _btnNegative.ProcessMode = ProcessModeEnum.Inherit;
                // _btnPositive.ProcessMode = ProcessModeEnum.Inherit;
                OpenConfirmDialog?.Invoke(_btnNegative.GlobalPosition + _btnNegative.Size/2);
            }

        }

    }


    public void OnReturnButtonPressed(PlayerCurser cursor){
        if (!Visible)
            return;

        Show(false);
    }

    //------------------------------------------
    // Singleton
    //------------------------------------------

    private static ConfirmDialogBase _instance;

    private ConfirmDialogBase()
	{
		if (_instance != null)
            return;

		_instance = this;
	}

    public static ConfirmDialogBase GetInstance()
	{
		return _instance;
	}

    //------------------------------------------
    // Custom
    //------------------------------------------

    protected void OnPositiveButtonPressed(PlayerCurser cursor, int buttonId){
        EmitSignal(nameof(OnPositiveConfirm), this);
        //GD.Print("positive pressed");
        Show(false);
    }

    protected void OnPositiveButtonHover(PlayerCurser cursor, int buttonId)
    {
        _selected = buttonId;
    }

    protected void OnNegativeButtonPressed(PlayerCurser cursor, int buttonId){
        EmitSignal(nameof(OnNegativeConfirm), this);
        //GD.Print("negative pressed");
        Show(false);
    }

    protected void OnNegativeButtonHover(PlayerCurser cursor, int buttonId)
    {
        _selected = buttonId;
    }

    //------------------------------------------
    // Public API
    //------------------------------------------

    public void Show(bool show, EConfirmDialogStyle style = EConfirmDialogStyle.NONE, string message = "_"){
        _openAudioPlayer.Play();
        
        if (!show){
            _closeTimer = _closeDelay;
            return;
        }

        _openTimer = _closeDelay;
        Visible = true;

        // Style
        switch (style){
            case EConfirmDialogStyle.OK:{
                _btnPositive.SetText(BTN_TEXT_OK);
                _btnNegative.Visible = false;
                break;
            }

            case EConfirmDialogStyle.OK_CANCEL:{
                _btnPositive.SetText(BTN_TEXT_OK);
                _btnNegative.Visible = true;
                _btnNegative.SetText(BTN_TEXT_CANCEL);
                break;
            }

            case EConfirmDialogStyle.YES_NO:{
                _btnPositive.SetText(BTN_TEXT_YES);
                _btnNegative.Visible = true;
                _btnNegative.SetText(BTN_TEXT_NO);
                break;
            }
        }

        // Message
        if (message != "_")
            Message = message;

        _selected = 1;
    }
}
