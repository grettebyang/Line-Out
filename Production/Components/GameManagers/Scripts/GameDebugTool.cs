using Godot;
using System;

public partial class GameDebugTool : Node
{
    [Export] public int DogsToSimlate = 3;

    [ExportCategory("UI")]
    [Export] Control _canvas;
    [Export] Label _lblLevelId;
    [Export] Label _lblFps;

    GameManager _gameManager;
    InputSystem _inputSystem;

    float _currentBestTime = -1;

    public override void _Ready()
    {
        _gameManager = GameManager.GetInstance();
        _inputSystem = InputSystem.GetInstance();
    }

    public override void _Process(double delta)
    {
        if (!_canvas.Visible && _gameManager.DebugMode)
        {
            LevelProgressSave save = _gameManager._saveManager.FindLevelProgress(_gameManager.CurrentLevelId);
            if (save != null)
                _currentBestTime = save.BestTime;
            else
                _currentBestTime = -2;
        }

        _canvas.Visible = _gameManager.DebugMode;

        // Activation
        if (!_gameManager.DebugMode)
            return;

        // Show
        string strTime = "not recorded";
        if (_currentBestTime != -2)
            strTime = LevelManager.TimeFormated(_currentBestTime);

        _lblLevelId.Text = "Level: " + _gameManager.CurrentLevelId + " (" + strTime + ")";
        _lblFps.Text = "FPS: " + Engine.GetFramesPerSecond();
    }

}
