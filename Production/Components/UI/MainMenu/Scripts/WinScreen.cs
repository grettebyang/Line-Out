using Godot;
using System;
using System.Threading.Tasks;

public partial class WinScreen : SubmenuBase
{
    [Export] protected Label _lblTitle;
    [Export] protected Label _lblTime;
    [Export] protected Label _lblBestTime;
    [Export] protected Label _lblNextTime;
    [Export] protected Control _ratingHolder;
    [Export] protected AnimationPlayer _animator;
    [Export] protected Timer _waitTimer;
    [Export] bool _skipShowDelay;
    [Export] protected float _showSelectionDelay = 2;
    [Export] protected float _focusSelectionDelay = 0.5f;

    [Export] protected TempScoreboardUI _scoreboardUI;
    [Export] protected CursorButton _btnSaveScore;
    [Export] protected CursorButton _btnNextLevel;

    public enum EWinScreenStates
    {
        APPEAR,
        SHOW_SELECTION,
        FOCUS_SELECTION
    }

    protected EWinScreenStates _state = EWinScreenStates.APPEAR;
    protected string _nextLevelPath = "";

    public override void _Ready()
    {
        base._Ready();
        Visible = false;
        ListenerSetup();
    }
    public override void _Input(InputEvent @event)
    {
        if (!Visible)
            return;


        if (@event.IsActionPressed(UIConstants.ACTION_CONFIRM))
        {
            Confirm();
        }
    }

    //------------------------------------------
    // Custom methods
    //------------------------------------------

    protected void OnTargetStateHappened()
    {
        Open();
        _scoreboardUI.Open();

        GameManager gm = GameManager.GetInstance();
        LevelManager levelManager = gm.CurrentLevel;
        _nextLevelPath = DBLevels._instance.GetNextLevelPath(levelManager);
        
        if(_nextLevelPath == "UnregistredLevel")
        {
            _btnNextLevel.SetDisabled();
        }
        else
        {
            _btnNextLevel.SetEnabled();
        }
        //_btnSaveScore.Disabled = false;
        //_mainButtons[0].GrabFocus();
        _waitTimer.Timeout += OnWaitTimerTimeout;

        // Start animations
        if (!_skipShowDelay)
        {
            _animator.Play("Appear");
            _state = EWinScreenStates.APPEAR;

            _waitTimer.WaitTime = _showSelectionDelay;
            _waitTimer.Start();
        }
        else
        {
            // Skip to show
            _state = EWinScreenStates.SHOW_SELECTION;
            _animator.Play("ShowSelection");
            _waitTimer.WaitTime = _focusSelectionDelay;
            _waitTimer.Start();
        }

        // Show time
        if (_lblTime != null)
        {
            levelManager = GameManager.GetInstance().CurrentLevel;

            _lblTime.Text = "Time: " + LevelManager.TimeFormated(levelManager._sessionTime);

            // Get scoreboard comparison
            TempScoreboard scoreboard =  GameManager.GetInstance().TempScoreboard;
            int placementPos = scoreboard.PositionOfTime(levelManager._sessionTime) + 1;
            int recordsCount = scoreboard.Scoreboard.Records.Count;
            string placement = "(" + placementPos + ". place from " + recordsCount + ")";

            _scoreboardUI.SetPlaceholderRecord(placementPos - 1, levelManager._sessionTime);
            _scoreboardUI.DisplayPageByPosition(placementPos - 1);
        }

        LevelProgressSave save = GameManager.GetInstance()._saveManager.FindLevelProgress(gm.CurrentLevelId);
        
        if (save != null && save.BestTime < levelManager._sessionTime)
            _lblBestTime.Text = "Best time: " + LevelManager.TimeFormated(save.BestTime);
        else
            _lblBestTime.Text = "Best time: " + LevelManager.TimeFormated(levelManager._sessionTime);

        // Hide if no best time
        if (save == null || save.BestTime == -1)
            _lblBestTime.Text = "";
    }

    protected void ShowRating()
    {
        GameManager gm = GameManager.GetInstance();
        LevelResource level = DBLevels._instance.ResourceById(gm.CurrentLevelId);
        LevelManager levelManager = GameManager.GetInstance().CurrentLevel;

        int rating = 0;

        foreach (float timeRating in level.TimesToRating)
        {
            if (levelManager._sessionTime <= timeRating)
            {
                Control star = _ratingHolder.GetChild(rating) as Control;
                star.Visible = true;
                rating++;
            }
        }

        if (rating < 3)
            _lblNextTime.Text = "Next star time: " + LevelManager.TimeFormated(level.TimesToRating[rating]);
        else
            _lblNextTime.Text = "";
    }

    protected virtual void ListenerSetup()
    {
        GameManager.GetInstance().CurrentLevel.OnFinished += OnTargetStateHappened;
        GameManager.GetInstance().CurrentLevel.OnFinished += ShowRating;
    }

    // Save to leaderboard
    protected void OnSaveScoreButtonPresed(PlayerCurser cursor, int characterId)
    {
        // if (_btnSaveScore.Disabled)
        //     return;

        SaveScoreDialog dialog = SaveScoreDialog.GetInstance();
        dialog.Show(true, EConfirmDialogStyle.OK_CANCEL, "Insert team name");
        dialog.OnPositiveConfirm += OnSaveScoreConfirm;
        //dialog.OnClosed += OnRestartDialogClose;
    }

    private void OnSaveScoreConfirm(SaveScoreDialog dialog)
    {
        _btnSaveScore.SetDisabled();
        dialog.OnPositiveConfirm -= OnSaveScoreConfirm;
    }

    // Restart
    protected void OnRestartButtonPresed(PlayerCurser cursor, int characterId)
    {
        ConfirmDialogBase dialog = PauseMenu.RestartDialog();
        dialog.OnPositiveConfirm += OnRestartDialogConfirm;
        dialog.OnClosed += OnRestartDialogClose;
    }

    private void OnRestartDialogConfirm(ConfirmDialogBase dialog)
    {
        dialog.Show(false);
        dialog.OnPositiveConfirm -= OnRestartDialogConfirm;
        dialog.OnClosed -= OnRestartDialogClose;
        GameManager.GetInstance().ReloadLevel();
        PlayerCurserContainer.GetInstance().HideCursors();
    }

     private void OnRestartDialogClose(ConfirmDialogBase dialog)
    {
        dialog.OnPositiveConfirm -= OnRestartDialogConfirm;
        dialog.OnClosed -= OnRestartDialogClose;
    } 

    // To menu
    protected void OnToMainMenuPressed(PlayerCurser cursor, int characterId)
    {
        ConfirmDialogBase dialog = PauseMenu.QuitDialog();
        dialog.OnPositiveConfirm += OnQuitDialogConfirm;
        dialog.OnClosed += OnQuitDialogClose;
    }

    private void OnQuitDialogConfirm(ConfirmDialogBase dialog)
    {
        dialog.Show(false);
        dialog.OnPositiveConfirm -= OnQuitDialogConfirm;
        dialog.OnClosed -= OnQuitDialogClose;
        GameManager.GetInstance().LoadMenu();
    }

    private void OnQuitDialogClose(ConfirmDialogBase dialog)
    {
        dialog.OnPositiveConfirm -= OnQuitDialogConfirm;
        dialog.OnClosed -= OnQuitDialogClose;
    }

    // Next level
    private void OnNextLevelPressed(PlayerCurser cursor, int buttonId)
    {
        PlayerCurserContainer.GetInstance().HideCursors();
        GameManager.GetInstance().LoadLevel(_nextLevelPath);
    }

    private void OnWaitTimerTimeout()
    {
        // Appear
        if (_state == EWinScreenStates.APPEAR)
        {
            _state = EWinScreenStates.SHOW_SELECTION;
            _animator.Play("ShowSelection");
            _waitTimer.WaitTime = _focusSelectionDelay;
            _waitTimer.Start();

            return;
        }

        // Show selection
        if (_state == EWinScreenStates.SHOW_SELECTION)
        {
            _state = EWinScreenStates.FOCUS_SELECTION;
            PlayerCurserContainer.GetInstance().ShowCursors();
            return;
        }

        if (_state == EWinScreenStates.FOCUS_SELECTION)
        {
            _waitTimer.Timeout -= OnWaitTimerTimeout;
        }
    }
}
