using Godot;
using System;

public class UIConstants
{
    // Use when handling hold input to have standardized hold time
    public static float HOLD_TIMER = 1.0f;
    public static float HOLD_TIMER_LONG = 3.0f;

    public const String ACTION_CONFIRM = "MenuConfirm";
    public const String ACTION_CANCEL = "MenuCancel";
    public const String ACTION_UP = "up";
    public const String ACTION_DOWN = "down";
    public const String ACTION_SKIP = "skip";

    public static Color COLOR_RHYTM_BLUE = new Color(0.24f, 0.37f, 0.71f);
    public static Color COLOR_RHYTM_WHITE = new Color(0, 0, 0);

    // Player colorcoding
    public static Color[] PLAYER_COLORS = {
        new Color(0.35f, 0.33f, 0.79f), // Blue
        new Color(0.86f, 0.46f, 0.46f), // Red
        new Color(0.75f, 0.6f, 0.19f), // Yellow
        new Color(0.45f, 0.62f, 0.42f), // Green
    };

    public static PackedScene[] CHARACTER_MESHES = {
        ResourceLoader.Load<PackedScene>("res://assets/3D Assets - Production/Prefabs/dog_temp_mesh_darkgray.tscn"),
        ResourceLoader.Load<PackedScene>("res://assets/3D Assets - Production/Prefabs/dog_temp_mesh_lightgray.tscn"),
        ResourceLoader.Load<PackedScene>("res://assets/3D Assets - Production/Prefabs/dog_temp_mesh_brown.tscn"),
        ResourceLoader.Load<PackedScene>("res://assets/3D Assets - Production/Prefabs/dog_temp_mesh_beige.tscn")
    };

    public static StandardMaterial3D[] CHARACTER_SKINS = {
        ResourceLoader.Load<StandardMaterial3D>("res://assets/3D Assets - Production/Imports/Dog_TEMP/Materials/M_DogTEMP_Beige.tres"),
        ResourceLoader.Load<StandardMaterial3D>("res://assets/3D Assets - Production/Imports/Dog_TEMP/Materials/M_DogTEMP_LightGrey.tres"),
        ResourceLoader.Load<StandardMaterial3D>("res://assets/3D Assets - Production/Imports/Dog_TEMP/Materials/M_DogTEMP_Chocolate.tres"),
        ResourceLoader.Load<StandardMaterial3D>("res://assets/3D Assets - Production/Imports/Dog_TEMP/Materials/M_DogTEMP_Beige.tres")
    };

    public static PackedScene[] CHARACTER_MODELS = {
        ResourceLoader.Load<PackedScene>("res://assets/3D Assets - Production/Prefabs/dogTEMP_DarkGrey.tscn"),
        ResourceLoader.Load<PackedScene>("res://assets/3D Assets - Production/Prefabs/dogTEMP_LightGrey.tscn"),
        ResourceLoader.Load<PackedScene>("res://assets/3D Assets - Production/Prefabs/dogTEMP_Brown.tscn"),
        ResourceLoader.Load<PackedScene>("res://assets/3D Assets - Production/Prefabs/dogTEMP_Beige.tscn")
    };

    public static string[] PLAYER_NAMES = {
        "Player 1",
        "Player 2",
        "Player 3",
        "Player 4",
    };

    public static string[] CHARACTER_NAMES = {
        "Patches",
        "Lois",
        "Neli",
        "Igor",
    };

    public static CompressedTexture2D[] CHARACTER_PORTRAITS = {
        ResourceLoader.Load<CompressedTexture2D>("res://Production/Components/UI/DogPortraits/DarkGrey.png"),
        ResourceLoader.Load<CompressedTexture2D>("res://Production/Components/UI/DogPortraits/LightGrey.png"),
        ResourceLoader.Load<CompressedTexture2D>("res://Production/Components/UI/DogPortraits/Brown.png"),
        ResourceLoader.Load<CompressedTexture2D>("res://Production/Components/UI/DogPortraits/Beige.png"),
    };

    // Rhythm feedback colorcoding
    public static Color[] FEEDBACK_COLORS = {
        Colors.Beige, // Blue
        Colors.Yellow,
        Colors.Green,
        Colors.Blue,
        Colors.Purple
    };
}
