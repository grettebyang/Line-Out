using Godot;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

public partial class Camera : Node3D {
    [Export] Node3D _followPoint, _leftRightPivot, _upDownPivot, _minPos,_maxPos, _lookTarget, _helper, _sledPivot;
    [Export] public Camera3D _sledCamera;
    [Export] DogSled _sled;
    [Export] public ISDogSpawner dogSpawnerReference;
    [Export] Curve _backMovement, _rotationLerp, _behindLerp;
    [Export] private float cameraAnimationDistanceBack = 30f;
    [Export] private float cameraAnimationDistanceUp = 20f;
    float FurtherestBackDot = 1;
    float[] _behindTimer = new float[4];
    float _lerpFactor1 = 2, _lerpFactor2 = 2;
    Node3D FurtherestBack;

    [ExportGroup("Camera Shake")]
    [Export] private float _shakeMultiplayer = 10;
    [Export] private float _shakeSpeed = 2;
	[Export] private float _shakeDecrease = 2;
    [Export] private float _camShakeCap = 15;

    private float _camShake = 0;
    private Vector3 _originalRotation;

    private bool PositionStored = false;
    private bool CanShake = true;
    public bool CameraShakeRequest = false;
    private bool IsShaking = false;
    private Vector3 _originalPosition;
    private bool isADogBehindSled = false;

    public override void _Ready()
    {
        GlobalPosition = _sledPivot.GlobalPosition;
        FurtherestBack = _sledPivot;
        _helper.Quaternion = _sled.Quaternion;
        _originalRotation = Rotation;
    }

    public void ActivateCamShake(float target)
    {
        _camShake = Mathf.Clamp(target, 0, _camShakeCap);
	}

    private void cameraShake(double delta)
    {
        //GD.Print("_camShake: " + _camShake);

        if (_camShake > 0)
        {
            float shakeRand = (float)GD.RandRange(-_camShake, _camShake);
            shakeRand = Mathf.DegToRad(shakeRand);
            Vector3 shakeTarget = new Vector3(_sledCamera.GlobalRotation.X, _sledCamera.GlobalRotation.Y, shakeRand * _shakeMultiplayer);
            _sledCamera.GlobalRotation = _sledCamera.GlobalRotation.Slerp(shakeTarget, (float)delta * _shakeSpeed);

            _camShake -= (float)delta * _shakeDecrease;
        }
    }

    // Used for when sled respawns, maybe we want to delay sled movement a bit before this finishes?
    public void respawnCameraAnimation() {
        Vector3 offset = _sled.Basis.Z * -cameraAnimationDistanceBack + (_sled.Basis.Y * cameraAnimationDistanceUp);
        _sledCamera.GlobalPosition = _sled.GlobalPosition + offset; 
        float _desiredFacingDirection = _sledCamera.GlobalPosition.AngleTo(_sled.GlobalPosition);
        _sledCamera.Rotation = new Vector3(0, _desiredFacingDirection, 0);
    }

    Vector2 GetRotationInput(DogController dog)
    {
        if (dog._inputController == null)
        {
            dog.isHoldingRightJoystick = false;
            return Vector2.Zero;
        }

        float inputVectorX = dog._inputController.GetInputAxis("r_left", "r_right");
        float inputVectorY = dog._inputController.GetInputAxis("r_front", "r_back");

        if(inputVectorX == 0 && inputVectorY == 0)
        {
            dog.isHoldingRightJoystick = false;
            return Vector2.Zero;
        }

        foreach (DogController compareDog in dogSpawnerReference.DogList)
        {
            if(dog != compareDog)
            {
                if(compareDog.isHoldingRightJoystick)
                {
                    dog.isHoldingRightJoystick = false;
                    return Vector2.Zero;
                }
            }
        }

        dog.isHoldingRightJoystick = true;

        Vector2 right_joystick_input = new Vector2(inputVectorX, inputVectorY);
        return right_joystick_input;
    }

    public override void _PhysicsProcess(double delta)
    {
        TrackDogs(delta);

        if(isADogBehindSled)
            GlobalPosition = GlobalPosition.Slerp(_sledPivot.GlobalPosition + Vector3.Up * 3 - _sled.GlobalBasis.Z * 6f, (float)delta * _lerpFactor1);
        if(!isADogBehindSled)
            GlobalPosition = GlobalPosition.Slerp(_sledPivot.GlobalPosition + new Vector3(0, 3, 0), (float)delta * _lerpFactor1);

        //if the slde makes less than half a rotation in a second
        //then lerp the cameras LR rotation pivot to match it's rotation
        //GD.Print(_sled.AngularVelocity.Length());
        //if(_sled.AngularVelocity.Length() < MathF.PI * 1.5){
        // GD.Print("total",_sled.AngularVelocity.Length());
        // GD.Print("total",_sled.AngularVelocity.Y);
        _helper.GlobalRotation = new Vector3(_sledPivot.GlobalRotation.X, _sledPivot.GlobalRotation.Y, 0);
        
        _leftRightPivot.Quaternion = _leftRightPivot.Quaternion.Slerp(_helper.Quaternion, _rotationLerp.Sample(_sled.AngularVelocity.Length()) * (float)delta);
        //}
        _helper.GlobalPosition = _minPos.GlobalPosition.Lerp(_maxPos.GlobalPosition, _backMovement.Sample(FurtherestBackDot));
        if (FurtherestBackDot < 1)
            _sledCamera.GlobalPosition = _sledCamera.GlobalPosition.Lerp(_helper.GlobalPosition, (float)delta * 1);
        else
            _sledCamera.GlobalPosition = _sledCamera.GlobalPosition.Lerp(_helper.GlobalPosition, (float)delta * 2);
            
        _sledCamera.LookAt(_lookTarget.GlobalPosition);
        // GD.Print(FurtherestBackDot);
        
        // run camera shake in each frame, but is only be applied if we request a camera shake with boolean:
        cameraShake(delta);
    }

    private void IsDogBehindSled(DogController dog)
    {
        Vector3 toDog = dog.GlobalPosition - _sled.GlobalPosition;

        if (_sled.GlobalBasis.Z.Dot(toDog) < 0 && toDog.Length() > 5)
            isADogBehindSled = true;
    }
    
    void TrackDogs(double delta) {
        Vector3 DesiredLookAt = new Vector3();
        
        int counter = 0;
        FurtherestBack = _sledPivot;
        FurtherestBackDot = 1;
        isADogBehindSled = false;
        Vector3 targetOffset = new Vector3();

            foreach (DogController dog in dogSpawnerReference.DogList) {
                if (dog != null) {
                    float DogAngle = ( dog.GlobalPosition - _sledPivot.GlobalPosition ).Dot(_sled.GlobalBasis.Z);
                    //GD.Print(DogAngle);
                    if (DogAngle < FurtherestBackDot) {
                        FurtherestBackDot = DogAngle;
                        FurtherestBack = dog;
                    }
                    if (DogAngle < 0.1) {
                        //GD.Print("sled", _sled.GlobalPosition);
                        _behindTimer[counter] += (float)delta;
                        //GD.Print(_behindTimer[counter]);
                    } else {
                        _behindTimer[counter] -= (float)delta;
                    }

                    Vector2 rot = Vector2.Zero;
                    if(GameManager.GetInstance().RestrictCameraControls)
                    {
                        // Only player one can control camera
                        if(dog.Id == 0)
                        {
                            rot = GetRotationInput(dog);
                        }
                    }
                    else // anyone can control camera
                    {
                        rot = GetRotationInput(dog);
                    }
                    targetOffset += targetOffset.Lerp(-_sled.GlobalBasis.X * rot.X * 20, 4f*(float)delta);
                    targetOffset += targetOffset.Lerp(-_sled.GlobalBasis.Y * rot.Y * 25, 4f*(float)delta);

                    IsDogBehindSled(dog);
                    
                    _behindTimer[counter] = Math.Clamp(_behindTimer[counter], 0, 1);
                    DesiredLookAt += dog.GlobalPosition.Lerp(_sledPivot.GlobalPosition, _behindLerp.Sample(_behindTimer[counter]));

                    counter++;
                }
            }
        
        if(counter == 0) {
            if(isADogBehindSled)
                 _lookTarget.GlobalPosition = _lookTarget.GlobalPosition.Lerp(_sled.GlobalPosition + new Vector3(0, 2, 0) + targetOffset, (float)delta * _lerpFactor2);
            else
                _lookTarget.GlobalPosition = _lookTarget.GlobalPosition.Lerp(_sled.GlobalPosition + targetOffset, (float)delta * _lerpFactor2);
        }
        else {
            DesiredLookAt = DesiredLookAt / counter;

            // if dog is behind sled we look ahead more instead of straight down on sled
            if(isADogBehindSled)
                 _lookTarget.GlobalPosition = _lookTarget.GlobalPosition.Lerp(DesiredLookAt + new Vector3(0, 2, 0) + targetOffset, (float)delta * _lerpFactor2);
            else
                _lookTarget.GlobalPosition = _lookTarget.GlobalPosition.Lerp(DesiredLookAt + targetOffset, (float)delta * _lerpFactor2);
        }
        //GD.Print(DesiredLookAt);
    }
}
