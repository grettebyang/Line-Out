using Godot;
using System;
using System.Collections.Generic;

public partial class SettingsUIGameplay : SubmenuBase
{
    [Export] private SettingsSubmenu _settingsMenu;

    [ExportCategory("UI")]
    [Export] private ToggleButton _restrictCameraControlsToggle;
    [Export] private ToggleButton _restrictMenuNavToggle;
    [Export] private ToggleButton _enableKeyboardToggle;
    [Export] private SettingsLeftRightOption _controlSchemes;

    private List<string> _controlSchemeOptions = new List<string>();
    private bool _restrictCameraControls;
    private bool _restrictMenuNav;
    private bool _enableKeyboard;
    private int _controlScheme;

    public override void _Ready()
    {
        base._Ready();

        // Add control scheme options
        _controlSchemeOptions.Add("Xbox");
        _controlSchemeOptions.Add("PlayStation");

        // On load
        _settingsMenu._onSettingsLoaded += OnSettingsLoaded;

        // On save
        _settingsMenu._onSettingsSaved += OnSettingsSaved;
    }

    // Loaded
    private void OnSettingsLoaded(JSONGameSettings settings)
    {
        // Restrict Camera
        _restrictCameraControls = settings.Gameplay.RestrictCameraControls;
        _restrictCameraControlsToggle.SetToggle(_restrictCameraControls);
        GameManager.GetInstance().RestrictCameraControls = _restrictCameraControls;           
        
        // Restrict Manu Nav
        _restrictMenuNav = settings.Gameplay.RestrictMenuNavigation;
        _restrictMenuNavToggle.SetToggle(_restrictMenuNav);
        PlayerCurserContainer.GetInstance().SetMenuNavRestriction(_restrictMenuNav);
        
        // Enable Keyboard
        // _enableKeyboard = settings.Gameplay.EnableKeyboard;
        // _enableKeyboardToggle.SetToggle(_enableKeyboard);
        // if(_enableKeyboard)
        // {
                     
        // }
        // else
        // {
            
        // }
        
        // Control Scheme
        _controlScheme = settings.Gameplay.ControlScheme;
        _controlSchemes.Select(_controlSchemeOptions[_controlScheme]);
    }

    // Saved
    private void OnSettingsSaved(JSONGameSettings settings)
    {
        settings.Gameplay.RestrictCameraControls = _restrictCameraControls;
        settings.Gameplay.RestrictMenuNavigation = _restrictMenuNav;
        settings.Gameplay.EnableKeyboard = _enableKeyboard;
        settings.Gameplay.ControlScheme = _controlScheme;
    }

    private void ToggleRestrictCameraControls(bool toggled)
    {     
        _restrictCameraControls = toggled;
        GameManager.GetInstance().RestrictCameraControls = _restrictCameraControls;    
        _settingsMenu._anyChanges = true;
    }

    private void ToggleRestrictMenuNavigation(bool toggled)
    {
        _restrictMenuNav = toggled;
        PlayerCurserContainer.GetInstance().SetMenuNavRestriction(_restrictMenuNav);
        _settingsMenu._anyChanges = true;
    }

    private void ToggleEnableKeyboard(bool toggled)
    {
        if(toggled)
        {
                       
        }
        else
        {
            
        }
        _enableKeyboard = toggled;
        _settingsMenu._anyChanges = true;
    }

    private void OnControlSchemeSelected(Label optionLabel, int changeDirection)
    {
        _controlScheme = (_controlScheme + _controlSchemeOptions.Count + changeDirection) % _controlSchemeOptions.Count;
        optionLabel.Text = _controlSchemeOptions[_controlScheme];
    }
}
