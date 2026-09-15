using Godot;
using System;

public partial class RhythmEvent : Node
{
	// Called when the node enters the scene tree for the first time.

	/// 
	/// Virtual methods
	/// 
	
	public virtual void RunSequence(){}
	public virtual void SequenceGo(){}
	public virtual void EndSequence(){}
	public virtual void GatherInput(){}
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
