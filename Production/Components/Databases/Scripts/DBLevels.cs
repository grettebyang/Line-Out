using Godot;
using System;
using Godot.Collections;
using System.Linq;

public partial class DBLevels : Node
{
    [Export] public LevelResource[] levels;

    public static DBLevels _instance;

    private DBLevels()
    {
        if (_instance != null)
            return;

        _instance = this;
    }

    public string IdOfResource(LevelManager levelManager)
    {
        if (levelManager == null)
            return "UnregistredLevel";

        foreach (LevelResource level in levels)
        {
            if (level._level == null)
                continue;

            if (levelManager.SceneFilePath == level._level.ResourcePath)
            {
                return level.IdName;
            }
        }

        return "UnregistredLevel";
    }

    public string GetNextLevelPath(LevelManager levelManager)
    {
        if (levelManager == null)
            return "UnregistredLevel";

        for(int i = 0; i < levels.Length - 1; i++)
        {
            LevelResource level = levels[i];
            if (level._level == null)
                continue;

            if (levelManager.SceneFilePath == level._level.ResourcePath)
            {
                return levels[i + 1]._level.ResourcePath;
            }
        }

        return "UnregistredLevel";
    }

    public LevelResource ResourceById(string id)
    {
        foreach (LevelResource level in levels)
        {
            if (level.IdName == id)
                return level;
        }

        return null;
    }
}