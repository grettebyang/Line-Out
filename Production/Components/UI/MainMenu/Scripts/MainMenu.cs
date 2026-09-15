using Godot;
using Godot.Collections;
using System;
using System.ComponentModel.Design;
using System.Xml.Linq;

public partial class MainMenu : MenuWrapper
{
    public const String ACTION_CONFIRM = "MenuConfirm";
    public const String ACTION_CANCEL = "MenuCancel";
    public const string MESSAGE_EXIT = "Do you want to leave the game?";

    [ExportGroup("Submenus")]
    [Export] protected LevelSelectionSubmenu _levelsMenu;
    [Export] protected SubmenuBase _scoreboard;
    [Export] protected SubmenuBase _multiplayerMenu;
    [Export] protected SubmenuBase _settingsMenu;
    [Export] protected SubmenuBase _creditsMenu;
    [Export] protected PlayerSelectionSubmenu _playerSelectionMenu;
    [Export] protected RhythmCalibrationSubmenu _rhythmCalibrationMenu;
    private InputSystem _inputSystem;
    private float _loadingTime = 0.0f;
    [Export] protected ControllersActivatorUI _controllersActivatorUI;

    //------------------------------------------
    // Override
    //------------------------------------------
    public override void _Process(double delta)
    {
        base._Process(delta);
        if(_loadingTime > 0.0f)
        {
            _loadingTime -= (float)delta;
            if(_loadingTime <= 0.0f)
            {
                LoadingScreen.GetInstance().HideLoading();
            }
        }
    }


    public override void _Ready(){
        _levelsMenu.Visible = false;
        _levelsMenu.OnSelectLevel += OnSelectLevel;

        _playerSelectionMenu.OnCharacterSelectionComplete += OnCharacterSelectionCompleteReturn;

        _inputSystem = InputSystem.GetInstance();
        _inputSystem.AllowControllerActivation(false); // This is only here because for debugging purposes it starts up the rhythm manager
        _inputSystem.AddActiveInputMode(EInputSystemMode.MENU);

        base._Ready();
        _backButton.Visible = false;

        RhythmManager.GetInstance().OnRhythmCalibrationComplete += OnRhythmCalibrationComplete;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        _levelsMenu.OnSelectLevel -= OnSelectLevel;
        _playerSelectionMenu.OnCharacterSelectionComplete -= OnCharacterSelectionCompleteReturn;
        RhythmManager.GetInstance().OnRhythmCalibrationComplete -= OnRhythmCalibrationComplete;
    }

    protected override void OpenSubmenu(SubmenuBase submenu, bool resetFocus = false, bool savePrevious = false){
        base.OpenSubmenu(submenu, resetFocus, savePrevious);
        _backButton.Visible = (_menuHistory.Count - 1 >= 0) && !_holdToGoBack; 
        _backButtonHold.Visible = _holdToGoBack;
        _confirmButton.Visible = submenu._confirmHintButtonVisible;

        _controllersActivatorUI.Visible = submenu._showControllerActivator;
        _inputSystem.AllowControllerActivation(submenu._showControllerActivator);
    }

    protected override void OnBackPressed()
    {
        // Go back to previous menu
        int last = _menuHistory.Count - 1;

        if (last == -1)
        {
            OnQuitPressed(null, 0);
            GD.Print("OnQuitPressed");
            return;
        }

        base.OnBackPressed();
        _backButton.Visible = (_menuHistory.Count > 0);
    }

    //------------------------------------------
    // Custom
    //------------------------------------------

    // Levels selection
    public void OnLevelsPressed(PlayerCurser cursor, int buttonId)
    {
        OpenSubmenu(_levelsMenu, true, true);
    }

    private void OnScoreboardPressed(PlayerCurser cursor, int buttonId)
    {
        OpenSubmenu(_scoreboard, true, true);
    }

    private void OnSelectLevel(string path)
    {
        // If players are not selected, go to player selection screen
        InputSystemController[] cons = _inputSystem.GetActiveControllersSlots();
        for(int i = 0; i < cons.Length; i++)
        {
            if(_inputSystem.GetActiveControllerSlot(i) != null && GameManager.GetInstance().GetPlayerCharacter(i) == -1)
            {
                OnPlayerSelectionPressed(null, 0);
                _playerSelectionMenu.OnCharacterSelectionComplete -= OnCharacterSelectionCompleteReturn;
                _playerSelectionMenu.OnCharacterSelectionComplete += OnCharacterSelectionCompleteStartLevel;
                return;
            }
        }
        GoToLevel(path);
    }

    private void GoToLevel(string path)
    {
        _inputSystem.AllowControllerActivation(false);
        _inputSystem.RemoveActiveInputMode(EInputSystemMode.MENU);
        GameManager.GetInstance().LoadLevel(path);
        PlayerCurserContainer.GetInstance().HideCursors();
        QueueFree();
    }

    // Multiplayer
    private void OnMultiplayerPressed(PlayerCurser cursor, int buttonId)
    {
        OpenSubmenu(_multiplayerMenu, true, true);
    }

    // Settings 
    private void OnSettingsPressed(PlayerCurser cursor, int buttonId)
    {
        OpenSubmenu(_settingsMenu, true, true);
    }

    // Credits
    private void OnCreditsPressed(PlayerCurser cursor, int buttonId){
        OpenSubmenu(_creditsMenu, true, true);
        _confirmButton.Visible = false;
    }

    // Player Selection
    private void OnPlayerSelectionPressed(PlayerCurser cursor, int buttonId){
        OpenSubmenu(_playerSelectionMenu, true, true);
    }

    private void OnCharacterSelectionCompleteStartLevel()
    {
        _playerSelectionMenu.OnCharacterSelectionComplete -= OnCharacterSelectionCompleteStartLevel;
        _playerSelectionMenu.Close();
        GoToLevel(_levelsMenu.GetLevelPath());
    }

    private void OnCharacterSelectionCompleteReturn()
    {
        // Go back to previous menu
        int last = _menuHistory.Count - 1;
        if (last == -1)
        {
            OnQuitPressed(null, 0);
            return;
        }

        OpenSubmenu(_menuHistory[last]);
        _menuHistory.RemoveAt(last);
        _backButton.Visible = (_menuHistory.Count > 0);
    }

    // Rhythm Calibration
    private void OnRhythmCalibrationPressed(PlayerCurser cursor, int buttonId)
    {
        OpenSubmenu(_rhythmCalibrationMenu, true, true);
    }

    private void OnRhythmCalibrationComplete()
    {
        _holdToGoBack = false;
        _backButton.Visible = true;
        _backButtonHold.Visible = false;
    }

    // Quit
    private void OnQuitPressed(PlayerCurser cursor, int buttonId){
        ConfirmDialogBase dialog = ConfirmDialogBase.GetInstance();
        dialog.Show(true, EConfirmDialogStyle.YES_NO, MESSAGE_EXIT);
        dialog.OnClosed += OnQuitDialogClosed;
        dialog.OnPositiveConfirm += OnQuitConfirm;
        _activeSubmenu._canControl = false;
    }

    private void OnQuitConfirm(ConfirmDialogBase dialog){
        GetTree().Quit();
    }

    private void OnQuitDialogClosed(ConfirmDialogBase dialog){
        dialog.OnClosed -= OnQuitDialogClosed;
        dialog.OnPositiveConfirm -= OnQuitConfirm;
        _activeSubmenu._canControl = true;
    }

    // Test Loading Screen
    private void OnLoadingScreenTest(PlayerCurser cursor, int buttonId)
    {
        LoadingScreen.GetInstance().DisplayLoading();
        _loadingTime = 5.0f;

    }
}
