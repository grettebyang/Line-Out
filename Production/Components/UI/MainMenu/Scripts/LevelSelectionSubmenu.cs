using Godot;
using System;
using System.Threading.Tasks;

public partial class LevelSelectionSubmenu : SubmenuBase
{
    [Export] protected PackedScene[] _levels;
    [Export] ControllersActivatorUI _activatorUI;
    [Export] LevelDescription[] _levelDescriptions;

    protected int _selectedLevel = -1;
    protected int _levelsHovered = 0;

    [Signal] public delegate void OnSelectLevelEventHandler(string path);

    public override void _Ready()
    {
        base._Ready();
    }


    public override void Confirm()
    {

    }

    public string GetLevelPath()
    {
        return _levels[_selectedLevel].ResourcePath;
    }

    protected override void OnFocusChange(Control node)
    {
        // base.OnFocusChange(node);

        // _selectedLevel = _mainButtons.IndexOf(_selectedButton);

        // // Show current description
        // if (_selectedLevel != -1)
        // {
        //     foreach (LevelDescription desc in _levelDescriptions)
        //     {
        //         desc.Visible = false;
        //     }

        //     _levelDescriptions[_selectedLevel].Visible = true;
        // }
    }

    public void HoverLevel(PlayerCurser cursor, int buttonId)
    {
        // Show current description
        foreach (LevelDescription desc in _levelDescriptions)
        {
            desc.ZIndex--;
        }

        _levelsHovered++;
        _levelDescriptions[buttonId].ZIndex = 0;
        //_levelDescriptions[buttonId].Visible = true;   
    }

    public void UnHoverLevel(PlayerCurser cursor, int buttonId)
    {
        foreach (LevelDescription desc in _levelDescriptions)
        {
            desc.ZIndex = Math.Min(desc.ZIndex + 1, 0);
        }

        _levelsHovered--;
        _levelDescriptions[buttonId].ZIndex = -1 - _levelsHovered;
        //_levelDescriptions[buttonId].Visible = false;   
    }

    public void SelectLevel(PlayerCurser cursor, int buttonId)
    {
        // Prevent start if no contoler is active
        if (InputSystem.GetInstance().ActiveControllersCount() == 0)
        {
            return;
        }

        // Start level
        _selectedLevel = buttonId;
        if (_selectedLevel != -1)
        {
            string levelPath = GetLevelPath();
            EmitSignal(nameof(OnSelectLevel), levelPath);
        }
    }
}
