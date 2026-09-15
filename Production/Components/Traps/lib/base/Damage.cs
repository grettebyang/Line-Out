using Godot;

interface ITrappable{
    void TrapEffect(Damage dmg);
}
public class Damage{
    
    //how much velocity and impulse are relative to each other
        public static float velImpulseFactor = 1;
        //how much damage
        public int _amount = 0;
        //does it disable regular movement
        public bool _makesStuck = false;
        public float _stuckTime = 0;
        //slow down or speed up vs this much
        public float _speedModulate = 1;
        //a burst of added velocity, might need to add an extra one for rigid bodies
        public Vector3 _pushAmountVec;
        public float _pushAmountVal;
        public bool _triggerMusicEvent = false;
        //change this if you want to create a filter on certain objects, such as if(dmg._source = geyzer){ return; }
        public string _source;
        //only need to use this if we need to pass node info
        public Node3D _posNode;
        public AudioStreamWav _trapSFX;
}
