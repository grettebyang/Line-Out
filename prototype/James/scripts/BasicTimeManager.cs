using Godot;
using System;
using System.Security.Cryptography.X509Certificates;
//this is EXTREMELY basic. Used to ensure obstacles/hazards/traps work correctly
//as they should be synced to some sort of timer
//this should be replaced with a singleton
//
public partial class BasicTimeManager : Node {
    [Export]
    public float GameTime, MenuTime;
    [Export]
    Curve SpeedInterpolation;
    float test;
    [Export]
    public float GameSpeed = 1;
    public enum TIMESTATE{
        MENU,
        GAME,
        RHYTHM,
        INITIAL
    }

    private static BasicTimeManager _instance;

    private BasicTimeManager() {
        if (_instance != null)
            return;

        _instance = this;
    }

    public static BasicTimeManager GetInstance() {
        return _instance;
    }

    public TIMESTATE State = TIMESTATE.GAME;

    public override void _Process(double delta){
        switch(State){
            case TIMESTATE.INITIAL:
            GameTime = 0;
            GameSpeed = 1;
            break;
            case TIMESTATE.MENU:
            GameSpeed = 0;
            break;
            case TIMESTATE.GAME:
                GameSpeed = Mathf.Lerp(GameSpeed, 1f, SpeedInterpolation.Sample(1 -GameSpeed) * (float)delta);
                GameSpeed = Math.Clamp(GameSpeed, 0, 1);
                
            break;
            case TIMESTATE.RHYTHM:
                GameSpeed = Mathf.Lerp(GameSpeed, 0.1f, SpeedInterpolation.Sample(GameSpeed)* (float)delta);
                GameSpeed = Math.Clamp(GameSpeed, 0, 1);
            break;
        }
    }

    public void MusicState() {
        State = TIMESTATE.RHYTHM;
    }
    public void GameState() {
        State = TIMESTATE.GAME;
    }
    public void MenuState() {
        State = TIMESTATE.MENU;
        GameSpeed = 0;
    }
    public float GetGameSpeed() {
        return GameSpeed;
    }
}
