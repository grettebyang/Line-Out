using Godot;
using System;
using System.Text.Json.Serialization;

/*
This file defines game settings json data structure
*/

public class JSONGameSettings
{
    public JSONGameSettingsGameplay Gameplay { get; set; }
    public JSONGameSettingsGraphics Graphics { get; set; }
    public JSONGameSettingsAudio Audio { get; set; }
    public JSONGameSettingsRhythm Rhythm { get; set; }

    [JsonConstructor]
    public JSONGameSettings()
    {
        Gameplay = new JSONGameSettingsGameplay();
        Graphics = new JSONGameSettingsGraphics();
        Audio = new JSONGameSettingsAudio();
        Rhythm = new JSONGameSettingsRhythm();
    }
}

public class JSONGameSettingsRhythm
{
    public float AudioCalibration { get; set; }
    public float VideoCalibration { get; set; }
    
}

public class JSONGameSettingsGameplay
{
    public bool RestrictCameraControls { get; set; }
    public bool RestrictMenuNavigation { get; set; }
    public bool EnableKeyboard { get; set; }
    public int ControlScheme { get; set; }
    
}

public class JSONGameSettingsGraphics
{
    public int Resolution { get; set; }
    public bool WindowMode { get; set; }
}

public class JSONGameSettingsAudio
{
    public float MasterVolume { get; set; }
    public float MusicVolume { get; set; }
    public float SFXVolume { get; set; }

    [JsonConstructor]
    public JSONGameSettingsAudio()
    {
        // Default values
        MasterVolume = 1;
        MusicVolume = 1;
        SFXVolume = 1;
    }
}
