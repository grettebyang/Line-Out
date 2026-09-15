using Godot;
using System;
using System.Threading.Tasks;

public partial class GameManager : Node {
    public const string MENU_LEVEL_PATH = "res://Production/Components/UI/MainMenu/Scenes/MainMenu.tscn";

    [Export] public GameDebugTool DebugTool;
    [Export] public SaveManager _saveManager;
    [Export] public TempScoreboard TempScoreboard;

    private String _currentLevelPath = "";
    private Node _currentScene;
    private LevelManager _currentLevel;

    public string CurrentLevelPath { get => _currentLevelPath; }
    public Node CurrentScene { get => _currentScene; }
    public LevelManager CurrentLevel { get => _currentLevel; }
    public string CurrentLevelId;
    public bool DebugMode = false;
    public bool RestrictCameraControls;
    public bool RestrictMenuNavigation;
    [Export] public int[] PlayerCharacters;

    // Signals
    [Signal] public delegate void OnLevelLoadedEventHandler();
    [Signal] public delegate void OnBeforeLevelDeleteEventHandler(LevelManager level);

    public override void _Ready()
    {
        // Dev get current level
        foreach (Node child in GetTree().Root.GetChildren())
        {
            LevelManager levelManager = child as LevelManager;
            if (levelManager != null)
            {
                _currentLevel = levelManager;
                CurrentLevelId = DBLevels._instance.IdOfResource(_currentLevel);
                _currentScene = child;
                break;
            }
        }
    }


    //------------------------------------------
    // Singleton
    //------------------------------------------

    private static GameManager _instance;

    private GameManager() {
        if (_instance != null)
            return;

        _instance = this;
    }

    public static GameManager GetInstance() {
        return _instance;
    }

    //------------------------------------------
    // Levels loading
    //------------------------------------------

    /// <summary>
    /// Load level from list with selected name
    /// </summary>
    /// <param name="levelName">Level name without path and .tscn postfix</param>
    /// <returns></returns>
    public async Task LoadLevel(String path) {
        LoadingScreen.GetInstance().DisplayLoading();
        if (_currentLevel != null){
            EmitSignal(nameof(OnBeforeLevelDelete), _currentLevel);
            _currentLevel.QueueFree();
        }

        /*
        Node levelScene = ResourceLoader.Load<PackedScene>(path).Instantiate();
        if (levelScene == null) {
            // Fail to find
            GD.PrintErr("Contains no level: '" + path + "'");
            return false;
        }
        */
        Error result = ResourceLoader.LoadThreadedRequest(path);
        while (ResourceLoader.LoadThreadedGetStatus(path) == ResourceLoader.ThreadLoadStatus.InProgress)
        {
            await this.ToSignal(this.GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
        }

        if (ResourceLoader.LoadThreadedGetStatus(path) == ResourceLoader.ThreadLoadStatus.Loaded)
        {
            Node levelScene = ResourceLoader.Load<PackedScene>(path).Instantiate();
            _currentScene = levelScene;
            _currentLevel = levelScene as LevelManager;
            CurrentLevelId = DBLevels._instance.IdOfResource(_currentLevel);
            _currentLevelPath = path;

            GetTree().Root.AddChild(levelScene);

            EmitSignal(nameof(OnLevelLoaded));
        }

        LoadingScreen.GetInstance().HideLoading();
    }

    public void ReloadLevel() {
        //InputSystem.GetInstance().DeactivateAllControllers();
        LoadingScreen.GetInstance().DisplayLoading();

        // Fix path with editor running scene
        if (_currentLevelPath == "") {
            _currentLevelPath = GetTree().CurrentScene.SceneFilePath;
        }

        // Proper reset
        if (_currentLevel != null){
            EmitSignal(nameof(OnBeforeLevelDelete), _currentLevel);
            _currentLevel.QueueFree();
        }

        Node levelScene = ResourceLoader.Load<PackedScene>(_currentLevelPath).Instantiate();
        _currentScene = levelScene;
        _currentLevel = levelScene as LevelManager;

        EmitSignal(nameof(OnLevelLoaded));
        GetTree().Root.AddChild(levelScene);
        LoadingScreen.GetInstance().HideLoading();
    }

    public void LoadMenu(bool toLevelSelection = false) {
        LoadLevel(MENU_LEVEL_PATH);

        if (toLevelSelection) {
            MainMenu menu = _currentScene as MainMenu;
            if(menu != null)
                menu.OnLevelsPressed(null, 0);
        }
    }

    public void ToggleDebugMode() {
        DebugMode = !DebugMode;
    }
    
    public void SetPlayerCharacter(int playerId, int characterId)
    {
        PlayerCharacters[playerId] = characterId;
    }

    public int GetPlayerCharacter(int playerId)
    {
        return PlayerCharacters[playerId];
    }

    public int GetPlayerOfCharacter(int characterId)
    {
        for(int i = 0; i < PlayerCharacters.Length; i++)
        {
            if(PlayerCharacters[i] == characterId)
            {
                return i;
            }
        }
        return -1;
    }

    public override void _Input(InputEvent @event) {
        if (Input.IsActionJustPressed("ToggleDebugMode")) {
            ToggleDebugMode();
        }
    }
}
