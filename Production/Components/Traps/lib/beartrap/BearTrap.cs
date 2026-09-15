using Godot;
using System;

public partial class BearTrap : CrashableTrapBase{
    [Export]
    Node3D _rightPivot, _leftPivot;
    [Export]
    Node3D _stuckPos;
    [Export]
    Curve _bearTrapSnap, _bearTrapShut;
    Transform3D _lMinTransform, _rMinTransform;
    Transform3D _lMaxTransform, _rMaxTransform;
    Node3D _stuckEnt;
    public AudioStreamPlayer3D _snapAudio;
    public bool _audioPlayed;
    protected Damage _effect = new Damage();

    

    float _injureAmount;
    
    float _disableTime = 4, _stuckTime = 3;


    public override void _EnterTree(){
        _singleTarget = true;
        _active = false;
        //set effects for affected ents to read
       
        _effect._amount = 20;
        _effect._makesStuck = true;
        _effect._stuckTime = _stuckTime;
        _effect._source = "BearTrap";
        _effect._posNode = _stuckPos;
        
        SetDamage(_effect);
        //set transforms for animations
        _lMinTransform = _leftPivot.Transform;
        _rMinTransform = _rightPivot.Transform;
        _lMaxTransform = _leftPivot.Transform.Rotated(_leftPivot.Basis.X, (float)Math.PI/2 - 0.12f);
        //GD.Print(_lMaxTransform);
        _rMaxTransform = _rightPivot.Transform.Rotated(_rightPivot.Basis.X, (float)Math.PI/2- 0.12f);
        //GD.Print(_rMaxTransform);
        _snapAudio = GetNode<AudioStreamPlayer3D>("BearTrapClose");
        _audioPlayed = false;
    }


    public override void CustomCollision(float delta){


    }

    public void PlaySnapAudio(){
        if(_occupied && !_audioPlayed){
            _snapAudio.Play();
            _audioPlayed = true;
        }
    }


    
    public override void Always(float delta){
        //GD.Print(_stuckTimer);
        _stuckTimer += delta;
        float _currentState;
        if(_occupied == false){
            _currentState = Math.Clamp( _stuckTimer / _disableTime, 0,1);
            _currentState = _bearTrapShut.Sample(1 - _currentState);
            _collisionsDisabled = false;
            if(_stuckTimer > _disableTime)
                _active = true;
        }
        else{
            _active = false;
            _currentState = Math.Clamp( _stuckTimer, 0,1);
            _currentState = _bearTrapSnap.Sample(_currentState);
            if(_stuckTimer > _stuckTime){
                 //GD.Print("test");
                _occupied = false;
                _audioPlayed = false;
                _stuckEnt = null;
                _collisionsDisabled = false;
                _stuckTimer = 0;
                //GD.Print("1");
                if (_stuckEnt != null){

                    _stuckEnt = null;
                }
            }
            // if (_stuckEnt == null){
            //     _stuckTimer = 0;
            //     _collisionsDisabled = false;
            // }
            
        }
        PlaySnapAudio();
        _rightPivot.Basis = _rMinTransform.Basis.Slerp(_rMaxTransform.Basis, _currentState);
        _leftPivot.Basis = _lMinTransform.Basis.Slerp(_lMaxTransform.Basis, _currentState);

    }
    public override void CustomEmpty(float delta){
        _occupied = false;
    }

}

