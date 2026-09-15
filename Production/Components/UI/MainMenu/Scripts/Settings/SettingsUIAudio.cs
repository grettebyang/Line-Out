using Godot;
using System;

public partial class SettingsUIAudio : SubmenuBase
{
    [Export] private SettingsSubmenu _settingsMenu;
    [Export] private string _masterName = "Master";
    [Export] private string _musicName = "Music";
    [Export] private string _sfxName = "SFX";

    private float _masterVolume;
    private float _musicVolume;
    private float _sfxVolume;

    [ExportCategory("UI")]
    [Export] private SliderButton _masterVolumeSlider;
    [Export] private SliderButton _musicVolumeSlider;
    [Export] private SliderButton _sfxVolumeSlider;

    public override void _Ready()
    {
        base._Ready();
        _settingsMenu._onSettingsLoaded += OnSettingsLoaded;
        _settingsMenu._onSettingsSaved += OnSettingsSaved;
    }

    // Loaded
    private void OnSettingsLoaded(JSONGameSettings settings)
    {
        _masterVolume = _settingsMenu.JSONSettings.Audio.MasterVolume;
        _masterVolumeSlider.SetValue(_masterVolume);
        int masterID = AudioServer.GetBusIndex(_masterName);
        AudioServer.SetBusVolumeLinear(masterID, _masterVolume);
        
        _musicVolume = _settingsMenu.JSONSettings.Audio.MusicVolume;
        _musicVolumeSlider.SetValue(_musicVolume);
        int musicID = AudioServer.GetBusIndex(_musicName);
        AudioServer.SetBusVolumeLinear(musicID, _musicVolume);
        
        _sfxVolume = _settingsMenu.JSONSettings.Audio.SFXVolume;
        _sfxVolumeSlider.SetValue(_sfxVolume);
        int sfxID = AudioServer.GetBusIndex(_sfxName);
        AudioServer.SetBusVolumeLinear(sfxID, _sfxVolume);
    }

    // Saved
    public void OnSettingsSaved(JSONGameSettings settings)
    {
        settings.Audio.MasterVolume = _masterVolume;
        settings.Audio.MusicVolume = _musicVolume;
        settings.Audio.SFXVolume = _sfxVolume;
    }

    // UI change signals
    private void OnMasterVolumeChange(float value)
    {
        int masterID = AudioServer.GetBusIndex(_masterName);
        AudioServer.SetBusVolumeLinear(masterID, value);
        _masterVolume = value;

        _settingsMenu._anyChanges = true;
    }
    private void OnMusicVolumeChange(float value)
    {
        int musicID = AudioServer.GetBusIndex(_musicName);
        AudioServer.SetBusVolumeLinear(musicID, value);
        _musicVolume = value;

        _settingsMenu._anyChanges = true;
    }
    private void OnSFXVolumeChange(float value)
    {
        int sfxID = AudioServer.GetBusIndex(_sfxName);
        AudioServer.SetBusVolumeLinear(sfxID, value);
        _sfxVolume = value;

        _settingsMenu._anyChanges = true;
    }
}
