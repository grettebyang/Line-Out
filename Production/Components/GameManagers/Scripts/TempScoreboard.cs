using Godot;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

public partial class TempScoreboard : Node
{
    private string _scoreboardPath = "/scoreboard.save";
    private TempJsonScoreboard _scoreboard;

    public TempJsonScoreboard Scoreboard { get { return _scoreboard; }}    

    public override void _Ready()
    {
        Setup();
    }

    void Setup()
    {
        _scoreboardPath = OS.GetUserDataDir() + _scoreboardPath;
        GD.Print("_scoreboardPath: " + _scoreboardPath);

        if (Godot.FileAccess.FileExists(_scoreboardPath))
        {
            // Load data
            string gpStr = FileHandler.ReadJsonFile(_scoreboardPath);
            try
            {
                _scoreboard = JsonSerializer.Deserialize<TempJsonScoreboard>(gpStr);
            }
            catch (JsonException e)
            {
                // Remove and create new
                File.Delete(_scoreboardPath);
                CreateNewSaveFile();
            }

            return;
        }

        // New file
        CreateNewSaveFile();
    }

    void CreateNewSaveFile()
    {
        _scoreboard = new TempJsonScoreboard();        
        _scoreboard.Records = new List<TempScoreboardRecord>();

        Godot.FileAccess file = Godot.FileAccess.Open(_scoreboardPath, Godot.FileAccess.ModeFlags.Write);
        string jStr = JsonSerializer.Serialize<TempJsonScoreboard>(_scoreboard);
        file.StoreString(jStr);
        file.Close();
    }

    public void SaveRecord(TempScoreboardRecord record)
    {
        bool saved = false;

        // Add record under matching time
        for (int i = 0; i < _scoreboard.Records.Count; i++)
        {
            float time = _scoreboard.Records[i].Time;
             if (record.Time > time)
                continue;

            // Save time here
            _scoreboard.Records.Insert(i, record);
            saved = true;
            break;
        }

        if (_scoreboard.Records.Count == 0)
        {
            // First record in leaderboard
            _scoreboard.Records.Insert(0, record);
        }
        else if (!saved)
        {
            // Last time
            _scoreboard.Records.Add(record);
        }

        // Update file
        Godot.FileAccess file = Godot.FileAccess.Open(_scoreboardPath, Godot.FileAccess.ModeFlags.Write);
        string jStr = JsonSerializer.Serialize<TempJsonScoreboard>(_scoreboard);
        file.StoreString(jStr);
        file.Close();
    }

    /// <summary>
    /// Returns in what position would time be placed
    /// </summary>
    /// <returns></returns>
    public int PositionOfTime(float time)
    {
        for (int i = 0; i < _scoreboard.Records.Count; i++)
        {
            float t = _scoreboard.Records[i].Time;
            if (time > t)
                continue;

            return i;
        }

        return _scoreboard.Records.Count;
    }
}

/// <summary>
/// Temporary scoreboard used for showcase
/// Makes no difference between various levels as showcase use only single level!
/// </summary>
public class TempJsonScoreboard
{
    public List<TempScoreboardRecord> Records { get; set; }

    public TempJsonScoreboard()
    {
        Records = new List<TempScoreboardRecord>();
    }
}

public class TempScoreboardRecord
{
    public float Time { get; set; }
    public string TeamName { get; set; }
    public int TeamSize { get; set; }

    public TempScoreboardRecord(float time, string teamName, int teamSize)
    {
        Time = time;
        TeamName = teamName;
        TeamSize = teamSize;
    }
}