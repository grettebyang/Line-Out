using Godot;
using System;
using System.Collections.Generic;

public enum EHUDMessageType{
    MESSAGE,
    WARNING,
    POSITIVE,
}

public partial class HUDMessage : Control
{
    class HUDMessageEntry{
        public string message;
        public float time;
        public EHUDMessageType type;
    }

    const float DEFAULT_DISPLAY_TIME = 2.0f;

    [Export] protected Control _messageHolder;
    [Export] protected Label _lblMessage;
    [Export] protected float _fadeoutSpeed = 3;
    [Export] protected AnimationPlayer _animator;

    [Export] Color _colorMessage = new Color(1,1,1,1);
    [Export] Color _colorWarning = new Color(0.86f, 0.42f, 0.42f, 1);
    [Export] Color _colorPositive = new Color(0.91f, 0.89f, 0.45f, 1);

    private static List<HUDMessageEntry> _messageQueue = new List<HUDMessageEntry>();
    private HUDMessageEntry _activeMessage;
    private Color _lastColor;

    public override void _Ready(){
        base._Ready();
    }


    public override void _Process(double delta){
        
        if (_messageQueue.Count == 0)
            return;

        // Show messages in queue
        if (_activeMessage == null){
            _activeMessage = _messageQueue[0];
            _lblMessage.Text = _activeMessage.message;

            // Play show anim 
            _animator.Play("Show");
        }

        _lastColor = _lblMessage.SelfModulate;

        if (_activeMessage.time > 0)
        {
            // Progress time
            _activeMessage.time -= (float)delta;
        }
        else
        {
            // Progress message
            _messageQueue.RemoveAt(0);
            
            if (_messageQueue.Count > 0)
            {
                _activeMessage = _messageQueue[0];
                _lblMessage.Text = _activeMessage.message;
            }
            else
            {
                // Hide after all message are displayed
                _activeMessage = null;
                _animator.Play("Hide");
            }
        }
    }

    public static void DisplayMessage(string message, EHUDMessageType type = EHUDMessageType.MESSAGE, float time = DEFAULT_DISPLAY_TIME){
        HUDMessageEntry msg = new HUDMessageEntry();
        msg.message = message;
        msg.type = type;
        msg.time = time;

        _messageQueue.Add(msg);
    }
}
