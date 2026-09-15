using Godot;
using System;

public partial class RhythmCalibrationSubmenu : SubmenuBase
{
    [Export]
    private Label _hintText;
    [Export]
    private Label _latencyDisplayText;
    [Export]
    private Control _skipHint;
    private InputSystem _inputSystem;
    private RhythmManager _rhythmManager;
    private CalibrationModes _calibrationMode;

    private static String[] HintTexts = { 
        "Volume Up!\nListen... Press B on the beat",
        "Press B when the blue rings are inside the yellow ring",
        "Calibration Complete"
    };

    private enum CalibrationModes
    {
        AUDIO,
        VISUAL,
        COMPLETE,

        NUMBER_OF_CALIBRATION_MODES
    }

    public override void _Ready()
    {
        base._Ready();
        _inputSystem = InputSystem.GetInstance();
        _rhythmManager = RhythmManager.GetInstance();
        _rhythmManager.OnRhythmCalibrationComplete += OnRhythmCalibrationComplete;
        _rhythmManager.OnAudioCalibrationComplete += OnAudioCalibrationComplete;
        _rhythmManager.DisplayCurrentLatency += DisplayLatency;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        _rhythmManager.OnRhythmCalibrationComplete -= OnRhythmCalibrationComplete;
        _rhythmManager.OnAudioCalibrationComplete -= OnAudioCalibrationComplete;
        _rhythmManager.DisplayCurrentLatency -= DisplayLatency;
    }


    public override void Close()
    {   
        base.Close();
        PlayerCurserContainer.GetInstance().SetCursorsFrozen(false);
        PlayerCurserContainer.GetInstance().Visible = true;
        _rhythmManager.EndRhythmCalibration();
        _rhythmManager.PlayMainMenuMusic(true);
        _inputSystem.RemoveActiveInputMode(EInputSystemMode.RYTHM_EVENT);
        //_inputSystem.AddActiveInputMode(EInputSystemMode.MENU);
    }
    public override void Open()
    {
        base.Open();
        PlayerCurserContainer.GetInstance().SetCursorsFrozen(true);
        PlayerCurserContainer.GetInstance().Visible = false;
        // Start calibration event
        _rhythmManager.BeginRhythmCalibration();
        _calibrationMode = CalibrationModes.AUDIO;
        _hintText.Text = HintTexts[(int)_calibrationMode];

        //_inputSystem.RemoveActiveInputMode(EInputSystemMode.MENU);
        _inputSystem.AddActiveInputMode(EInputSystemMode.RYTHM_EVENT);
        _skipHint.Visible = true;
    }

    // public override bool Back()
    // {
    //     if(_calibrationMode == CalibrationModes.COMPLETE)
    //     {
    //         return true;
    //     }
    //     return false;
    // }

    public void OnRhythmCalibrationComplete()
    {
        _calibrationMode = CalibrationModes.COMPLETE;
        _hintText.Text = HintTexts[(int)_calibrationMode];
        _latencyDisplayText.Text = "Audio Latency: " + ((int)(-1000 * _rhythmManager._audioCalibrationOffset)).ToString() + "ms" + "\nVideo Latency: " + ((int)(-1000 * _rhythmManager._calibrationOffset)).ToString() + "ms";

        _inputSystem.RemoveActiveInputMode(EInputSystemMode.RYTHM_EVENT);
        _inputSystem.AddActiveInputMode(EInputSystemMode.MENU);

        _skipHint.Visible = false;
    }

    public void OnAudioCalibrationComplete()
    {
        _calibrationMode = CalibrationModes.VISUAL;
        _hintText.Text = HintTexts[(int)_calibrationMode];
    }

    public void DisplayLatency(float latency)
    {
        _latencyDisplayText.Text = "Latency: " + ((int)(-1000 * latency)).ToString() + "ms";
    }
}
