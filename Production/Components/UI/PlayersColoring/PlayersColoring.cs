using Godot;
using System;
using System.Collections.Generic;

/*
Creates 3D UI over existing dogs
*/
public partial class PlayersColoring : Node
{
    private const float DRAW_OFFSET = 0.5f;
    private const float DRAW_SPHERE_SIZE = 0.15f;

    [Export] private DogSled _sled;
    private List<Node> _dogs;

    public override void _Process(double delta) {
        if (_sled == null || _sled.DogSpawner == null)
            return;

        _dogs = _sled.DogSpawner.DogList;

        // Draw indicators over players
        foreach (DogController dog in _dogs) {
            Vector3 pos = dog.GlobalPosition + new Vector3(0, DRAW_OFFSET, 0);
            Color col = UIConstants.PLAYER_COLORS[dog.GetId()];

        }
    }
}
