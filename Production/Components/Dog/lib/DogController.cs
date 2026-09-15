using Godot;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;

public partial class DogController : BaseCharacter
{

    public Camera3D _camera;
    [Export] public BarkManager _barkEffect;
    [Export] public ConfusionHalo _confusionHalo;

    [ExportCategory("SFX")]
    [Export] public AudioStream _runningSound;
    [Export] public AudioStreamPlayer3D _runningAudio;
    [Export] public AudioStreamPlayer3D _landingAudio;
    [Export] public AudioStreamPlayer3D _crunch;
    [Export] public Bark _bark;
    // Controller variables 
    public int Id;
    public int _characterId;
    public InputSystemController _inputController;
    private bool _hasLanded;
    private const string DOG_MODEL_METATAG = "dogModel";

    // TODO: Remove confusion and smoosh from dog controller, make it separated functionality
    public bool _confused = false;
    public float _confusionTimer = 0.0f;

    [ExportCategory("Dog Smoosh")]
    [Export] MeshInstance3D _dogMesh; // the parrent of all dog parts
    [Export] Node3D _dogModelMeshParrent; // the parrent of all dog parts
    [Export] Curve dogUnSmooshCurve;
    [Export] float _smooshFactor = 0.01f; // How much to smoosh the dog
    [Export] float _timeSpendSmooshed = 5f; // How long dog is smooshed
    float _restoreSizeState = 0f;
    [Export] float _unsmooshDuration = 1f;
    private float _smooshTimer = 0f;
    private bool _isSmooshed = false;
    private float _originalYScale;
    public bool IsDogSlowedByMud = false;

    public void SetCamRef(Camera3D cam)
    {
        _camera = cam;
    }

    public void PlayRunningAudio()
    {
        if (!InAir() && _inputVector.Length() > 0.1f)
        {
            if(!_runningAudio.IsPlaying())
                _runningAudio.Play();
        }
        else
        {
            if(_runningAudio.IsPlaying())
                _runningAudio.Stop();
        }
    }

    public void PlayLandingAudio()
    {
        if (!InAir())
        {
            if (!_hasLanded)
            {
                _landingAudio.Play();
                _hasLanded = true;
            }
        }
        else
        {
            _hasLanded = false;
        }
    }

    public override void _Ready()
    {
        base._Ready();
		GD.Randomize();
        setupDogMaterials();
        _dogMesh.MaterialOverride = UIConstants.CHARACTER_SKINS[_characterId];
        _bark = GetNode<Bark>("Bark");
        _bark?.SetBarkList(_characterId);

        if (_dogModelMeshParrent != null)
            _originalYScale = _dogModelMeshParrent.Scale.Y;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        HandleInput();
        PlayRunningAudio();
        PlayLandingAudio();
        UpdateConfusionTime((float)delta);
        RestoreDogFromSmoosh((float)delta);
    }

    private void RestoreDogFromSmoosh(float delta)
    {
        if (!_isSmooshed) return;

        _smooshTimer -= delta;

        if (_smooshTimer <= 0f)
        {
            _restoreSizeState += delta / _unsmooshDuration;
            _restoreSizeState = Mathf.Clamp(_restoreSizeState, 0, 1f);

            float curveValue = dogUnSmooshCurve.Sample(_restoreSizeState);
            float _targetY = Mathf.Lerp(_smooshFactor, _originalYScale, curveValue);

            Vector3 currentScale = _dogModelMeshParrent.Scale;
            currentScale.Y = _targetY;
            _dogModelMeshParrent.Scale = currentScale;

            // Make sure we have the real scale and reset speed after being back to normal
            if (currentScale.Y >= _originalYScale)
            {
                _dogModelMeshParrent.Scale = new Vector3(_dogModelMeshParrent.Scale.X, _originalYScale, _dogModelMeshParrent.Scale.Z);
                _isSmooshed = false;
                _restoreSizeState = 0;
                CurrentSpeedBoost *= 2;
            }
        }
    }

    public void SmoshDog()
    {
        Vector3 scale = _dogModelMeshParrent.Scale;
        scale.Y =  _smooshFactor;
        _dogModelMeshParrent.Scale = scale;

        CurrentSpeedBoost /= 2;
        _smooshTimer = _timeSpendSmooshed;
        _isSmooshed = true;
    }

    private void modifyDogMaterial(Node modelPart, float alpha, bool isXray)
    {
        if (modelPart is MeshInstance3D bodyPart)
        {
            Material currentMat = bodyPart.Mesh.SurfaceGetMaterial(0);

            if (currentMat is StandardMaterial3D stdMat)
            {
                StandardMaterial3D newMat = (StandardMaterial3D)stdMat.Duplicate();
                // Temporary solution to make dog colors more subtle which is why it's hard-coded for now until we have
                // more standard coloring options in the singleton
                Color temp = UIConstants.PLAYER_COLORS[Id];
                temp.A = alpha;

                newMat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
                newMat.AlbedoColor = temp;

                if (isXray)
                {
                    // We might wanna display collectibles before our dog's xray skin:
                    newMat.RenderPriority = 20;
                    newMat.NoDepthTest = true;
                }

                bodyPart.SetSurfaceOverrideMaterial(0, newMat);
            }
        }
    }

    // Give each dog unique color
    private void setupDogMaterials()
    {
        Godot.Collections.Array<Node> modelParts = _dogModelMeshParrent.GetChildren();
        for (int j = 0; j < modelParts.Count; j++)
        {
            if (modelParts[j].GetChildCount() > 0)
            {
                modifyDogMaterial(modelParts[j].GetChild(0), 0.1f, true);
                modifyDogMaterial(modelParts[j], 1f, false);
            }
        }
    }

    //------------------------------------------
    // Input Handling
    //------------------------------------------
    public void HandleInput()
    {
        if (_inputController == null)
        {
            return;
        }

        if (_inputController.IsPressed("jump"))
        {
            HandleJump();
        }

        if (_inputController.IsJustPressed("bark"))
        {
            _bark.OnBark();
            _barkEffect.Bark(Id);
            GameManager.GetInstance().CurrentLevel.DogBark(GlobalPosition);
        }

        float inputVectorX = _inputController.GetInputAxis("left", "right");
        float inputVectorY = _inputController.GetInputAxis("back", "front");
        if (_confused)
        {
            inputVectorX *= -1;
            inputVectorY *= -1;
        }

        if (inputVectorX != 0 || inputVectorY != 0)
        {
            HandleMoving(inputVectorX, inputVectorY);
        }
        else
        {
            _inputVector = Vector3.Zero;
        }
    }

    public void SetInputController(InputSystemController controller, int id)
    {
        _inputController = controller;
        Id = id;
    }

    private void HandleMoving(float inputVectorX, float inputVectorY)
    {
        _inputVector = new Vector3(inputVectorX, 0, inputVectorY);
        if (_inputVector.Length() < 0.05f)
        {
            _inputVector = Vector3.Zero;
            return;
        }
        _inputVector = RelativeCamera3D(_inputVector);
    }

    //-----------------------------------------------
    // Relative vector offset functions
    //-----------------------------------------------
    private Vector2 RelativeCameraFlat(Vector2 inputVector)
    {
        Vector2 cameraForward = new Vector2(-_camera.GlobalBasis.Z.X, -_camera.GlobalBasis.Z.Z).Normalized();
        return inputVector.Rotated(cameraForward.Angle() + Mathf.Pi / 2);
    }
    private Vector3 RelativeCamera3D(Vector3 inputVector)
    {
        Vector2 adjusted2D = RelativeCameraFlat(new Vector2(inputVector.X, inputVector.Z));
        return new Vector3(adjusted2D.X, inputVector.Y, adjusted2D.Y);
    }

    //------------------------------------------
    // Public API
    //------------------------------------------

    public int GetId() { return Id; }

    public void ConfuseDog()
    {
        _confusionHalo.Visible = true;
        _confusionHalo.ProcessMode = ProcessModeEnum.Inherit;
        _confused = true;
        _confusionTimer = 5.0f;
        _trapAudio.Stream = ConfusionSoundsList.CONFUSION_SOUNDS[GD.Randi() % ConfusionSoundsList.CONFUSION_SOUNDS.Length]; 
        _trapAudio.Play();
    }

    public void UpdateConfusionTime(float delta)
    {
        if (_confused)
        {
            _confusionTimer -= delta;
            if (_confusionTimer <= 0)
            {
                _confusionHalo.Visible = false;
                _confusionHalo.ProcessMode = ProcessModeEnum.Disabled;
                _confusionTimer = 0.0f;
                _confused = false;
            }            
        }
    }
}
