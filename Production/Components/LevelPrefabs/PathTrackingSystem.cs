using Godot;
using System;
//[Tool]

public partial class PathTrackingSystem : Node3D{

    Godot.Collections.Array<Path3D> _paths;
    [Export] Node3D _sled;
    //int _currentPathIndex, _closestPathIndex;
    [Export] Camera _cam;
    Godot.Collections.Array<float> _pathDists;
    Godot.Collections.Array<PathFollow3D> _trackers;
    int _closestPath = 0, _currentPath = 0;
    [Export] float _offset, _distToDelete, _distToIgnore, _rotationFactor;
    float _closestDist;
    //change this to an angle float later that can be passed into UI
    [Export] Node3D _placeHolderCompass, _smoothCompass;
    //if we are ignoring Y axis
    [Export] bool _ignoreY, _enabled;
    float _angleToTracker;

    public override void _Ready()
    {
        InitialState();
        if (GameManager.GetInstance().CurrentLevel._sled != null)
        {
            _sled = GameManager.GetInstance().CurrentLevel._sled;
        }

        _smoothCompass.Visible = false;
    }

    public void InitialState()
    {   
        if (_trackers != null)
        {
            foreach (PathFollow3D a in _trackers)
            {
                a.Free();
            }
        }
        _pathDists = new Godot.Collections.Array<float>();
        _paths = new Godot.Collections.Array<Path3D>();
        _trackers = new Godot.Collections.Array<PathFollow3D>();

        GD.Print("Reset Sled Path");
        int index = 0;
        foreach (Node3D a in GetChildren()){
            if (a is Path3D) {
                _trackers.Add(new PathFollow3D());
                a.AddChild(_trackers[index]);
                _paths.Add(a as Path3D);
                _pathDists.Add(99999999);
                index++;
            }
        }
    }

    void UpdateTracker(int Index, Vector3 sledPos)
    {   
        if (_paths[Index] == null) { return; }
        
        _trackers[Index].Progress = _paths[Index].Curve.GetClosestOffset(sledPos);

        Vector3 a = _trackers[Index].GlobalPosition;
        _trackers[Index].Progress += _offset;
        
        Vector3 b = _trackers[Index].GlobalPosition;
        _pathDists[Index] = _trackers[Index].GlobalPosition.DistanceTo(_sled.GlobalPosition);

        if (a.DistanceTo(b) < _distToDelete && _pathDists[Index] < _distToIgnore)
        {
            _paths.RemoveAt(Index);
            _pathDists.RemoveAt(Index);
            _trackers.RemoveAt(Index);

        }
    }
    public float ReturnAngle()
    {
        return _smoothCompass.GlobalRotation.Y;
    }



    //off set which path gets updated each frame, but update closest path always
    //sometimes we will update the same path twice, but that is okay. Fixed cost
    //some things will be moved to _ready later, currently this is so I can use [tool] to test in the editor
    public override void _PhysicsProcess(double delta)
    {
        if (!_enabled) {
            _placeHolderCompass.Visible = false;
            return;
        }
        else {
            _placeHolderCompass.Visible = false;
        }
        if (Input.IsKeyLabelPressed(Key.Z))
        {
            InitialState();
        }
        if (Input.IsKeyPressed(Key.X))
        {
            GD.Print(_angleToTracker);
        }
        Vector3 sledPos = _sled.GlobalPosition;
        sledPos.Y += 2;
        _placeHolderCompass.GlobalPosition = sledPos;
        _smoothCompass.GlobalPosition = sledPos;
        if (_ignoreY)
        {
            sledPos.Y = 0;
        }
        else
        {
            sledPos.Y -= 2;
        }

        UpdateTracker(_closestPath, sledPos);
        UpdateTracker(_currentPath, sledPos);
        UpdateClosest();
        _currentPath++;
        if (_currentPath >= _paths.Count)
        {
            _currentPath = 0;
        }
        sledPos.Y = 0;

        PathFollow3D closest = _trackers[_closestPath];
        _placeHolderCompass.LookAt(closest.GlobalPosition);
        _placeHolderCompass.GlobalRotation = new Vector3(0, _placeHolderCompass.GlobalRotation.Y, 0);

        Vector3 trackerPos = _trackers[_closestPath].GlobalPosition;
        trackerPos.Y = 0;
        _smoothCompass.Quaternion = _smoothCompass.Quaternion.Slerp(_placeHolderCompass.Quaternion, (float)delta * _rotationFactor);
        _smoothCompass.GlobalRotation = new Vector3(0, _smoothCompass.GlobalRotation.Y, 0);


        //GD.Print(_angleToTracker);
        
        _angleToTracker = _smoothCompass.GlobalRotation.Y;


    }

    void UpdateClosest()
    {
        _closestDist = 99999;
        _closestPath = 0;
        for (int i = _pathDists.Count - 1; i >= 0; i--)
        {
            if (_pathDists[i] < _closestDist)
            {
                _closestDist = _pathDists[i];
                _closestPath = i;
            }
        }
    }



}
