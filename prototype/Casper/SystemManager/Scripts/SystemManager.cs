using Godot;
using System;
using System.Collections.Generic;

public partial class SystemManager : Node
{

    [Export]
    public SubViewportContainer SubViewportContainer { get; set; }

    private List<Label> _labels;

    public float SessionTime;

    private int _lapCount;

    private float _lapTime;

    public override void _Ready() {
        StartSession();
        AddLabels();
    }

    public override void _Process(double delta) {
        SessionTime += (float)delta;
        _lapTime += (float)delta;
        UpdateLabels();
    }

    public void StartSession() {
        _labels = new List<Label>();
        _lapCount = 0;
        SessionTime = 0;
        _lapTime = 0;
    }

    private void StartLap() {
        _lapCount++;
        _lapTime = 0;

    }


    private void AddLabels() {
        SubViewport subViewport = SubViewportContainer.GetChild<SubViewport>(0);
        foreach (Label label in subViewport.GetChildren()) {
            _labels.Add(label);
        }
    }

    private void UpdateLabels() {
        if (_labels is null) {
            GD.Print("_labels is null");
            return;

        }


        // Temporary
        foreach (Label label in _labels) {
            if (label.Name == "SessionTime") {
                label.Text = $"Session Time: {(int)SessionTime}";
            }
            else if (label.Name == "LapTime") {
                label.Text = "Lap Time: " + (int)_lapTime;
            }
            else if (label.Name == "LapCount") {
                label.Text = "Lap Count: " + _lapCount;
            }
            else {
                GD.Print(label.Name);
            }

        }
    }
}
