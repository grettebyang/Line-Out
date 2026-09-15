using Godot;
using System;

[GlobalClass]
public partial class SequenceChange : Resource
{
	[Export]
	public int[] _sequence;
	[Export]
	public int bars;
	[Export]
	public int timeSignature;
}
