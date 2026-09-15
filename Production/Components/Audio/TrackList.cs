using Godot;
using System;

public class TrackList
{
	public static Soundtrack[] TRACK_LIST = {
		ResourceLoader.Load<Soundtrack>("res://Production/Components/Rhythm/Tracks/YourDrivewayisaDisaster.tscn"),
		ResourceLoader.Load<Soundtrack>("res://Production/Components/Rhythm/Tracks/HouseBlend.tscn"),
		ResourceLoader.Load<Soundtrack>("res://Production/Components/Rhythm/Tracks/NotTooSweet.tscn"),
		ResourceLoader.Load<Soundtrack>("res://Production/Components/Rhythm/Tracks/DankRoast.tscn")
	};
}
