using Godot;
using System;

public delegate void Notify();
public delegate void NotifyFail(string reason);
public delegate void NotifyPosition(Vector3 pos);
public partial class LevelManager : Node
{
    const string FAIL_REASON_TIMEOUT = "Failed to deliver in time!";
    const string FAIL_REASON_NPCWIN = "Lost to competitor!";

    [ExportGroup("Level properties")]
    [Export] protected int _lapCount;
    [Export] protected float _levelTime;
    [Export] protected float _respawnTime = 2;
    [Export] protected bool ShowSessionTime = false;

    [ExportGroup("References")]
    [Export] public DogSled _sled;
    [Export] public Area3D _endGoal;
    [Export] public Node3D Collectables;
    [Export] public Node3D Meat;
    [Export] public Node3D TimeBoosts;
    [Export] public PackedScene DamageEffectScene;
    [Export] public Tracknames Soundtrack;

    [ExportGroup("Systems")]
    [Export] public CheckpointManager _checkpointManager;
    [Export] public RhythmManager _rythmManager;


    private BasicTimeManager _timeManager;

    public bool _levelWon = false;

    // State tracking
    protected bool _running = false;
    public float _sessionTime;
    protected float _levelTimeCounter;
    protected float _respawnTimeCounter;
    protected float _lastMeasuredTime = 0;

    public float LevelTime { get => _levelTime; }
    public float LevelTimeCounter { get => _levelTimeCounter; }

    // References
    protected InputSystem _inputSystem;

    public Notify OnStart;
    public Notify OnFinished;
    public NotifyFail OnFail;
    public Notify OnRespawn;
    public Notify OnPauseToggle;
    public NotifyPosition OnBark;

    //------------------------------------------
    // Override methods
    //------------------------------------------
    public override void _Ready()
    {
        SetupLevel();

        if (_levelTime > 0)
            _levelTimeCounter = _levelTime;

        _timeManager = BasicTimeManager.GetInstance();
        _timeManager.GameState();

        if (_endGoal != null)
            _endGoal.AreaEntered += OnEndGoalEntered;

        if (_checkpointManager != null)
            _checkpointManager.OnEntered += OnCheckpointEntered;
    }

    public override void _Process(double delta)
    {
        // Track time
        if (_running)
            _sessionTime += (float)delta * _timeManager.GameSpeed;

        if (_running && _levelTimeCounter > 0)
        {
            _levelTimeCounter -= (float)delta * _timeManager.GameSpeed;

            // Run out of time
            if (_levelTimeCounter <= 0)
                LevelFailed(FAIL_REASON_TIMEOUT);
        }

        // Toggle pause
        if (Input.IsActionJustPressed(PauseMenu.ACTION_PAUSE_MENU))
        {
            if (!_levelWon)
                OnPauseToggle?.Invoke();
        }

        // Respawn
        if (_respawnTimeCounter > 0)
        {
            _respawnTimeCounter -= (float)delta;
            if (_respawnTimeCounter <= 0)
                OnRespawnTimerTimeout();
        }

        // Test restart
        if (OS.IsDebugBuild())
        {
            /*
            if (Input.IsActionJustPressed("testRestart"))
            {
                GameManager.GetInstance().ReloadLevel();
            }
            */
        }
    }

    private void RespawnSled()
    {
        _inputSystem.RemoveActiveInputMode(EInputSystemMode.PLAYER_MOVEMENT);
        _respawnTimeCounter = _respawnTime;
        //GD.Print("RespawnSled");
    }

    private void OnRespawnTimerTimeout()
    {
        _inputSystem.AddActiveInputMode(EInputSystemMode.PLAYER_MOVEMENT);
        _sled.PackageCarrier.Health.Revive();
        OnRespawn?.Invoke();
    }

    //------------------------------------------
    // Custom
    //------------------------------------------

    private void OnCheckpointEntered(Checkpoint checkpoint)
    {
        // Show time
        float checkpointTime = _sessionTime - _lastMeasuredTime;
        _lastMeasuredTime = _sessionTime;

        string time = TimeFormated(checkpointTime);
        HUDMessage.DisplayMessage("Checkpoint reached - Travel time: " + time);
    }

    private void OnEndGoalEntered(Node3D body)
    {
        if (!_running)
            return;

        // NPC
        RacingNPCSled npcSled = body as RacingNPCSled;
        if (npcSled != null)
        {
            LevelFailed(FAIL_REASON_NPCWIN);
            return;
        }

        // Players
        DogController dog = body as DogController;
        if (dog != null)
            LevelFinished();
    }

    private void OnAvalancheConsume()
    {
        RespawnSled();
    }

    private void StartLevel()
    {
        _running = true;
        OnStart?.Invoke();
    }

    protected void LevelFinished()
    {
        _running = false;
        _levelWon = true;
        OnFinished?.Invoke();

        // Rating
        GameManager gm = GameManager.GetInstance();
        LevelResource level = DBLevels._instance.ResourceById(gm.CurrentLevelId);
        int rating = 0;

        foreach (float timeRating in level.TimesToRating)
        {
            if (_sessionTime <= timeRating)
            {
                rating++;
            }
        }

        //GD.Print("Rating: " + rating + " stars");

        // Save best time
        LevelProgressSave save = gm._saveManager.FindLevelProgress(gm.CurrentLevelId);
    
        if (save == null || _sessionTime < save.BestTime || save.BestTime == -1)
        {
            gm._saveManager.SaveLevelProgress(save);
            save.BestTime = _sessionTime;
        }

        // Trigger other components ...
        _inputSystem.ActivateSingleInputMode(EInputSystemMode.MENU);
    }

    protected void LevelFailed(string reason = "")
    {
        _running = false;
        _levelWon = false;
        OnFail?.Invoke(reason);

        _inputSystem.ActivateSingleInputMode(EInputSystemMode.MENU);
    }

    protected void SetupLevel()
    {
        _running = false;
        _levelWon = false;
        _lastMeasuredTime = 0;

        _inputSystem = InputSystem.GetInstance();
        _rythmManager = RhythmManager.GetInstance();

        // Activator setup - if not set from menu

        GD.Print("Activator setup: " + _inputSystem.GetOnlyActiveControllers().Count);
        if (_inputSystem.GetOnlyActiveControllers().Count == 0)
        {
            _inputSystem.AllowControllerActivation(true);
        }

        RhythmManager.GetInstance().Hike += StartLevel;

        // Setup sled
        _sled.PackageCarrier.Health.OnDeath += RespawnSled;

        GetLevelTimeBoosts();
    }

    /// <summary>
    /// Return remaining level time as formated time string - 00:00.00
    /// </summary>
    /// <returns></returns>
    public string LevelTimeCounterFormated()
    {
        float sesTime = LevelTimeCounter;
        string min = Mathf.Floor(sesTime / 60).ToString();
        string sec = Mathf.Floor(sesTime % 60).ToString();
        string mil = ((int)(((sesTime - Mathf.Floor(sesTime)) % 1) * 100)).ToString();
        string milHund = MathF.Floor(100f * (sesTime % 1.0f) % 10).ToString();
        string milTen = MathF.Floor(10f * (sesTime % 1.0f)).ToString();
        string secOne = MathF.Floor(sesTime % 10).ToString();
        string secTen = (MathF.Floor((sesTime % 60) / 10)).ToString();
        string minOne = (MathF.Floor(sesTime / 60) % 10).ToString();
        string minTen = MathF.Floor((MathF.Floor(sesTime / 60) / 10)).ToString();

        return minTen + minOne + ":" + secTen + secOne + "." + milTen + milHund;
    }

    public static string TimeFormated(float sesTime)
    {
        string min = Mathf.Floor(sesTime / 60).ToString();
        string sec = Mathf.Floor(sesTime % 60).ToString();
        string mil = ((int)(((sesTime - Mathf.Floor(sesTime)) % 1) * 100)).ToString();
        string milHund = MathF.Floor(100f * (sesTime % 1.0f) % 10).ToString();
        string milTen = MathF.Floor(10f * (sesTime % 1.0f)).ToString();
        string secOne = MathF.Floor(sesTime % 10).ToString();
        string secTen = (MathF.Floor((sesTime % 60) / 10)).ToString();
        string minOne = (MathF.Floor(sesTime / 60) % 10).ToString();
        string minTen = MathF.Floor((MathF.Floor(sesTime / 60) / 10)).ToString();

        return minTen + minOne + ":" + secTen + secOne + "." + milTen + milHund;
    }

    public void OnDamageReceived()
    {
        HurtEffect hurtEffect = DamageEffectScene.Instantiate<HurtEffect>();
        AddChild(hurtEffect);
        _sled._camera.CameraShakeRequest = true;
    }

    public void AddToTimer()
    {
        _levelTimeCounter = Math.Min(_levelTimeCounter + 10, _levelTime);
    }

    public void DogBark(Vector3 pos)
    {
        OnBark?.Invoke(pos);
    }

    public void GetLevelTimeBoosts()
    {
        if (GameManager.GetInstance().CurrentLevel.TimeBoosts is null)
        {
            return;
        }

        var collectables = TimeBoosts.GetChildren();

        foreach (Node3D collectable in collectables)
        {
            (collectable as Collectable).OnFikaEntered += AddToTimer;
        }
    }

    // Public API
    public bool IsSessionRunning()
    {
        return _running;
    }
}
 