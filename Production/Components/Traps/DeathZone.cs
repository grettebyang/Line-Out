using Godot;
using System;
using System.ComponentModel;

public partial class DeathZone : TrapBase{
    protected Damage _effect = new Damage();
    [Export] 
    string _src = "Death";
    public override void _EnterTree(){
        _active = true;
        _effect._source = _src;
        _effect._amount = 2;
        _effect._trapSFX = _trapSound;
        SetDamage(_effect);
    }
}
