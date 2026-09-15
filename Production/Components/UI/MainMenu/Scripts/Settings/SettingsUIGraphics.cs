using Godot;
using System;
using System.Collections.Generic;

public partial class SettingsUIGraphics : SubmenuBase
{
    [Export] private SettingsSubmenu _settingsMenu;

    [ExportCategory("UI")]
    [Export] private SettingsLeftRightOption _resolutions;
    [Export] private ToggleButton _windowModeToggle;

    List<(string, Vector2I)> _resolutionOptions = new List<(string, Vector2I)>();
    public int _currentResolutionOption = 0;
    public bool _windowMode;

    public int _previousResolution = 0;

    public override void _Ready()
    {
        base._Ready();

        // Resolutions
        _resolutionOptions.Add(("Auto Detect", DisplayServer.ScreenGetSize()));
        _resolutionOptions.Add(("1920x1080", new Vector2I(1920, 1080)));
        _resolutionOptions.Add(("2560x1440", new Vector2I(2560, 1440)));
        _resolutionOptions.Add(("3840x2160", new Vector2I(3840, 2160)));
        _resolutionOptions.Add(("1680x1050", new Vector2I(1680, 1050)));
        _resolutionOptions.Add(("1920x1200", new Vector2I(1920, 1200)));
        _resolutionOptions.Add(("1024x768", new Vector2I(1024, 768)));
        _resolutionOptions.Add(("1280x960", new Vector2I(1280, 960)));
        _resolutionOptions.Add(("2560x1080", new Vector2I(2560, 1080)));
        _resolutionOptions.Add(("3440x1440", new Vector2I(3440, 1440)));
        _resolutionOptions.Add(("1280x720", new Vector2I(1280, 720)));
        _resolutionOptions.Add(("1600x900", new Vector2I(1600, 900)));
        _resolutionOptions.Add(("1366x768", new Vector2I(1366, 768)));

        // foreach (string str in _resolutionOptions)
        // {
        //     _resolutions.AddItem(str);
        // }

        // On load
        _settingsMenu._onSettingsLoaded += OnSettingsLoaded;

        // On save
        _settingsMenu._onSettingsSaved += OnSettingsSaved;
    }

    // Loaded
    private void OnSettingsLoaded(JSONGameSettings settings)
    {
        // Window Mode
        _windowMode = settings.Graphics.WindowMode;
        _windowModeToggle.SetToggle(_windowMode);
        if(_windowMode)
        {
            GetWindow().Mode = Window.ModeEnum.Windowed;            
        }
        else
        {
            GetWindow().Mode = Window.ModeEnum.ExclusiveFullscreen;
        }

        // Resolution
        _currentResolutionOption = settings.Graphics.Resolution;
        _resolutions.Select(_resolutionOptions[_currentResolutionOption].Item1);
        if(_currentResolutionOption == 0) // auto detect
        {
            GetWindow().Size = DisplayServer.ScreenGetSize();
        }
        else
        {
            GetWindow().Size = _resolutionOptions[_currentResolutionOption].Item2;
        }
    }

    private void OnSettingsSaved(JSONGameSettings settings)
    {
        settings.Graphics.Resolution = _currentResolutionOption;
        settings.Graphics.WindowMode = _windowMode;
    }

    private void OnResultionSelected(Label optionLabel, int changeDirection)
    {
        _currentResolutionOption = (_currentResolutionOption + _resolutionOptions.Count + changeDirection) % _resolutionOptions.Count;
        optionLabel.Text = _resolutionOptions[_currentResolutionOption].Item1;
    }

    private void ApplyResolution(PlayerCurser cursor, int buttonId)
    {
        if(_currentResolutionOption == 0) // auto detect
        {
            GetWindow().Size = DisplayServer.ScreenGetSize();
        }
        else
        {
            GetWindow().Size = _resolutionOptions[_currentResolutionOption].Item2;
        }
        _settingsMenu._anyChanges = true;
    }

    private void RevertResolution(PlayerCurser cursor, int buttonId)
    {
        _currentResolutionOption = _settingsMenu.JSONSettings.Graphics.Resolution;
        _resolutions.Select(_resolutionOptions[_currentResolutionOption].Item1);
        GetWindow().Size = _resolutionOptions[_currentResolutionOption].Item2;
    }

    private void ToggleWindowMode(bool toggled)
    {
        if(toggled)
        {
            GetWindow().Mode = Window.ModeEnum.Windowed;            
        }
        else
        {
            GetWindow().Mode = Window.ModeEnum.ExclusiveFullscreen;
        }
        _windowMode = toggled;
        _settingsMenu._anyChanges = true;
    }
}
