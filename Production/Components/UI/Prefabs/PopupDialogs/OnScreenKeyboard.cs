using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;

public partial class OnScreenKeyboard : Control
{
    [Signal] public delegate void OnKeyEnteredEventHandler(int character);
    [Signal] public delegate void OnBackspaceEnteredEventHandler();
    [Export] protected Control _row1;
    [Export] protected Control _row2;
    [Export] protected Control _row3;
    private Array<CursorButton> _letters = new Array<CursorButton>();
    private bool _caps = false;

    public override void _Ready()
    {
        foreach(Node node in _row1.GetChildren())
        {
            if(node is CursorButton)
            {
                CursorButton btn = node as CursorButton;
                _letters.Add(btn); 
            }
        }
        foreach(Node node in _row2.GetChildren())
        {
            if(node is CursorButton)
            {
                CursorButton btn = node as CursorButton;
                _letters.Add(btn); 
            }
        }
        foreach(Node node in _row3.GetChildren())
        {
            if(node is CursorButton)
            {
                CursorButton btn = node as CursorButton;
                _letters.Add(btn); 
            }
        }
    }

    public void OnKeyPressed(PlayerCurser cursor, int buttonId)
    {
        int value = buttonId;
        if(_caps && buttonId >= 97)
        {
            value -= 32;
        }
        EmitSignal(nameof(OnKeyEntered), value);
    }

    public void OnBackspacePressed(PlayerCurser cursor, int buttonId)
    {
        EmitSignal(nameof(OnBackspaceEntered));
    }

    public void OnCapsPressed(PlayerCurser cursor, int buttonId)
    {
        // All keys' text are capitalized
        _caps = !_caps;
        if(_caps)
        {
            foreach(CursorButton btn in _letters)
            {
                btn.SetText(((char)(btn.GetButtonId() - 32)).ToString());
            }
        }
        else
        {
            foreach(CursorButton btn in _letters)
            {
                btn.SetText(((char)btn.GetButtonId()).ToString());
            }            
        }
    }
}
