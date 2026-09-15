using Godot;
using System;

public partial class BasicAnimationController : Node{
    //animation controller for the cuboid dog
    //personal todo:
    //Change animation state for running to be based off of the current input vector instead of velocity
    //add aditional trap states for if the dog enters a geyzer, it should ragdoll in a funny way
    //bark animation, tilt head up
    //turning animation, rotate front half into the curve, this will likely require some sort of animation blending

    [Export]
    BaseCharacter _dog;
    [Export]
    AnimationPlayer _anim;

    BasicTimeManager _timeManager;
    
    Node3D _mesh;  
    enum AnimState{
        AIR,
        IDLE,
        WALK,
        RUN
    }
    float _airTimer;
    private AnimState _animState;
    public override void _Ready()
    {
        _timeManager = BasicTimeManager.GetInstance();

        _mesh = GetParent() as Node3D;
        _anim.GetAnimation("Run-loop").LoopMode = Animation.LoopModeEnum.Pingpong;
        _anim.GetAnimation("Jump").LoopMode = Animation.LoopModeEnum.None;
    }

    public override void _Process(double delta) {
        if (_timeManager.GameSpeed <= 0.9f) {
            _anim.SpeedScale = _timeManager.GameSpeed / 2;
        }
        else {
            _anim.SpeedScale = _timeManager.GameSpeed;
        }
        if (_dog.InAir())
        {
            _airTimer += (float)delta;
            if (_airTimer > delta * 2)
                _animState = AnimState.AIR;
        }
        else
        {
            _airTimer = 0;
            if (_dog.GetInputLength() < 0.05)
                _animState = AnimState.IDLE;
            else
                _animState = AnimState.WALK;
        }
        switch(_animState){
            case AnimState.AIR:
                if(_airTimer < 0.3f)
                _anim.Play("Jump");
                ////gdint("air");
                _mesh.LookAt(_mesh.GlobalPosition + _dog.GlobalBasis.Z * 5 + _dog.Velocity * -1);
            break;
            case AnimState.IDLE:
                _mesh.LookAt(_mesh.GlobalPosition + _dog.GlobalBasis.Z);
                if( _anim.CurrentAnimation == "Run-loop")
                    _anim.Stop();
                _anim.Play("Idle");
            break;
            case AnimState.WALK:
                _mesh.LookAt(_mesh.GlobalPosition + _dog.GlobalBasis.Z);
                //_anim.SpeedScale = Math.Clamp(_dog.GetRealVelocity().Length()/3,0,1);
                if( _anim.CurrentAnimation != "Run-loop")
                    _anim.Play("Run-loop");
                //aabs
                //gdint("run");
            break;
        }
        }
        ////gdint(_anim.GetAnimationLibraryList());
}








