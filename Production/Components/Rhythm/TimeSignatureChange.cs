using Godot;
using System;

[GlobalClass]
public partial class TimeSignatureChange : Resource
{
	[Export]
	public int top;
	[Export]
	public int bottom;
	[Export]
	public int bars;
	[Export]
	public int onBeat;	
}
