using Godot;
using System;
using System.Collections.Generic;
//base class for traps, when creating a new trap update the member functions, use Always instead of _Process and update 
public partial class TrapBase : Area3D{
    public
    bool _collisionsDisabled = false;
    protected Damage _thisEffect = new Damage();
    public float _gracePeriod;
    float _delta = 0;
    
    protected Godot.Collections.Dictionary<Node3D,float> _graceList = [];
    public bool _singleTarget= false;
    public float _timer = 0, _stuckTimer = 0;
    public bool _occupied;
    private BasicTimeManager _timeManager; 
    public bool _active = true;
    [Export]
    public AudioStreamWav _trapSound;


    public virtual void CustomReady(){

    }
    public void SetDamage(Damage dmg){
        _thisEffect = dmg;
    }
    public override void _Ready(){
        _timeManager = GetNode("/root/TimeManager") as BasicTimeManager;
        //GD.Print("initialised trap");
    }


 
    public override void _Process(double delta){
        //swap this with game manager time
        _delta = (float)delta * _timeManager.GameSpeed;
        if(delta == 0)
            return;
        
        Debug(_delta);
        Always(_delta);


        if(!HasOverlappingBodies()){
            CustomEmpty(_delta);

        }   
        
        if(_active & !_collisionsDisabled){
            ////GD.Print("I am active");
            WhenCollision(_delta);
            CustomCollision(_delta);
        }
        AlwaysLate(_delta);
        //GD.Print(_graceLi`t.Values);
        if(_gracePeriod == 0)
            return;
        foreach(Node3D key in _graceList.Keys){
            _graceList[key] += _delta;
            if(_graceList[key] > _gracePeriod)
            _graceList.Remove(key);
        }
        //GD.Print(_graceList.Values);
    }

    //override this for when nothing is in the area
    //example, if we have something that has a threatened vs non threatened mode
    public virtual void CustomEmpty(float delta){

    }
    public virtual void AlwaysLate(float delta){

    }
    //don't overide this
    protected void WhenCollision(float delta){
        foreach (Node3D obj in GetOverlappingBodies()){
            if(obj is not ITrappable || _graceList.ContainsKey(obj))
                return;
            
            ITrappable castObj = obj as ITrappable;
            castObj.TrapEffect(_thisEffect);
            if(_gracePeriod > 0){
                _graceList.Add(obj, 0.00001f);
            }
            if(_singleTarget){
                _collisionsDisabled = true;
                _occupied = true;
                _active = false;
                _stuckTimer = 0;
                return;
            }

        }
    }
    //overide this if you want to add custom animations or other actions
    public virtual void CustomCollision(float delta){
        
    }
    public virtual void SingleTarget(){

    }

    public virtual void Always(float delta){

    }
    public virtual void Debug(float delta){

    }
}
