using Godot;
using System;
using System.IO;
using System.Text.Json;
using Godot.Collections;
using System.Security.Cryptography.X509Certificates;

public partial class SettingsSubmenu : SubmenuBase
{
    public const string MESSAGE_SAVE = "Save changes?";

    [Export] Array<SubmenuBase> _categories;
    [Export] OptionsHeader _header;

    string _settingsPath = "/game_settings.save";

    private JSONGameSettings _jsonSettings;
    public JSONGameSettings JSONSettings
    { 
        get { return _jsonSettings; }
        set { _jsonSettings = value; }
    }

    private SubmenuBase _activeCategory;
    public bool _anyChanges = false;

    public delegate void OnSettingsLoadSave(JSONGameSettings jsonSettings);
    public OnSettingsLoadSave _onSettingsLoaded;
    public OnSettingsLoadSave _onSettingsSaved;
    public EventVersion _onSettingsClosed;

    public override void _Ready()
    {
        base._Ready();

        // UI
        _header._onOptionChange += OnCategoryChange; 

        _settingsPath = OS.GetUserDataDir() + _settingsPath;

        // Load existing data
        if (Godot.FileAccess.FileExists(_settingsPath))
        {
            string str = FileHandler.ReadJsonFile(_settingsPath);
            try
            {
                _jsonSettings = JsonSerializer.Deserialize<JSONGameSettings>(str);
                _onSettingsLoaded?.Invoke(_jsonSettings);
            }
            catch (JsonException e)
            {
                // Remove and create new
                File.Delete(_settingsPath);
                CreateNewSettingsFile();
            }
            return;
        }

        // Create new file
        CreateNewSettingsFile();
    }

    public override void Open()
    {
        base.Open();

         // Setup UI
        foreach(SubmenuBase category in _categories)
        {
            category.ProcessMode = ProcessModeEnum.Disabled;
            category.Visible = false;
        }
        if(_activeCategory != null)
        {
            OnCategoryChange(0);
        }
        else
        {
            _activeCategory = _categories[0];
            _activeCategory.Visible = true;
            _activeCategory.ProcessMode = ProcessModeEnum.Inherit;
        }
        _header.OnFocusEnter(0);
    }

    public override void Close()
    {
        base.Close();
        _onSettingsClosed?.Invoke();
    }

    public override bool Back()
    {
        if(!_anyChanges)
            return true;
        
        ConfirmDialogBase dialog = ConfirmDialogBase.GetInstance();
        dialog.Show(true, EConfirmDialogStyle.YES_NO, MESSAGE_SAVE);
        dialog.OnClosed += OnSaveDialogClosed;
        dialog.OnPositiveConfirm += OnSaveConfirm;
        dialog.OnNegativeConfirm += OnSaveDecline;
        _canControl = false;

        return false;
    }

    public void OnSaveDialogClosed(ConfirmDialogBase dialog)
    {
        _goBackToPreviousMenu?.Invoke(!_anyChanges);
        dialog.OnClosed -= OnSaveDialogClosed;
        dialog.OnPositiveConfirm -= OnSaveConfirm;
        dialog.OnNegativeConfirm -= OnSaveDecline;
        _canControl = true;
    }

    public void OnSaveConfirm(ConfirmDialogBase dialog)
    {
        // Save settings
        _onSettingsSaved?.Invoke(_jsonSettings);
        Godot.FileAccess file = Godot.FileAccess.Open(_settingsPath, Godot.FileAccess.ModeFlags.Write);
        string jStr = JsonSerializer.Serialize<JSONGameSettings>(_jsonSettings);
        file.StoreString(jStr);
        file.Close();

        _anyChanges = false;
    }

    public void OnSaveDecline(ConfirmDialogBase dialog)
    {
        // Revert settings, then close settings menu
        _onSettingsLoaded?.Invoke(_jsonSettings);
        
        _anyChanges = false;
    }

    void CreateNewSettingsFile()
    {
        _jsonSettings = new JSONGameSettings();

        Godot.FileAccess file = Godot.FileAccess.Open(_settingsPath, Godot.FileAccess.ModeFlags.Write);
        Error er = Godot.FileAccess.GetOpenError();
        GD.Print("ER: " + er.ToString());

        string jStr = JsonSerializer.Serialize<JSONGameSettings>(_jsonSettings);
        file.StoreString(jStr);
        file.Close();
    }

    private void OnCategoryChange(int category)
    {
        _activeCategory.Visible = false;
        _activeCategory.ProcessMode = ProcessModeEnum.Disabled;
        _activeCategory = _categories[category];
        _activeCategory.Visible = true;
        _activeCategory.ProcessMode = ProcessModeEnum.Inherit;

        for(int i = 0; i < _mainButtons.Count; i++)
        {
            if(category != i)
            {
                _mainButtons[i].ResetSelected();
            }
        }
    }
}
