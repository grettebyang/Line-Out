using Godot;
using System;
using System.Collections.Generic;

public partial class TrackHUD : CanvasLayer
{
    [ExportGroup("UI Elements")]
    [Export] protected Label _lblTimer;
    [Export] protected Control _packageStateHolder;
    [Export] protected TextureProgressBar _packageHpBar;
    [Export] protected Label _packageHpBarValue;
    [Export] protected float _lowHp = 10f;
    [Export] protected float _hpChangeTimeDelay = 0.5f;
    [Export] protected float _lowTime = 10;
    [Export] protected Control _powerupHolders;
    [Export] protected Control _compass;
    [Export] protected Control _directionArrow;
    [Export] protected Control _timerHolder;
    [Export] protected TextureProgressBar _timerProgress;
    [Export] protected TextureProgressBar _timerProgressSecondary;
    [Export] protected Control _bestTimeHolder;
    [Export] protected Label _bestTimeData;

    [ExportGroup("Coloring")]
    [Export] protected Color _colorBlue;
    [Export] protected Color _colorRed;
    [Export] protected Color _colorBrown;
    [Export] protected float _hpRecolorSpeed = 5;

    [ExportGroup("References")]
    [Export] protected AnimationPlayer _HPAnimator;
    [Export] protected AnimationPlayer _TimerAnimator;
    [Export] protected AnimationPlayer _PowerupAnimator;

    [ExportGroup("Icons")]
    [Export] protected Texture2D _iconCoffee;
    [Export] protected Texture2D _iconCupcake;
    [Export] protected Texture2D _iconSugarcane;

    protected LevelManager _levelManager;
    protected DogSled _sled;

    protected RhythmManager _rhythmManager;
    
    protected float _hpChangeTimer;
    protected Color _hpChangeColor;


    protected List<PowerupHolder> _powerupHolderQueue = new List<PowerupHolder>();
    protected String _PowerupHolderScene = "res://Production/Components/UI/HUD/Scenes/powerup_holder.tscn";
    protected Vector2 _CardOffset = new Vector2(0.0f, 160.0f);

    //------------------------------------------
    // Override
    //------------------------------------------

    public override void _Ready()
    {
        GameManager gameManager = GameManager.GetInstance();
        gameManager.OnBeforeLevelDelete += OnBeforeLevelDelete;

        _levelManager = gameManager.CurrentLevel;
        _levelManager.OnRespawn += OnRespawn;
        _sled = _levelManager._sled;
        if (_sled.PackageCarrier != null)
        {
            OnPackageLoadChange();
            _sled.PackageCarrier.OnAddPackage += OnPackageLoadChange;
            _sled.PackageCarrier.OnRemovePackage += OnPackageLoadChange;
        }

        _rhythmManager = RhythmManager.GetInstance();
        _rhythmManager.OnPowerupActivate += OnPowerupActivate;
        _rhythmManager.OnPowerupCollected += OnPowerupCollected;
        _rhythmManager.OnPowerupEnded += OnPowerupEnded;
        _rhythmManager.OnRhythmEventStart += OnRhythmEventStart;

        // Visible
        _compass.Visible = false;
        _packageStateHolder.Visible = _sled.PackageCarrier != null;
        _timerHolder.Visible = false;

        // Timer setup
        if (gameManager.CurrentLevel != null)
            gameManager.CurrentLevel.OnStart += OnGameStart;

        //_timerProgress.MaxValue = _levelManager.LevelTime;
        _timerProgress.MaxValue = 60;
        _timerProgressSecondary.MaxValue = 60;

        // Display best time
        LevelProgressSave save = gameManager._saveManager.FindLevelProgress(gameManager.CurrentLevelId);
        _bestTimeHolder.Visible = (save != null) && (save.BestTime != -1);
        if (save != null)
        {
            _bestTimeData.Text = LevelManager.TimeFormated(save.BestTime);
        }

        // Package HP setup 
        _sled.PackageCarrier.Health.OnChangeHealth += OnPackageHealthChange;
    }

    public override void _Process(double delta){
        // Text update

        // Time
        float sesTime = _levelManager._sessionTime;

        if (sesTime > 0)
            _lblTimer.Text = LevelManager.TimeFormated(sesTime);
        else
            _lblTimer.Text = "00:00.00";

        // Time update
        if (_timerProgress != null)
        {
            if ((int)(sesTime / 60) % 2 == 0)
            {
                _timerProgress.Value = sesTime - (int)(sesTime / 60) * 60;
                _timerProgress.ZIndex = 1;
                _timerProgress.Visible = true;
                _timerProgressSecondary.Visible = sesTime / 60 > 1;
                _timerProgressSecondary.ZIndex = 0;
            }
            else
            {
                _timerProgressSecondary.Value = sesTime - (int)(sesTime / 60) * 60;
                _timerProgressSecondary.Visible = true;
                _timerProgressSecondary.ZIndex = 1;
                _timerProgress.Visible = false;
                _timerProgress.Visible = sesTime / 60 > 1;
                _timerProgress.ZIndex = 0;
            }
        }

        // Package HP
        if (_sled.PackageCarrier != null && _sled.PackageCarrier.Health.MaxHealth > 0)
            PackageHealthUpdate((float)delta);
        else
            _packageStateHolder.Visible = false;

        // Powerups
        for(int i = 0; i < _rhythmManager._powerUpQueue.Count; i++)
        {
            float _remainingPowerupTime = 100f * _rhythmManager._powerUpQueue[i]._dur;
            if(i < _powerupHolderQueue.Count)
                _powerupHolderQueue[i].UpdateTimer(_remainingPowerupTime);
        } 

    }

    //------------------------------------------
    // Custom
    //------------------------------------------

    private void OnBeforeLevelDelete(LevelManager level)
    {
        // Unregister all callbacks
        GameManager.GetInstance().OnBeforeLevelDelete -= OnBeforeLevelDelete;
        GameManager.GetInstance().CurrentLevel.OnStart -= OnGameStart;
        GameManager.GetInstance().CurrentLevel.OnRespawn -= OnRespawn;

        _sled.PackageCarrier.Health.OnChangeHealth -= OnPackageHealthChange;
        _sled.PackageCarrier.OnAddPackage -= OnPackageLoadChange;
        _sled.PackageCarrier.OnRemovePackage -= OnPackageLoadChange;

        _rhythmManager.OnPowerupActivate -= OnPowerupActivate;
        _rhythmManager.OnPowerupCollected -= OnPowerupCollected;
        _rhythmManager.OnPowerupEnded -= OnPowerupEnded;
        _rhythmManager.OnRhythmEventStart -= OnRhythmEventStart;
    }

    private void PackageHealthUpdate(float delta)
    {
        _packageStateHolder.Visible = true;
        _packageHpBar.MaxValue = _sled.PackageCarrier.Health.MaxHealth;
        _packageHpBar.Value = _sled.PackageCarrier.Health.Health;
        _packageHpBarValue.Text = _packageHpBar.Value.ToString();

        // Coloring
        if (_sled.PackageCarrier.Health.Health > _lowHp)
        {
            if (_hpChangeTimer > 0)
            {
                _hpChangeTimer -= delta;
            }
            else
            {
                _hpChangeColor = _hpChangeColor.Lerp(Colors.White, delta * _hpRecolorSpeed);
            }
        }
        else
        {
            _hpChangeColor = _colorRed;
        }

        _packageStateHolder.SelfModulate = _hpChangeColor;

        // Hp value
        /*
        if (_sled.PackageCarrier.Health.Health > _lowHp)
            _packageHpBarValue.AddThemeColorOverride("font_color", _colorBrown);
        else
            _packageHpBarValue.AddThemeColorOverride("font_color", _colorRed);
        */
    }

    private void OnPackageHealthChange(int currentHealth, int change){
        // Dmg 
        if (change < 0){
            _hpChangeTimer = _hpChangeTimeDelay;
            _hpChangeColor = _colorRed;
            _HPAnimator.Play("HPDmg");
        }
        else if (change > 0){
            // Healing
            _HPAnimator.Play("HPHeal");
        }
        
    }

    private void OnPackageLoadChange(){
        _packageHpBar.MaxValue = _sled.PackageCarrier.Health.MaxHealth;
    }

    private void OnGameStart(){
        GameManager.GetInstance().CurrentLevel.OnStart -= OnGameStart;

        // Show timer
        _timerHolder.Visible = true;
        _TimerAnimator.Play("TimeStart");
    }

    private void OnPowerupActivate(float duration, float maxDuration){
        if(_powerupHolderQueue.Count > 0)
            _powerupHolderQueue[_powerupHolderQueue.Count - 1].PowerupActivate(duration, maxDuration);
    }

    private void OnPowerupCollected(){
        Texture2D icon;
        switch (_rhythmManager._gauge._puType){
            case 0: icon = _iconCoffee; break;
            case 1: icon = _iconSugarcane; break;
            case 2: icon = _iconCupcake; break;
            default: icon = _iconCoffee; break;
        }            
        AddPowerupCard(icon);

        // if(_rhythmManager._powerUpQueue.Count > 0 && _powerupHolder.Visible)
        // {
        //     // Swap icon with one behind
        //     if (!_secondaryPowerupHolder.Visible)
        //         _secondaryPowerupHolder.Visible = true;
            
        //     // Icon 
        //     switch (_rhythmManager._gauge._puType){
        //         case 0: _secondaryPowerupIcon.Texture = _iconCoffee; break;
        //         case 1: _secondaryPowerupIcon.Texture = _iconSugarcane; break;
        //         case 2: _secondaryPowerupIcon.Texture = _iconCupcake; break;
        //     }                
        // }
        // else
        // {
        //      ShowPowerupHolder();
        // }   
    }

    // private void ShowPowerupHolder()
    // {
    //     if (!_powerupHolder.Visible)
    //         _PowerupAnimator.Play("PowerAdd");

    //     _secondaryPowerupHolder.Visible = false;
    //     _powerupPie.Visible = false;
    //     _powerupHolder.Visible = true;

    //     if (!_PowerupAnimator.IsPlaying())
    //         _PowerupAnimator.Play("PowerReady");

    //     // Icon 
    //     switch (_rhythmManager._gauge._puType){
    //         case 0: _powerupIcon.Texture = _iconCoffee; break;
    //         case 1: _powerupIcon.Texture = _iconSugarcane; break;
    //         case 2: _powerupIcon.Texture = _iconCupcake; break;
    //     }           
    // }

    private void OnPowerupEnded(int index)
    {
        if(_powerupHolderQueue.Count <= index)
        {
            return;
        }

        // Remove card from list
        _powerupHolderQueue[index].QueueFree();
        _powerupHolderQueue.RemoveAt(index);

        // Move cards behind it to front now
        for(int i = 0; i < index; i++)
        {
            _powerupHolderQueue[i].ShiftDown();
            _powerupHolderQueue[i].Position += _CardOffset;
        }

        // _powerupPie.Visible = false;
        // if (_rhythmManager._gauge != null && _rhythmManager._gauge.Value > 0)
        // {
        //     ShowPowerupHolder();
        // }
        // else
        // {
        //     _powerupHolder.Visible = false;
        //     _secondaryPowerupHolder.Visible = false;
        // }
    }

    private void AddPowerupCard(Texture2D icon)
    {
        // Move all the cards up
        foreach(PowerupHolder card in _powerupHolderQueue)
        {
            card.ShiftUp();
            card.Position -= _CardOffset;
        }

        // Place the new card at the end of the queue
        PowerupHolder ph = (PowerupHolder)ResourceLoader.Load<PackedScene>(_PowerupHolderScene).Instantiate();
        _powerupHolderQueue.Add(ph);
        GD.Print("Added card");

        _powerupHolders.AddChild(ph);
        ph.ShowReady(icon);
    }

    private void OnRhythmEventStart()
    {
        if(_powerupHolderQueue.Count > 0)
        {
            _powerupHolderQueue[_powerupHolderQueue.Count - 1].StartRhythmEvent();
        }
    }

    private void OnRespawn()
    {
        foreach(PowerupHolder ph in _powerupHolderQueue)
        {
            ph.QueueFree();
        }
        _powerupHolderQueue.Clear();
    }
}
