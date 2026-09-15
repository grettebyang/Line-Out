using Godot;
using System;

public enum Tracknames
{
	LAPLANDS_FIKA_DELIVERY_DOGS,
	CALIBRATION_TRACK,
	YOUR_DRIVEWAY_IS_A_DISASTER,
	NOT_TOO_SWEET,
	HOUSE_BLEND,
	DANK_ROAST
}

public partial class SoundtrackCollection : Node
{
	private Godot.Collections.Array<Soundtrack> TrackList = new Godot.Collections.Array<Soundtrack>();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var children = GetChildren();
		foreach(Soundtrack track in children)
		{
			TrackList.Add(track);
		}

	}

	public Soundtrack GetSoundtrack(Tracknames name)
	{
		return TrackList[(int)name];
	}
}
