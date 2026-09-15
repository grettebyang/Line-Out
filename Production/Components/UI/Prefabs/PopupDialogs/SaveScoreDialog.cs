using Godot;
using System;
using System.Data.SqlTypes;

public partial class SaveScoreDialog : Control
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
    [Export] protected LineEdit _editTeamName;
    [Export] protected float _closeDelay = 0.1f;
    [Export] protected AudioStreamPlayer _openAudioPlayer;
    [Export] protected OnScreenKeyboard _osk;

    protected float _closeTimer;
    protected float _openTimer;
    protected int _selected;

    private string _prevEditTeamNameText;

    [Signal] public delegate void OnPositiveConfirmEventHandler(SaveScoreDialog dialog);
    [Signal] public delegate void OnNegativeConfirmEventHandler(SaveScoreDialog dialog);
    [Signal] public delegate void OnClosedEventHandler(SaveScoreDialog dialog);
    public delegate void GrabCursors(Vector2 pos);
    public event GrabCursors OpenSaveScoreDialog;

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

        // Resize buttons

        _prevEditTeamNameText = _editTeamName.Text;
        _editTeamName.TextChanged += OnEditTeamNameChange;

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
                _btnPositive.ProcessMode = ProcessModeEnum.Disabled;
                _osk.Visible = false;
                _osk.ProcessMode = ProcessModeEnum.Disabled;
            }

        }

        if (_openTimer > 0){
            _openTimer -= (float)delta;
            if (_openTimer <= 0){
                OpenSaveScoreDialog?.Invoke(_btnNegative.GlobalPosition + _btnNegative.Size/2);
                _osk.Visible = true;
                _osk.ProcessMode = ProcessModeEnum.Inherit;
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

    private static SaveScoreDialog _instance;

    private SaveScoreDialog()
	{
		if (_instance != null)
            return;

		_instance = this;
	}

    public static SaveScoreDialog GetInstance()
	{
		return _instance;
	}

    //------------------------------------------
    // Custom
    //------------------------------------------

    private void OnKeyboardButtonEntered(int value)
    {
        char character = (char)value;

        _editTeamName.Text += character;
        _editTeamName.CaretColumn = _editTeamName.Text.Length;

        _btnPositive.ProcessMode = ProcessModeEnum.Inherit;
    }

    private void OnBackspaceEntered()
    {
        _editTeamName.Text = _editTeamName.Text.Remove(_editTeamName.Text.Length - 1);
        _editTeamName.CaretColumn = _editTeamName.Text.Length;

        if(_editTeamName.Text.Length == 0)
        {
            _btnPositive.ProcessMode = ProcessModeEnum.Disabled;
        }
    }

    private void OnEditTeamNameChange(string text)
    {
        if (text.Length == 0)
        {
            _btnPositive.ProcessMode = ProcessModeEnum.Disabled;
            return;
        }

        // Max lenght
        // if (text.Length > 20)
        // {
        //     _editTeamName.Text = _prevEditTeamNameText;
        //     return;
        // }

        // Deleting
        int dif = text.Length - _prevEditTeamNameText.Length;
        if (dif < 1)
        {
            _prevEditTeamNameText = _editTeamName.Text;
            if(_editTeamName.Text.Length > 0)
            {
                _btnPositive.ProcessMode = ProcessModeEnum.Inherit;
            }
            return;
        }

        for (int i = _prevEditTeamNameText.Length; i < text.Length; i++)
        {
            char c = text[i];

            GD.Print("Try prevent: " + c);

            // Check if any of invalid characters was entered
            if(c == '(' || c == ')' || c == '{' || c == '}' || c == '[' || c == ']' 
                || c == ';' || c == '.' || c == ',' || c == '?' || c == '/' || c == '\\' 
                || c == '+' || c == '-' || c == '*' || c == '\'' || c == ':' || c == '!'
                || c == '"' || c == '~' || c == '`')
            {
                _editTeamName.Text = _prevEditTeamNameText;
                _editTeamName.CaretColumn = text.Length - 1;
                if(_editTeamName.Text.Length > 0)
                {
                    _btnPositive.ProcessMode = ProcessModeEnum.Inherit;
                }
                return;
            }
        }

        _prevEditTeamNameText = _editTeamName.Text;
        if(_editTeamName.Text.Length > 0)
        {
            _btnPositive.ProcessMode = ProcessModeEnum.Inherit;
        }
    }

    protected void OnPositiveButtonPressed(PlayerCurser cursor, int buttonId){
        if (_editTeamName.Text.Length <= 0)
            return;

        float time = GameManager.GetInstance().CurrentLevel._sessionTime;
        string name = _editTeamName.Text;
        int teamSize = GameManager.GetInstance().CurrentLevel._sled.DogSpawner.DogList.Count;

        TempScoreboardRecord record = new TempScoreboardRecord(time, name, teamSize);
        GameManager.GetInstance().TempScoreboard.SaveRecord(record);

        EmitSignal(nameof(OnPositiveConfirm), this);
        Show(false);
    }

    protected void OnPositiveButtonHover(PlayerCurser cursor, int buttonId)
    {
        _selected = buttonId;
    }

    protected void OnNegativeButtonPressed(PlayerCurser cursor, int buttonId){
        EmitSignal(nameof(OnNegativeConfirm), this);
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
        _editTeamName.GrabFocus();
        _editTeamName.Text = "";

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
