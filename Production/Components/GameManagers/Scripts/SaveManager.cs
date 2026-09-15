using Godot;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

public partial class SaveManager : Node
{
    string _gameProgressPath = "/game_progress.save";
    const string SPLIT = "_";

    // Game progress data
    JSONLevelsProgress _gameProgress;

    public override void _Ready()
    {
        SetupProgressSave();
    }

    void SetupProgressSave()
    {
        GD.Print("User data: " + OS.GetUserDataDir());
        _gameProgressPath = OS.GetUserDataDir() + _gameProgressPath;

        if (Godot.FileAccess.FileExists(_gameProgressPath))
        {
            // Load data
            string gpStr = FileHandler.ReadJsonFile(_gameProgressPath);
            try
            {
                _gameProgress = JsonSerializer.Deserialize<JSONLevelsProgress>(gpStr);
            }
            catch (JsonException e)
            {
                // Remove and create new
                File.Delete(_gameProgressPath);
                CreateNewSaveFile();
            }

            return;
        }

        // New file
        CreateNewSaveFile();
    }

    void CreateNewSaveFile()
    {
        _gameProgress = new JSONLevelsProgress();

        foreach (LevelResource level in DBLevels._instance.levels)
        {
            LevelProgressSave prog = new LevelProgressSave(level.IdName);
            _gameProgress.LevelsProgress.Add(prog);
        }

        Godot.FileAccess file = Godot.FileAccess.Open(_gameProgressPath, Godot.FileAccess.ModeFlags.Write);
        string jStr = JsonSerializer.Serialize<JSONLevelsProgress>(_gameProgress);
        file.StoreString(jStr);
        file.Close();
    }

    /// <summary>
    /// Create new save or update record matching to same level id
    /// </summary>
    /// <param name="save"></param>
    public void SaveLevelProgress(LevelProgressSave save)
    {
        bool saveExists = false;

        // Override existing save
        for (int i = 0; i < _gameProgress.LevelsProgress.Count; i++)
        {
            if (_gameProgress.LevelsProgress[i].Id == save.Id)
            {
                _gameProgress.LevelsProgress[i] = save;
                saveExists = true;
                break;
            }
        }

        // Create new save
        if (!saveExists)
            _gameProgress.LevelsProgress.Add(save);

        // Update file
        Godot.FileAccess file = Godot.FileAccess.Open(_gameProgressPath, Godot.FileAccess.ModeFlags.Write);
        string jStr = JsonSerializer.Serialize<JSONLevelsProgress>(_gameProgress);
        file.StoreString(jStr);
        file.Close();
    }

    public LevelProgressSave FindLevelProgress(string id)
    {
        foreach (LevelProgressSave save in _gameProgress.LevelsProgress)
        {
            if (save.Id == id)
                return save;
        }

        return new LevelProgressSave(id);
    }
}

public class JSONLevelsProgress
{
    public List<LevelProgressSave> LevelsProgress { get; set; }

    public JSONLevelsProgress()
    {
        LevelsProgress = new List<LevelProgressSave>();
    }
}

public class LevelProgressSave
{
    public string Id { get; set; }
    public float BestTime { get; set; } // in secs

    public LevelProgressSave(string id)
    {
        Id = id;
        BestTime = -1;
    }

     [JsonConstructor]
    public LevelProgressSave(string id, float bestTime)
    {
        Id = id;
        BestTime = bestTime;
    }
}