using Godot;
using System;

public partial class FileHandler
{
    public static string ReadJsonFile(string path)
    {
        Godot.FileAccess file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
        string jString = file.GetAsText();
        file.Close();

        return jString;
    }
}
