using Godot;
using System;
using Godot.Collections;

public partial class OptionsHeader : Control
{
    Array<CursorButton> _buttons = new Array<CursorButton>();
    private int _currentOption = 0;

    public int CurrentOption { get { return _currentOption; } }

    public delegate void OnOptionChange(int currentOption);
    public OnOptionChange _onOptionChange;

    public override void _Ready()
    {
        foreach (Node node in GetChildren())
        {
            CursorButton bnt = node as CursorButton;
            //bnt.MouseEntered += OnMouseEnter;

            _buttons.Add(bnt);
        }

        // this.FocusEntered += OnFocusEnter;
        // this.MouseEntered += OnFocusEnter;
    }

    public override void _Input(InputEvent @event){
        // Check hover
        int i = 0;
        // foreach (Button btn in _buttons){

        //     if (btn.HasFocus())
        //     {
        //         if (_currentOption != i)
        //             Select(i);
        //     }

        //     if (Input.IsMouseButtonPressed(MouseButton.Left) && btn.IsHovered())
        //     {
        //         Select(i);
        //     }

        //     i++;
        // }
    }

    public void Select(PlayerCurser cursor, int selection)
    {
        _currentOption = selection;
        _onOptionChange?.Invoke(selection);
    }

    public void OnFocusEnter()
    {
        _buttons[_currentOption].Select();
    }

    public void OnFocusEnter(int option)
    {
        _buttons[option].Select();        
    }
}
