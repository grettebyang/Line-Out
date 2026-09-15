using Godot;
using System;

public partial class FailScreen : WinScreen
{
    protected override void ListenerSetup()
    {
        GameManager.GetInstance().CurrentLevel.OnFail += OnFail;
    }

    protected void OnFail(string reason)
    {
        OnTargetStateHappened();
        
        // Custom title
        if (_lblTitle != null && reason != "" && reason != string.Empty)
        {
            _lblTitle.Text = reason;
        }
    }
}
