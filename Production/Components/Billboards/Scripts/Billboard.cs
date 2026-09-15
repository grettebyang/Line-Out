using Godot;
using System;

public partial class Billboard : MeshInstance3D
{
    [Export]
    SubViewport viewport;

    public override void _Ready()
    {
        var mat = GetActiveMaterial(0) as StandardMaterial3D;

        if(mat != null)
        {
            mat.EmissionTexture = viewport.GetTexture();
        }
    }
}
