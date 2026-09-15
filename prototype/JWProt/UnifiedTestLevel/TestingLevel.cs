using Godot;
using System;

public partial class TestingLevel : Node
{
    [Export] Label _uiCaseName;

    public TestLevelCase[] _cases;
    public static int _currentCase = 0;

    DogSled _sled;

    public override void _Ready()
    {
        // Refs
        _sled = GameManager.GetInstance().CurrentLevel._sled;

        // Setup cases
        _cases = new TestLevelCase[this.GetChildCount()];
        for (int i = 0; i < _cases.Length; i++)
        {
            _cases[i] = this.GetChild(i) as TestLevelCase;
            ActivateCase(i, false);
        }

        ActivateCase(_currentCase, true);
    }


    public override void _Process(double delta)
    {
        // Switch between levels
        if (Input.IsActionJustPressed("LevelCaseNext"))
        {
            // Next
            ActivateCase(_currentCase, false);

            _currentCase++;
            if (_currentCase >= _cases.Length)
            {
                _currentCase = 0;
            }

            ActivateCase(_currentCase, true);
        }
        else if (Input.IsActionJustPressed("LevelCasePrevious"))
        {
            // Previous
            ActivateCase(_currentCase, false);

            _currentCase--;
            if (_currentCase < 0)
            {
                _currentCase = _cases.Length - 1;
            }

            ActivateCase(_currentCase, true);
        }

        // UI update
        int max = _cases.Length - 1;
        _uiCaseName.Text = "Case: " + _cases[_currentCase].Name + " (" + _currentCase + "/" + max + ")";
    }

    public void ActivateCase(int i, bool activate)
    {
        // Deactivate
        if (!activate)
        {
            _cases[i].ProcessMode = ProcessModeEnum.Disabled;
            _cases[i].Visible = false;
            return;
        }

        // Activate
        _cases[i].ProcessMode = ProcessModeEnum.Always;
        _cases[i].Visible = true;

        // Move sled
        Node3D posPoint = _cases[i];
        if (_cases[i]._enterPoint != null)
        {
            posPoint = _cases[i]._enterPoint;
        }

        _sled.GlobalPosition = posPoint.GlobalPosition;
        _sled.Rotation = new Vector3(0, posPoint.GlobalRotation.Y, 0);
        _sled.PositionCharacters();
    }
}
