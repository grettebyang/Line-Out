using Godot;
using System;

public partial class LoadingScreen : CanvasLayer
{
    [Export] float _loadingSpinSpeed = 5;
    [Export] LoadingIcon _loadingIcon;
    [Export] AnimationPlayer _animator;

    private static LoadingScreen _instance;

    private LoadingScreen() {
        if (_instance != null)
            return;

        _instance = this;
    }

    public static LoadingScreen GetInstance() {
        return _instance;
    }

    public override void _Ready()
    {
        HideLoading();
    }

    public override void _Process(double delta)
	{
		_loadingIcon.Rotation += _loadingSpinSpeed * (float)delta;
	}

    public void DisplayLoading()
    {
        _animator.Play("Enter");
        _loadingIcon.OnEnter();
    }

    public void HideLoading()
    {
        _animator.Play("Leave");
        _loadingIcon.OnHide();
    }
}
