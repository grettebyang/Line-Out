using Godot;
using System;

public partial class ButtonDecoration : BeatListener
{
    public void OnFocus(){
        Visible = true;
    }

    public void OnFocusLeave(){
        Visible = false;
    }
}
