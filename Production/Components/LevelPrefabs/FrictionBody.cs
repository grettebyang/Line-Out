using Godot;
using System;
using System.Runtime.Intrinsics.X86;

public partial class FrictionBody : Node3D
{
    public static readonly float NormalFriction = 5.0f;
    public static readonly float IceFriction = 1.0f;
    public static readonly float MudFriction = 10f;

    public static readonly float NormalSpeed = 1.0f;
    public static readonly float IceSpeed = 1f;
    public static readonly float MudSpeed = 0.75f;

    public enum SURFACE_TYPES {
        NORMAL,
        ICE,
        MUD,
        UNDEFINED
    }

    [Export] public SURFACE_TYPES SurfaceType = SURFACE_TYPES.NORMAL;

    public float GetFriction() {
        switch (SurfaceType) {
            case SURFACE_TYPES.NORMAL:
                return NormalFriction;
            case SURFACE_TYPES.ICE:
                return IceFriction;
            case SURFACE_TYPES.MUD:
                return MudFriction;
            default:
                return 1.0f;
        }
    }

    public float GetSpeedAdjustment() {
        switch (SurfaceType) {
            case SURFACE_TYPES.NORMAL:
                return NormalSpeed;
            case SURFACE_TYPES.ICE:
                return IceSpeed;
            case SURFACE_TYPES.MUD:
                return MudSpeed;
            default:
                return 1.0f;
        }
    }
}
