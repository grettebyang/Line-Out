using Godot;
using System;

// Dummy script so that dogsled.cs can find the correct attatchment point.
// We could compare by name as well, but if the name of the attatchment point was ever changed accidentally,
// then it would stop working, meanwhile this will work even if the name of the attatchment point is changed, as long as this script is attatched to the node
public partial class RopePivot : Node3D
{
}
