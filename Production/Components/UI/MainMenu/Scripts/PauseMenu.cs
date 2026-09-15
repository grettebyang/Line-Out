using Godot;
using System;

public partial class PauseMenu : MenuWrapper
{
    public const string PAUSE_MENU_PATH = "res://Production/Components/UI/MainMenu/Scenes/PauseMenu.tscn";
    public const string ACTION_PAUSE_MENU = "PauseMenuToggle";

    public const string MSG_RESTART = "Restart the level?";
    public const string MSG_RESPAWN_DOGS = "Respawn dogs to sled?";
    public const string MSG_QUIT = "Go back to the main menu?";

    protected BasicTimeManager.TIMESTATE _lastState;
    protected bool _canTogglePause = false;

    [Export] SettingsSubmenu _settingsMenu;

    //------------------------------------------
    // Override methods
    //------------------------------------------

    public override void _Ready(){
        base._Ready();
        Visible = false;
        ProcessMode = ProcessModeEnum.Disabled;

        GameManager.GetInstance().CurrentLevel.OnPauseToggle += TogglePause;
        _settingsMenu._onSettingsClosed += OnSettingsClosed;
        
    }

    public override void _Process(double delta)
    {
        base._Process(delta);        
        // Toggle pause
        if (_canTogglePause && Input.IsActionJustPressed(ACTION_PAUSE_MENU))
        {
            TogglePause();
        }
    }

    //------------------------------------------
    // Custom
    //------------------------------------------

    private void TogglePause(){
        if (!Visible)
            CallDeferred("Open");
        else
            OnContinue(null, 0);
    }

    private void Close(){
        EmitSignal(nameof(OnClose));
        BasicTimeManager.GetInstance().State = _lastState;
        GD.Print("Close pause1");
        Visible = false;
        ProcessMode = ProcessModeEnum.Disabled;
        if(GameManager.GetInstance().CurrentLevel != null)
            GameManager.GetInstance().CurrentLevel.ProcessMode = ProcessModeEnum.Inherit;
        _canTogglePause = false;
    }

    // Continue
    private void OnContinue(PlayerCurser cursor, int buttonId){
        Close();
        RhythmManager.GetInstance().Unpause();
        InputSystem.GetInstance().RemoveActiveInputMode(EInputSystemMode.MENU);
        //InputSystem.GetInstance().AddActiveInputMode(EInputSystemMode.PLAYER_MOVEMENT);
        InputSystem.GetInstance().ClearFrameInput();
        PlayerCurserContainer.GetInstance().HideCursors();
    }

    // Respawn Dogs
    private void OnRespawnDogs(PlayerCurser cursor, int buttonId){
        ConfirmDialogBase dialog = ConfirmDialogBase.GetInstance();
        dialog.Show(true, EConfirmDialogStyle.YES_NO, MSG_RESPAWN_DOGS);
        dialog.OnPositiveConfirm += OnRespawnDogsDialogConfirm;
        dialog.OnClosed += OnRespawnDogsDialogClose;
        //GameManager.GetInstance().CurrentLevel.ProcessMode = ProcessModeEnum.Disabled;
        _canTogglePause = false;
    }

    private void OnRespawnDogsDialogConfirm(ConfirmDialogBase dialog)
    {
        // Respawn dogs to sled
        GameManager.GetInstance().CurrentLevel._sled.RespawnDogsAtSled();

        // Close dialog
        dialog.OnPositiveConfirm -= OnRespawnDogsDialogConfirm;
        dialog.OnClosed -= OnRespawnDogsDialogClose;
        //GameManager.GetInstance().CurrentLevel.ProcessMode = ProcessModeEnum.Inherit;
        _canTogglePause = true;
        
        // Close pause menu
        OnContinue(null, -1);
    }

    private void OnRespawnDogsDialogClose(ConfirmDialogBase dialog)
    {
        dialog.OnPositiveConfirm -= OnRespawnDogsDialogConfirm;
        dialog.OnClosed -= OnRespawnDogsDialogClose;
        //GameManager.GetInstance().CurrentLevel.ProcessMode = ProcessModeEnum.Inherit;
        _canTogglePause = true;
    }

    // Restart
    private void OnRestart(PlayerCurser cursor, int buttonId){
        ConfirmDialogBase dialog = ConfirmDialogBase.GetInstance();
        dialog.Show(true, EConfirmDialogStyle.YES_NO, MSG_RESTART);
        dialog.OnPositiveConfirm += OnRestartDialogConfirm;
        dialog.OnClosed += OnRestartDialogClose;
        //GameManager.GetInstance().CurrentLevel.ProcessMode = ProcessModeEnum.Disabled;
        _canTogglePause = false;
    }

    private void OnRestartDialogConfirm(ConfirmDialogBase dialog){
        //dialog.Show(false);
        dialog.OnPositiveConfirm -= OnRestartDialogConfirm;
        dialog.OnClosed -= OnRestartDialogClose;
        GameManager.GetInstance().ReloadLevel();
        //GameManager.GetInstance().CurrentLevel.ProcessMode = ProcessModeEnum.Inherit;
        InputSystem.GetInstance().RemoveActiveInputMode(EInputSystemMode.MENU);
        PlayerCurserContainer.GetInstance().HideCursors();

        Close();
    }

    private void OnRestartDialogClose(ConfirmDialogBase dialog){
        dialog.OnPositiveConfirm -= OnRestartDialogConfirm;
        dialog.OnClosed -= OnRestartDialogClose;
        //GameManager.GetInstance().CurrentLevel.ProcessMode = ProcessModeEnum.Inherit;
        _canTogglePause = true;
    }

    // Settings
    private void OnSettings(PlayerCurser cursor, int buttonId)
    {
        OpenSubmenu(_settingsMenu, true, true);
        //GameManager.GetInstance().CurrentLevel.ProcessMode = ProcessModeEnum.Disabled;
        _canTogglePause = false;
    }

    private void OnSettingsClosed()
    {
        _canTogglePause = true;
    }

    // Quit
    private void OnQuit(PlayerCurser cursor, int buttonId){
        ConfirmDialogBase dialog = ConfirmDialogBase.GetInstance();
        dialog.Show(true, EConfirmDialogStyle.YES_NO, MSG_QUIT);
        dialog.OnPositiveConfirm += OnQuitDialogConfirm;
        dialog.OnClosed += OnQuitDialogClose;
        _canTogglePause = false;
    }

    private void OnQuitDialogConfirm(ConfirmDialogBase dialog){
        //dialog.Show(false);
        dialog.OnPositiveConfirm -= OnQuitDialogConfirm;
        dialog.OnClosed -= OnQuitDialogClose;
        GameManager.GetInstance().LoadMenu(true);

        Close();
    }

    private void OnQuitDialogClose(ConfirmDialogBase dialog){
        dialog.OnPositiveConfirm -= OnQuitDialogConfirm;
        dialog.OnClosed -= OnQuitDialogClose;
        _canTogglePause = true;
    }

    //------------------------------------------
    // Public
    //------------------------------------------

    public void Open(){

        GameManager.GetInstance().CurrentLevel.ProcessMode = ProcessModeEnum.Disabled;
        _canTogglePause = true;
        _lastState = BasicTimeManager.GetInstance().State;
        BasicTimeManager.GetInstance().State = BasicTimeManager.TIMESTATE.MENU;
        GD.Print("Open pause1");
        RhythmManager.GetInstance().Pause();
        InputSystem.GetInstance().AddActiveInputMode(EInputSystemMode.MENU);
        //InputSystem.GetInstance().RemoveActiveInputMode(EInputSystemMode.PLAYER_MOVEMENT);
        Visible = true;
        ProcessMode = ProcessModeEnum.Always;
        PlayerCurserContainer.GetInstance().ShowCursors();
    }

    public static ConfirmDialogBase RestartDialog(){
        ConfirmDialogBase dialog = ConfirmDialogBase.GetInstance();
        dialog.Show(true, EConfirmDialogStyle.YES_NO, MSG_RESTART);
        return dialog;
    }

    public static ConfirmDialogBase QuitDialog(){
        ConfirmDialogBase dialog = ConfirmDialogBase.GetInstance();
        dialog.Show(true, EConfirmDialogStyle.YES_NO, MSG_QUIT);
        return dialog;
    }
}
