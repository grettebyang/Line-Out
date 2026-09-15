using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

public delegate void EventVersion();
public delegate void OnHike();

interface IPowerupEffects{
	void SpeedBoost(float mult);
	void SpeedBoostEnd(float mult);

	void Hover();
	void HoverEnd();

	void Invincibility();
	void InvincibilityEnd();
}

public partial class RhythmManager : Node
{

	// Called when the node enters the scene tree for the first time.
	[Export]
	public int _bpm;
	[Export]
	public float _timeCushion;	
	[Export]
	public Soundtrack _soundtrack;
	[Export]
	public EventFeedbackPlayer _eventFeedbackSound;
	[Export]
	public AudioStreamPlayer _goalReachedMusic;
	[Export]
	public RhythmUI _rhythmUI;
	[Export]
	public SoundtrackCollection _soundtrackCollection;

	public List<Node> _dogs;
	public DogSled _sled;
	public PowerupGauge _gauge;
	public bool _sequenceGo = false;
	public float _metronome = 0.0f;
	public double _metPrev = 0.0d;
	public Powerup _curPowerup;
	public int[] _beatInput;
	public List<ButtonPrompt> _beatList = new List<ButtonPrompt>();
	public List<int> _beatSequence = new List<int>();
	public List<int> _beatAccuracySequence = new List<int>();
	public List<int> _feedbackReady = new List<int>();
	public bool onBeat = false;
	public int _beatIndex = 0;
	public int _barLength;
	public int _initialBeatsBetween = 0;
	public int _curEventSeqLength;
	public int _curEventSeqBeatCount;
	public int _sequenceStartIndex;
	public List<int> _curEventSequence = new List<int>();
	public int[] _currentBeatsPlayed;
	public int[] _beatIndexes;
	public float _powerupDuration;
	public int _beatCombo = 0;
	public bool _nextBeat = false;
	public Label _textPrompt;
	public double _delay = 0.0d;	
	public int _state;
	public int _playerCount;
	public bool _muted = false;
	public float _accuracy = 0.0f;
	public float _beatAccuracy = 0.0f;
	public Control[] _fbLabels = new Control[0];
	public List<Collectable> _collectables;
	public bool _onBeat = false;
	public int _countdown;
	public bool _paused = false;
	public bool _levelEnded = false;

	// Rhythm event trigger
	public int _beatsBarked = 0; // When this is 3, _triggerEvent = true;
	public bool _triggerEvent = false;

	// Rhythm calibration variables
	JSONGameSettings _jsonSettings;
    string _settingsPath = "/game_settings.save";
	private int _beatsHit = 0; // when this reaches 8, calibration is complete
	private int _playersHit = 0; // this is the number of players who hit the beat on a given beat
	private float _curDifference = 0.0f; // the average difference on a given beat
	private float _avgDifference = 0.0f; // the average difference on a given beat
	private float _beatActualTime = 0.0f; // the time in the soundtrack that the current on beat falls on
	public float _calibrationOffset = 0.0f; // set this to _avgDifference on calibration complete
	[Export] public float _audioCalibrationOffset = 0.0f; // this will adjust the actual playback position in the soundtrack when calculating beat timing
	public bool _audioCalibrationComplete = false;
	public bool _rhythmCalibrationComplete = false;


	public List<int> _initialEventInputs = new List<int>();
	public List<ActiveEffect> _powerUpQueue;

	public int prevBeat = 0;

	public event EventVersion SeqRun;
	public event EventVersion SeqGo;
	public event OnHike Hike;
	public event EventVersion BeatPulse;
	[Signal] public delegate void OnRhythmCalibrationCompleteEventHandler();
	[Signal] public delegate void OnAudioCalibrationCompleteEventHandler();
	[Signal] public delegate void DisplayCurrentLatencyEventHandler(float latency);
	[Signal] public delegate void OnPowerupActivateEventHandler(float duration, float maxDuration);
	[Signal] public delegate void OnPowerupCollectedEventHandler();
	[Signal] public delegate void OnPowerupEndedEventHandler(int index);
	[Signal] public delegate void OnRhythmEventStartEventHandler();


	public Godot.Collections.Array<InputSystemController> _controllers;

	private InputSystem inputSystem;
	private BasicTimeManager _timeManager;
	public LevelManager levelManager;

	public class ActiveEffect{
		public float _dur { get; set; }
		public Powerup _pow { get; set; }
		public bool _initialBoost;
		public ActiveEffect(Powerup pow, float dur){
			this._dur = dur;
			this._pow = pow;
			this._initialBoost = false;
		}
		public ActiveEffect(Powerup pow, float dur, bool initialBoost){
			this._dur = dur;
			this._pow = pow;
			this._initialBoost = initialBoost;
		}
	}

    //------------------------------------------
    // Singleton
    //------------------------------------------

    private static RhythmManager _instance;

    private RhythmManager(){
		if (_instance != null)
            return;

		_instance = this;
	}

    public static RhythmManager GetInstance()
	{
		return _instance;
	}

	public void ActivateController(InputSystemController controller, int slotId){
		//GD.Print("controller activate");		
        _controllers = inputSystem.GetOnlyActiveControllers();
		_playerCount = _controllers.Count;
		//_playerCount++;
		//GD.Print(_playerCount);
	}

	public void DeactivateController(InputSystemController conroller, int slotId){
		//GD.Print("controller deactivate");
        _controllers = inputSystem.GetOnlyActiveControllers();
		_playerCount = _controllers.Count;
		//_playerCount--;
		//GD.Print(_playerCount);
	}

	private void OnControllersActivationsAllowed(bool allowed) // This is really only important for debugging purposes
    {
		if (!allowed)
		{
			InputSystem.GetInstance().OnControllerActivationAllowed -= OnControllersActivationsAllowed;
			SetReferences();
		}
    }

	public void SetReferences()
	{
		if (GameManager.GetInstance().CurrentLevel == null)
		{
			GD.PrintErr("RhythmManager.SetReferences() - Loaded level doesn't contain level manager!");
			PlayMainMenuMusic(false);
			OnLevelRestart();
			return;
		}
		levelManager = GameManager.GetInstance().CurrentLevel;

		ResetBeatsPlayed();
		ResetBeatIndexes();
		ResetBeatInput();
		_soundtrack.Stop();
		_soundtrack = _soundtrackCollection.GetSoundtrack(levelManager.Soundtrack);
		_soundtrack._audioCalibrationOffset = _audioCalibrationOffset;
		_soundtrack.Reset();
		_bpm = _soundtrack.bpm;

		levelManager.OnFinished += OnGoalReached;
		//EndAllPowerups();

		//levelManager.OnPauseToggle += TogglePause;
		_sled = GameManager.GetInstance().CurrentLevel._sled;
		_dogs = _sled.DogSpawner.DogList;
		_playerCount = _controllers.Count;
		_sled.PackageCarrier.Health.OnDeath += OnLevelRestart;
		OnLevelRestart();
		CreateFeedbackLabels();


		_powerUpQueue = new List<ActiveEffect>();

		GetCollectables();
		for (int i = 0; i < _collectables.Count; i++)
		{
			_collectables[i].OnCollectableEntered += OnCollectableEntered;
		}

		// Disable rhythm event if it is happening


		EmptyBeatList();
		SeqRun -= InitialRunSequence;
		PreLevelRhythmEvent();
	}

	public void OnBeforeLevelDelete(LevelManager level)
	{
		_levelEnded = false;
		EndAllPowerups();
	}

	public void OnLevelRestart(){
		_paused = false;
		_levelEnded = false;
		_rhythmUI.LevelRestart(_bpm);
		_rhythmUI.SetupBarkTrigger(_playerCount);
		_goalReachedMusic.Stop();
		EmptyBeatList();
		EndSequence();
		EndAllPowerups();
	}

	public void EmptyBeatList(){
		for(int i = 0; i < _beatList.Count; i++){
			if(IsInstanceValid(_beatList[i])){
				_beatList[i].Free();
			}
		}
		_beatList = new List<ButtonPrompt>(0);
	}

	public void PreLevelRhythmEvent() { 
		_metronome = 0.0f;
		_soundtrack.Reset();
		_soundtrack.Play();
		ResetBeatsPlayed();
		ResetBeatIndexes();
		ResetBeatInput();
		_beatAccuracy = 0.0f;
		_accuracy = 0.0f;
        inputSystem.RemoveActiveInputMode(EInputSystemMode.PLAYER_MOVEMENT);
		inputSystem.AddActiveInputMode(EInputSystemMode.RYTHM_EVENT);
		_sequenceStartIndex = 16;
		_sequenceGo = true;
		//_delay = (2 * (60f/(float)_bpm));
		SeqRun -= RunSequence2; 
		SeqRun += InitialRunSequence;
		_beatSequence = _soundtrack._initialSequence; // 7/8 [1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0] 6/8 [1, 0, 0, 1, 0, 0, 1, 0, 0] 5/4 [1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0]
		_curEventSeqLength = _beatSequence.Count;
		_curEventSeqBeatCount = 4;
		_countdown = 5;
		_curPowerup = GetNode<Powerup>("Powerups/Boost");
		_barLength = _soundtrack._tsTop;
		//_initialEventInputs = _soundtrack._initialSequenceInput;

		_soundtrack.UpdateSoundtrack(_metronome);
		// int beatsRem = _soundtrack.GetBeatsUntilNextSequenceBar();
		// beatsRem = 15;
		int beatsBehind = 1;
		_beatIndex = _soundtrack._curBeat - 1;
		//GD.Print("difference: " + (_soundtrack.Stream.GetLength() - _soundtrack._calibratedPlaybackPosition));
		if(_audioCalibrationOffset < 0.0f && _soundtrack.Stream.GetLength() - _soundtrack._calibratedPlaybackPosition < .5f) // before start of loop
		{
			//beatsRem += (_soundtrack.Stream.GetLength() - _soundtrack._calibratedPlaybackPosition)/(60f/_bpm);
			_beatIndex -= _soundtrack._tsTop;
		}
		beatsBehind += _beatIndex;
		//GD.Print("_beatIndex: " + _beatIndex);
		//GD.Print("beats rem: " + beatsRem);
		//soundtrack has been updated
		_metronome = (_soundtrack._calibratedPlaybackPosition) % (60f/_bpm);
		//GD.Print("metronome: " + _metronome);
		//_delay = (beatsRem * (60d / (double)_bpm)) + (60d / (double)_bpm) - _metronome;
		//GD.Print("delay: " + _delay);
		for(int i = 0; i < _curEventSeqLength; i++)
        {
			int seqItem = _beatSequence[i];
			_curEventSequence.Add(seqItem);
			_beatAccuracySequence.Add(0);
			_feedbackReady.Add(0);
            if(seqItem == 1)
            {
				//GD.Print("Sequence Index: " + i);
				var scene = ResourceLoader.Load<PackedScene>("res://Production/Components/Rhythm/circle_prompt.tscn").Instantiate();
				CirclePrompt b = (CirclePrompt)scene;
				b.CreatePrompt(1, i - beatsBehind, (float)_bpm, (60d / (double)_bpm) - _metronome, _calibrationOffset); // ADDED "- 1" TO NUMBER OF BEATS PARAMETER
				//b = new ButtonPrompt(_curPowerup.sequence.Dequeue(), i+1, _center.Position);
				_rhythmUI.AddChild(scene);

				_beatList.Add(b);
            }
        }	
		_rhythmUI.PreLevelRhythmEvent((int)_curPowerup._numOfLevels);

		_sequenceGo = true;
		prevBeat = _soundtrack._curBeat;
	}

	public void CreateBeat(float rem, int i){
		var scene = ResourceLoader.Load<PackedScene>("res://Production/Components/Rhythm/circle_prompt.tscn").Instantiate();
		ButtonPrompt b = (ButtonPrompt)scene;
		b.CreatePrompt(1, i, (float)_bpm, rem, _calibrationOffset);
		//b = new ButtonPrompt(_curPowerup.sequence.Dequeue(), i+1, _center.Position);
		_rhythmUI.AddChild(scene);

		_beatList.Add(b);
	}

	public void InitialRunSequence(){
		if(_delay > (60f / (float)_bpm)) 
		{
			return;
		}

		RhythmEventProcessInput();

		if(_onBeat)
		{
			_onBeat = false;
			if(_beatIndex >= _soundtrack._tsTop * 2 && _beatIndex < _curEventSeqLength && _curEventSequence[_beatIndex] == 1)
			{
				_rhythmUI.BeginCountdown(_countdown);
				_countdown -= 1;
				_rhythmUI.CountdownPulse(_countdown);
			}
			else if((_beatIndex % _soundtrack._tsTop) % 2 == 0)
			{
				_rhythmUI.InitialEventPulse();
			}			
		}
		
		if(_beatIndex >= _curEventSeqLength && _beatIndex > 0){
			//Success scenario
			float accuracy = _accuracy/(_curEventSeqBeatCount * _playerCount);
			_curPowerup.PowerEffect(_dogs, _sled, _dogs.Count, accuracy);
			_powerUpQueue.Add(new ActiveEffect(_curPowerup, _curPowerup._duration, true));
			if(_accuracy > 0.0f){
				_eventFeedbackSound.PlayFeedbackSound(accuracy, 4.0f);
			}
			EndInitialEvent();
		}
	}

	public void BarkToTriggerRhythmEvent() // Listen for barks on beat, everyone has to bark on the beat three times, doesn't have to be consecutive
	{
		_triggerEvent = true;
		for(int j = 0; j < _playerCount; j++)
		{
			// _beatIndex doesn't matter here
			GatherInputForPlayer(j);
			if (_beatInput[j] == 1 && _currentBeatsPlayed[j] == 0)
			{
				_currentBeatsPlayed[j] = 1;
				_rhythmUI.BarkTriggerUpdate(j);
			}
			if(_currentBeatsPlayed[j] == 0)
			{
				_triggerEvent = false;
			}
		}
		ResetBeatInput();
	}

	public void BeginRhythmCalibration()
	{
		GD.Print("begin rhythm calibration");
		_metronome = 0.0f;
		_soundtrack.Stop();
		_soundtrack = _soundtrackCollection.GetSoundtrack(Tracknames.CALIBRATION_TRACK);
		_bpm = _soundtrack.bpm;
		_soundtrack._audioCalibrationOffset = 0.0f;
		_soundtrack.Reset();
		_soundtrack.Play(); // Set soundtrack to click track
		ResetBeatsPlayed();
		ResetBeatIndexes();
		ResetBeatInput();
        inputSystem.RemoveActiveInputMode(EInputSystemMode.PLAYER_MOVEMENT); // Remove menu input mode?
		inputSystem.AddActiveInputMode(EInputSystemMode.RYTHM_EVENT);
		_sequenceGo = true;
		//_delay = (2 * (60f/(float)_bpm));
		SeqRun -= InitialRunSequence;
		SeqRun -= RunSequence2; 
		SeqRun += RhythmCalibration;
		_beatSequence = [1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0]; // 7/8 [1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0] 6/8 [1, 0, 0, 1, 0, 0, 1, 0, 0] 5/4 [1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0]
		_curEventSeqLength = _beatSequence.Count;
		_curEventSeqBeatCount = 3;
		_beatIndex = 0;

		_audioCalibrationComplete = false;
		_rhythmCalibrationComplete = false;
		_beatsHit = 0;
		_beatActualTime = 0.0f;
		ResetCalibrationVariables();

		//int beatsRem = _soundtrack.GetBeatsUntilNextSequenceBar();
		//soundtrack has been updated
		_soundtrack.UpdateSoundtrack(_metronome);
		_metronome = (_soundtrack._calibratedPlaybackPosition) % (60f/_bpm);
		//GD.Print(_metronome);
		_delay = (60d / (double)_bpm) - _metronome;
		//GD.Print("delay: " + _delay);
		for(int i = 0; i < _curEventSeqLength; i++)
        {
			int seqItem = _beatSequence[i];
			_curEventSequence.Add(seqItem);
            if(seqItem == 1)
            {
				_metronome = (_soundtrack._calibratedPlaybackPosition) % (60f/_bpm);
				var scene = ResourceLoader.Load<PackedScene>("res://Production/Components/Rhythm/circle_prompt.tscn").Instantiate();
				CirclePrompt b = (CirclePrompt)scene;
				b.CreatePrompt(1, i + 3, (float)_bpm, (60d / (double)_bpm) - _metronome, 0.0f); 
				// Set to invisible for audio calibration
				b.Visible = false; 

				_rhythmUI.AddChild(scene);

				_beatList.Add(b);
            }
        }	
		prevBeat = _soundtrack._curBeat;
	}

	// AKA InitialRunSequenceOld
	public void RhythmCalibration()
	{
		CheckCalibrationSkip();
		var _barLength = _soundtrack._tsTop;
		//GD.Print(_beatIndex);
		// if (_scaleDiff < .004f * _timeCushion *  60f / (float)_bpm || _metronome < .006f * _timeCushion * 60.0f / (float)_bpm)
		// {

		if (prevBeat != _soundtrack._curBeat)
		{
			_onBeat = true;
			//var beats = _soundtrack.GetInitialSequencePosition(_beatIndex % _soundtrack._initialSequence.Count);
			var beatDiff = _soundtrack._curBeat - prevBeat;
			if(beatDiff < 0)
			{
				_beatIndex += _barLength;
			}
			_beatIndex += beatDiff;
			//GD.Print("beat index: " + _beatIndex + " soundtrack beat: " + _soundtrack._curBeat);
			// if(beats > 0)
			// {
			// 	_initialBeatsBetween = beats;
			// }
			int beatIndexModulate = _beatIndex % 4;
			if (beatIndexModulate == 0) // on the on beat
			{
				_beatActualTime = _soundtrack._calibratedPlaybackPosition;
				_rhythmUI.FlashOnBeat();
			}
			else if (beatIndexModulate == 1) // resize circles
			{
				var index = ((_beatIndex - 1) % 12)/4;
				//GD.Print(index);
				_beatList[index].CreatePrompt(1, 10, (float)_bpm, (60f / (float)_bpm) - _metronome, 0.0f);
			}
			else if(beatIndexModulate == 2) // beat window has ended
			{
				ResetBeatsPlayed();
				if(_playersHit > 0)
				{
					_beatsHit++;
					float diff = _beatActualTime - _curDifference/_playersHit;
					// Compensate for loop, change late to make better
					if(Math.Abs(diff) > 1.0f)
					{
						diff += (float)_soundtrack.Stream.GetLength();
					}
					_avgDifference += diff;
					//GD.Print("Beat actual time: " + _beatActualTime);
					//GD.Print("Hit time: " + _curDifference);
					//GD.Print(diff);

					EmitSignal(nameof(DisplayCurrentLatency), diff);

					_curDifference = 0.0f;
					_playersHit = 0;
				}
				if(_beatsHit == 8 && !_audioCalibrationComplete)
				{
					_avgDifference /= 8;
					_audioCalibrationOffset = _avgDifference;
					GD.Print("Audio calibration complete");
					// End audio calibration
					EndAudioCalibration();
				}
				else if(_beatsHit == 16)
				{
					_avgDifference /= 8;
					_calibrationOffset = _avgDifference;
					GD.Print("Rhythm calibration complete");
					// End audio calibration
					EndRhythmCalibration();
					ResetBeatInput();
					return;
				}
			}
		}
		//prevBeat = _soundtrack._curBeat;
		if(_beatIndex < 2) // skip first beat
		{
			return;
		}
		//GD.Print(_beatIndexes[0] + " " + _beatIndex + " Beat Placement: " + _metronome / (60f / (float)_bpm));

		for(int j = 0; j < _playerCount; j++)
		{
			// _beatIndex doesn't matter here
			GatherInputForPlayer(j);
			if (_beatInput[j] == 1 && _currentBeatsPlayed[j] == 0)
			{
				_currentBeatsPlayed[j] = 1;
				_playersHit++;
				//GD.Print("hit");
				_curDifference += _soundtrack._calibratedPlaybackPosition;

				//_rhythmUI.InputPulse();
			}
			// GD.Print("\n\n\n\n\n\n\n\n");
			// GD.Print("Beat index: " + _beatIndex % _beatSequence.Count + ", Player beat index: " + _beatIndexes[0] % _beatSequence.Count + ", sequence number: " + _soundtrack.GetSequenceItemAtIndex(_beatIndex % _beatSequence.Count) + ", beat played: " + _currentBeatsPlayed[0]);
		}

		if(_onBeat && _beatIndex % 4 == 1) // when the beat has officially changed (all beat indexes are on the current beat)
		{
			_onBeat = false;
			//GD.Print("Beat index: " + _beatCheckIndex + "Player beat index: " + _beatIndexes[0]);
		}
		ResetBeatInput();
	}

	public void ResetCalibrationVariables()
	{
		_playersHit = 0; // this is the number of players who hit the beat on a given beat
		_curDifference = 0.0f; // the average difference on a given beat
		_avgDifference = 0.0f; // the average difference on a given beat
	}

	public void EndAudioCalibration()
	{
		_rhythmUI.BeginRhythmCalibration(_bpm);
		// mute click track
		_soundtrack.VolumeDb = -80.0f;
		// set rings visible
		for(int i = 0; i < _beatList.Count; i++)
		{
			_beatList[i].Visible = true;
		}
		_beatsHit = 8;
		_audioCalibrationComplete = true;
		ResetCalibrationVariables();
		EmitSignal(nameof(OnAudioCalibrationComplete));
		GD.Print("Audio Calibration: " + _audioCalibrationOffset);
	}

	public void EndRhythmCalibration()
	{
		_rhythmCalibrationComplete = true;
		_rhythmUI.EndRhythmCalibration();
	
		// stop playing click track
		_soundtrack.VolumeDb = 3.0f;
		_soundtrack.Stop();
		//_soundtrack = _soundtrackCollection.GetSoundtrack(Tracknames.LAPLANDS_FIKA_DELIVERY_DOGS);
		EndSequence();
		SeqRun -= RhythmCalibration;
		
        // Save calibration settings
        _jsonSettings.Rhythm.AudioCalibration = _audioCalibrationOffset;
		_jsonSettings.Rhythm.VideoCalibration = _calibrationOffset;
        Godot.FileAccess file = Godot.FileAccess.Open(_settingsPath, Godot.FileAccess.ModeFlags.Write);
        string jStr = JsonSerializer.Serialize<JSONGameSettings>(_jsonSettings);
        file.StoreString(jStr);
        file.Close();

		EmitSignal(nameof(OnRhythmCalibrationComplete));
		GD.Print("Rhythm Calibration: " + _calibrationOffset);
	}

	public void EndInitialEvent(){
		//Success scenario
		EndSequence();
		_rhythmUI.EndInitialEvent();
		inputSystem.AddActiveInputMode(EInputSystemMode.PLAYER_MOVEMENT);
		//start timer
		EmptyBeatList();
		SeqRun -= InitialRunSequence;
		SeqRun += RunSequence2; 
		Hike.Invoke();
	}

	public void GetCollectables() {
        _collectables = new List<Collectable>();

		if (GameManager.GetInstance().CurrentLevel.Collectables is null) {
			return;
		}

        var collectables = GameManager.GetInstance().CurrentLevel.Collectables.GetChildren();
		
		foreach (Node3D collectable in collectables) {
			_collectables.Add(collectable as Collectable);
        }
	}

	public override void _Ready(){
		// Load saved rhythm calibration
		_settingsPath = OS.GetUserDataDir() + _settingsPath;
        if (Godot.FileAccess.FileExists(_settingsPath))
        {
            string str = FileHandler.ReadJsonFile(_settingsPath);
			_jsonSettings = JsonSerializer.Deserialize<JSONGameSettings>(str);
			_audioCalibrationOffset = _jsonSettings.Rhythm.AudioCalibration;
			_calibrationOffset = _jsonSettings.Rhythm.VideoCalibration;
        }

		_controllers = new Godot.Collections.Array<InputSystemController>();
		inputSystem = InputSystem.GetInstance();
		_timeManager = BasicTimeManager.GetInstance();
		inputSystem.OnControllerActivate += ActivateController;
		inputSystem.OnControllerDeactivate += DeactivateController;
		inputSystem.OnControllerActivationAllowed += OnControllersActivationsAllowed;
		GameManager.GetInstance().OnLevelLoaded += SetReferences;
		GameManager.GetInstance().OnBeforeLevelDelete += OnBeforeLevelDelete;

		var scene = ResourceLoader.Load<PackedScene>("res://Production/Components/Rhythm/powerup_gauge.tscn").Instantiate();
		PowerupGauge b = (PowerupGauge)scene;
		AddChild(scene);
		_gauge = b;
		_powerUpQueue = new List<ActiveEffect>();
		SeqGo += SequenceGo2; 

		_bpm = _soundtrack.bpm;
		_curPowerup = GetNode<Powerup>("Powerups/Invincibility"); 
	}

	public override void _Process(double delta){
        // if(Input.IsActionJustPressed("space")){
        // 	_curPowerup.PowerEffect(_dogs, _sled, _playerCount, 8f);
        // 	_powerupDuration = _curPowerup._duration;
        // }
		if(!_paused && !_levelEnded)
		{        // Debug Mode
			if (GameManager.GetInstance().DebugMode && !levelManager.IsSessionRunning()) {
				EndInitialEvent();
			}

			// var triggerEvent = false;
			// for(int i = 0; i < _controllers.Count; i++){
			// 	if(_controllers[i].IsJustPressed("trigger")){
			// 		triggerEvent = true;
			// 	}
			// 	else if(_controllers[i].IsJustPressed("mute")){
			// 		_muted = !_muted;
			// 		if(_muted){
			// 			_soundtrack.VolumeDb = -80f;
			// 		}
			// 		else{
			// 			_soundtrack.VolumeDb = 0.0f;
			// 		}
			// 	}
			// }
			//_metronome = (_metronome + (float)delta) % (60f/(float)_bpm);
			_soundtrack.UpdateSoundtrack(_metronome);
			_metronome = (_soundtrack._calibratedPlaybackPosition) % (60f/_bpm);
			float _metronomeDelta = (float)_soundtrack._metDelta;
			UIPulse();
			if(_gauge.Value >= 100 && !_triggerEvent) // if rhythm event is triggerable
			{
				BarkToTriggerRhythmEvent();
			}

			if (!_sequenceGo && _triggerEvent && _gauge.Value >= 100)
			{
				StartRhythmEvent();
				EmitSignal(nameof(OnRhythmEventStart));
			}
			if(_sequenceGo){
				if(_delay > 0.0d)
				{
					_delay = Math.Max(0.0d, _delay - _soundtrack._metDelta);
				}
				//GD.Print(_delay);
				SeqRun?.Invoke();
			}
			else
			{
				_rhythmUI.FadeUI(_metronomeDelta);
			}
			prevBeat = _soundtrack._curBeat;
			UpdateEffectTimes((float)delta);
			_rhythmUI.UpdateUI(_metronomeDelta);

			// update the circle prompts
			for(int i = 0; i < _beatList.Count; i++)
			{
				_beatList[i].UpdateButton(_metronomeDelta);
			}
		}
	}

	public void TogglePause()
	{
		_paused = !_paused;
		if(_paused)
		{
			_soundtrack.Stop();
			foreach(ActiveEffect pu in _powerUpQueue)
			{
				pu._pow.PauseSFX(true);
			}
		}
		else
		{
			_soundtrack.Play(_soundtrack._curPlaybackPosition);
			foreach(ActiveEffect pu in _powerUpQueue)
			{
				pu._pow.PauseSFX(false);
			}
		}
	}

	public void Pause()
	{
		_paused = true;
		_soundtrack.Stop();
		foreach(ActiveEffect pu in _powerUpQueue)
		{
			pu._pow.PauseSFX(true);
		}
	}

	public void Unpause()
	{
		_paused = false;
		_soundtrack.Play(_soundtrack._curPlaybackPosition);
		foreach(ActiveEffect pu in _powerUpQueue)
		{
			pu._pow.PauseSFX(false);
		}
	}
	
	public void UIPulse()
	{
		if(prevBeat != _soundtrack._curBeat && ((_soundtrack._curBeat - 1) % _soundtrack._tsTop) % _soundtrack._onBeat == 0) // change this for varying time signatures
		{
			_rhythmUI.Pulse();
			BeatPulse?.Invoke();
		}
	}

	public void StartRhythmEvent()
	{
		_timeManager.MusicState();
		_curPowerup = (Powerup)GetNode<Node>("Powerups").GetChild(_gauge._puType);
		inputSystem.AddActiveInputMode(EInputSystemMode.RYTHM_EVENT);
		SeqGo.Invoke();
	}

	public void UpdateEffectTimes(float delta)
	{
		for (int i = 0; i < _powerUpQueue.Count; i++)
		{
			if(_powerUpQueue[i] != null)
            {
				_powerUpQueue[i]._dur = Math.Max(_powerUpQueue[i]._dur - delta, 0.0f);
				if (_powerUpQueue[i]._dur == 0)
				{
					bool secondaryPUStillActive = false;
					foreach(ActiveEffect pu in _powerUpQueue)
					{
						if(pu != _powerUpQueue[i] && pu._pow.GetType() == _powerUpQueue[i]._pow.GetType())
						{
							// Do not end a powerup when there is another of the same type still active
							secondaryPUStillActive = true;
							break;
						}
					}
					if(!_powerUpQueue[i]._initialBoost)
					{
						EmitSignal(nameof(OnPowerupEnded), i);
					}
					if (_dogs != null && !secondaryPUStillActive)
					{
						_powerUpQueue[i]._pow.EndEffect(_dogs, _sled, _dogs.Count);
					}
					_powerUpQueue.RemoveAt(i);
				}
            }
		}
	}

	public void EndAllPowerups(){
		for(int i = 0; i < _powerUpQueue.Count; i++){
			if(_dogs != null && _sled != null){
				_powerUpQueue[i]._pow.EndEffect(_dogs, _sled, _dogs.Count);
			}
			_powerUpQueue.Remove(_powerUpQueue[i]);
		}		
		_powerUpQueue = new List<ActiveEffect>();
	}

	public void EndSequence(){
		_triggerEvent = false;
		_rhythmUI.EndSequence();
		_sequenceGo = false;
		for (int i = 0; i < _beatList.Count; i++)
		{
			_beatList[i].Free();
		}
		ResetBeatIndexes();
		ResetBeatInput();
		ResetBeatsPlayed();
		_curEventSequence.Clear();
		_beatAccuracySequence.Clear();
		_feedbackReady.Clear();
		_curEventSeqBeatCount = 0;
		_curEventSeqLength = 0;
		_beatList = new List<ButtonPrompt>(0);
		_beatIndex = 0;
		_beatCombo = 0;
		_nextBeat = false;
		_gauge.Value = 0;
		_delay = 0.0d;
		inputSystem.RemoveActiveInputMode(EInputSystemMode.RYTHM_EVENT);
		//Change lm state to RUNNING
		_timeManager.GameState();
	}

	public void GatherInput()
	{
		//Sprite2D circ = _center.GetNode<Sprite2D>("CircleFilled");

		for (int i = 0; i < _playerCount; i++)
		{
			if (_controllers[i].IsJustPressed("bark"))
			{
				_beatInput[i]++;
				_rhythmUI.InputPulse();
			}
		}
	}

	public void GatherInputForPlayer(int _player)
	{
		if (_controllers[_player].IsJustPressed("bark"))
		{
			_beatInput[_player]++;
			_rhythmUI.InputPulse();
		}
	}

	public void CheckCalibrationSkip()
	{
		for(int i = 0; i < _playerCount; i++)
		{
			if(_controllers[i].IsJustPressed("Y"))
			{
				if(!_audioCalibrationComplete)
				{
					ResetAudioCalibration();
					EndAudioCalibration();
					break;
				}
				else if(!_rhythmCalibrationComplete)
				{
					ResetCalibrationOffset();
					EndRhythmCalibration();
					break;
				}
			}
		}
	}

	public void RunSequence2(){
		if(_delay > (60f / (float)_bpm)) 
		{
			return;
		}

		RhythmEventProcessInput();
		
		if(_beatIndex >= _curEventSeqLength && _beatIndex > 0){
			//Success scenario
			float accuracy = _accuracy/(_curEventSeqBeatCount * _playerCount);
			_curPowerup.PowerEffect(_dogs, _sled, _dogs.Count, accuracy);
			_powerUpQueue.Add(new ActiveEffect(_curPowerup, _curPowerup._duration));
			EmitSignal(nameof(OnPowerupActivate), _curPowerup._duration, _curPowerup._maxDuration);
			if(_accuracy > 0.0f){
				_eventFeedbackSound.PlayFeedbackSound(accuracy, _curPowerup._numOfLevels);
			}
			//GD.Print("beat index: " + _beatIndex + ", sequence length: " + _curEventSeqLength);
			EndSequence();
		}
	}

	public void RhythmEventProcessInput()
	{		
		var beatDiff = _soundtrack._curBeat - prevBeat;
		if(beatDiff < -1)
		{
			beatDiff += _barLength;
		}
		if (beatDiff > 0)
		{
			_barLength = _soundtrack._tsTop;
			_onBeat = true;
			if(_delay == 0.0d) 
			{
				_beatIndex += beatDiff;
				GD.Print("Beat difference: " + beatDiff);
				// Hardcoded solution for if _beatIndex gets ahead, but as long as beatDiff > 0, it shouldn't happen?
				if(_beatIndex % _barLength == _soundtrack._curBeat % _barLength)
				{
					_beatIndex--;
				}
				if(_beatIndex < _curEventSeqLength && _curEventSequence[_beatIndex] == 1)
				{
					//flash
					_rhythmUI.FlashOnBeat();
				}
			}
			GD.Print("beat index: " + _beatIndex + " current beat: " + _soundtrack._curBeat);
			if(_beatIndex == 3 && _soundtrack._curBeat != 4)
			{
				GD.PrintErr("RHYTHM SEQUENCE OFF: BEAT INDEX AHEAD BY " + (1 + _beatIndex - _soundtrack._curBeat) + " BEAT(S)");
			}
		}

		var _prevAccuracy = _accuracy;
		for(int j = 0; j < _playerCount; j++)
		{
			var _curIndex = _beatIndexes[j];
		
			// If beat is current beat but on the back end of the beat (after _beatIndex increases) 
			// if(metronome < t && _beatIndex > _beatIndexes[j])
			// 		if two beats are back to back, only the first one will count as played

			// If beat is next beat but at the front of the beat (before _beatIndex increases)
			// if(metronome > t && _beatIndex == _beatIndexes[j])
			// 		if two beats are back to back, only the first one will count as played

			// If beat is played correctly
			GatherInputForPlayer(j);
			if (_curIndex < _curEventSeqLength && _curEventSequence[_curIndex] == 1)
			{
				if (_beatInput[j] == 1 && _currentBeatsPlayed[j] == 0)
				{
					_currentBeatsPlayed[j] = 1;
					//SetIndividualFeedbackText(j, 1);
					_accuracy++;
					_beatAccuracySequence[_curIndex]++;
					_feedbackReady[_curIndex]++;
					ShowFeedback(_curIndex);
				}

				if (_beatIndexes[j] <= _beatIndex && _currentBeatsPlayed[j] == 1 && _metronome < (60f / (float)_bpm) / 2f) //Prior to beat change
				{
					_beatIndexes[j]++;
					_currentBeatsPlayed[j] = 0;
					//GD.Print("beat changed before beat " + _beatIndex);
				}
				else if (_beatIndexes[j] < _beatIndex && _currentBeatsPlayed[j] == 0 && _metronome > (60f / (float)_bpm) / 2f) //After beat change
				{
					_beatIndexes[j]++;
					_currentBeatsPlayed[j] = 0;
					//GD.Print("beat changed after beat " + _beatIndex);
					_feedbackReady[_curIndex]++;
					ShowFeedback(_curIndex);
					//SetIndividualFeedbackText(j, 0);
				}
		
			}
			else //If it is an empty beat
			{
				//Beat index changes prior to end of metronome
				if(_beatIndexes[j] <= _beatIndex && _metronome > (60f / (float)_bpm) / 2f)
				{
					_beatIndexes[j]++;
					//_currentBeatsPlayed[j] = 0;
					//GD.Print("rest beat changed before beat " + _beatIndex);
				}
				if (_curIndex >= _sequenceStartIndex && _beatInput[j] == 1)
				{
					_currentBeatsPlayed[j] = 1;
					// Beat played on an empty beat (too early)
					//SetIndividualFeedbackText(j, -1);
				}
			}
			
			//GD.Print("Beat index: " + _beatIndex + ", Player beat index: " + _beatIndexes[0] + ", sequence number: " + _soundtrack.GetSequenceItemAtIndex(_beatIndex) + ", beat played: " + _currentBeatsPlayed[0]);
		}
		ResetBeatInput();
		if(_prevAccuracy != _accuracy)
		{
			_rhythmUI.UpdateAccuracyMeter(100f * _accuracy / ((float)_curEventSeqBeatCount*_playerCount));
		}
	}

	public void SequenceGo2(){
		_sequenceStartIndex = 0;
		_beatIndex = 0;
		_beatCombo = 0;
		_accuracy = 0.0f;
		_beatAccuracy = 0.0f;
		ResetBeatsPlayed();
		ResetBeatInput();
		ResetBeatIndexes();
		int beatsRem = _soundtrack.GetBeatsUntilNextSequenceBar();
		_metronome = (_soundtrack._calibratedPlaybackPosition) % (60f/_bpm);
		_delay = (beatsRem * (60d / (double)_bpm)) + Math.Abs((60d / (double)_bpm) - _metronome);
		
		_curEventSeqLength = _soundtrack.GetSequenceLength();
		_curEventSeqBeatCount = _soundtrack.GetSequenceBeatCount();
		_beatList = new List<ButtonPrompt>(_curEventSeqLength);
		_beatAccuracySequence = new List<int>(_curEventSeqLength);
		_feedbackReady = new List<int>(_curEventSeqLength);
		_barLength = _soundtrack._tsTop;

		for(int i = 0; i < _curEventSeqLength; i++)
        {
			int seqItem = _soundtrack.GetSequenceItemAtIndex(i);
			_curEventSequence.Add(seqItem);
			_beatAccuracySequence.Add(0);
			_feedbackReady.Add(0);
            if(seqItem == 1)
            {
				var scene = ResourceLoader.Load<PackedScene>("res://Production/Components/Rhythm/circle_prompt.tscn").Instantiate();
				CirclePrompt b = (CirclePrompt)scene;
				b.CreatePrompt(1, beatsRem + i, (float)_bpm, (60d / (double)_bpm) - _metronome, _calibrationOffset);
				//b = new ButtonPrompt(_curPowerup.sequence.Dequeue(), i+1, _center.Position);
				_rhythmUI.AddChild(scene);

				_beatList.Add(b);
            }
        }	
		_rhythmUI.SequenceGo((int)_curPowerup._numOfLevels);

		_sequenceGo = true;
		prevBeat = _soundtrack._curBeat;
	}

	//Checks if all player input is on the beat and played correctly
	// public bool CheckButtons(int nextButton){
	// 	if(_beatInput.Length > _playerCount || _beatInput.Length < _playerCount){
	// 		return false;
	// 	}
	// 	for(int i = 0; i < _playerCount; i++){
	// 		if(_beatInput[i] != nextButton){
	// 			return false;
	// 		}
	// 	}
	// 	return true;
	// }

	// public bool CheckEmptyInput()
	// {
	// 	if (_beatInput.Length > 0)
	// 	{
	// 		return false;
	// 	}
	// 	return true;
	// }

	// public bool CheckButtonsIgnoreSymbol()
	// {
	// 	if (_beatInput.Length > _playerCount || _beatInput.Length < _playerCount)
	// 	{
	// 		return false;
	// 	}
	// 	return true;
	// }	

	// public float CheckButtonsAccuracy(){
	// 	var _inputCount = GetNumberOfInputs();
	// 	if(_inputCount > _playerCount){
	// 		_inputCount = _playerCount + (_playerCount - _inputCount);
	// 	}
	// 	return (float)_inputCount / (float)_playerCount;
	// }

	// public float CheckAccuracy(){
	// 	var _inputCount = GetNumberOfInputs();
	// 	if(_inputCount > _playerCount){
	// 		_inputCount = _playerCount + (_playerCount - _inputCount);
	// 	}
	// 	return (float)_inputCount / (float)_playerCount;
	// }

	// public string GetSymbol(int sym){
	// 	switch(sym){
	// 		case 1 : return "B"; break;
	// 		case 2 : return "A"; break;
	// 		case 3 : return "X"; break;
	// 		case 4 : return "Y"; break;
	// 		default : return ""; break;
	// 	}
	// }

	private void OnCollectableEntered(int puType, Godot.Color col, double val){
		if(_gauge.Value == 0){
			_gauge._puType = puType;
			_gauge.Modulate = col;
			_gauge.Value += val;
			_rhythmUI.CollectablePickedUp();
			_beatIndex = _soundtrack._curBeat - 1;
			EmitSignal(nameof(OnPowerupCollected));
		}
		else if(_gauge._puType == puType){
			_gauge.Value += val;
		}
	}

	public void CreateFeedbackLabels(){
		foreach(Control con in _fbLabels){
			con.Free();
		}
		_fbLabels = new Control[_playerCount];
		for(int i = 0; i < _playerCount; i++){
			var scene = ResourceLoader.Load<PackedScene>("res://Production/Components/Rhythm/RhythmPlayerFeedback.tscn").Instantiate();
			Control fb = (Control)scene;
			AddChild(scene);
			_fbLabels[i] = fb;
			var section = (i*Math.PI/_playerCount) + Math.PI/(float)(2*_playerCount) - Math.PI;
			var m = 150f;
			fb.Position = _rhythmUI.GetCenterPosition() + new Vector2(m*(float)Math.Cos(section), m*(float)Math.Sin(section));
			FeedbackText label = fb.GetChild<FeedbackText>(-1);
			label.Modulate = UIConstants.PLAYER_COLORS[i];
			label.LabelSettings.FontSize = 32;
			label.SetInvisible();
		}
	}

	public void SetFeedbackTextInvisible(){
		for(int i = 0; i < _playerCount; i++){
			FeedbackText label = _fbLabels[i].GetChild<FeedbackText>(-1);
			label.SetInvisible();
		}
	}

	public void SetIndividualFeedbackText(int player, int message){
		FeedbackText label = _fbLabels[player].GetChild<FeedbackText>(-1);
		if(_beatInput[player] == Math.Abs(message)){
			switch(message){
				case -1 : label.Text = "Too early!"; break;
				case 0 : label.Text = "Miss!"; break;

				case 1 : 
					label.Text = "Perfect!"; 
					break;
				default : break;
			}
			//label.SetVisible();
		}
	}

	public void ResetBeatInput(){
		_beatInput = new int[_playerCount];
		for(int i = 0; i < _playerCount; i++){
		 	_beatInput[i] = 0;
		}
	}
	public void ResetBeatIndexes(){
		_beatIndexes = new int[_playerCount];
		for(int i = 0; i < _playerCount; i++){
		 	_beatIndexes[i] = 0;
		}
	}

	public void ResetBeatsPlayed()
    {
		_currentBeatsPlayed = new int[_playerCount];
		for(int i = 0; i < _playerCount; i++)
        {
			_currentBeatsPlayed[i] = 0;
        }
    }

	public int GetNumberOfInputs(){
		int sum = 0;
		for(int i = 0; i < _playerCount; i++){
			sum += _beatInput[i];
		}
		return sum;
	}

	public void OnGoalReached(){
		_rhythmUI.LevelEnd();
		_levelEnded = true;
		_soundtrack.Stop();
		_goalReachedMusic.Play();
	}

	public void PlayMainMenuMusic(bool atLastPlaybackPos)
	{
		_soundtrack.Stop();
		_soundtrack = _soundtrackCollection.GetSoundtrack(Tracknames.LAPLANDS_FIKA_DELIVERY_DOGS);
		_bpm = _soundtrack.bpm;
		if(atLastPlaybackPos)
		{
			_soundtrack.Play(_soundtrack._curPlaybackPosition);			
		}
		else
		{
			_soundtrack.Play();
		}
	}

	public void ResetAudioCalibration()
	{
		_audioCalibrationOffset = 0.0f;
	}

	public void ResetCalibrationOffset()
	{
		_calibrationOffset = 0.0f;
	}

	public void ShowFeedback(int sequenceIndex)
	{
		if(_feedbackReady[sequenceIndex] == _playerCount)
		{
			//GD.Print("Beat Accuracy: " + (float)_beatAccuracySequence[sequenceIndex]/(float)_playerCount);
			int accuracyLevel = (int)Math.Floor((float)_beatAccuracySequence[sequenceIndex]/((float)_playerCount/4f));
			_rhythmUI.ShowCollectiveFeedback(accuracyLevel);
			//SetFeedback(_beatAccuracy/_playerCount);
		}
	}
}
