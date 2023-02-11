using System;
using System.Runtime.InteropServices;

namespace VRCOSC.Game.SRanipal;

public enum LipShapeV2
{
    JawRight,
    JawLeft,
    JawForward,
    JawOpen,
    MouthApeShape,
    MouthUpperRight,
    MouthUpperLeft,
    MouthLowerRight,
    MouthLowerLeft,
    MouthUpperOverturn,
    MouthLowerOverturn,
    MouthPout,
    MouthSmileRight,
    MouthSmileLeft,
    MouthSadRight,
    MouthSadLeft,
    CheekPuffRight,
    CheekPuffLeft,
    CheekSuck,
    MouthUpperUpRight,
    MouthUpperUpLeft,
    MouthLowerDownRight,
    MouthLowerDownLeft,
    MouthUpperInside,
    MouthLowerInside,
    MouthLowerOverlay,
    TongueLongStep1,
    TongueLeft,
    TongueRight,
    TongueUp,
    TongueDown,
    TongueRoll,
    TongueLongStep2,
    TongueUpRightMorph,
    TongueUpLeftMorph,
    TongueDownRightMorph,
    TongueDownLeftMorph
}

[StructLayout(LayoutKind.Sequential)]
public struct PredictionDataV2
{
    public unsafe fixed float blend_shape_weight[37];
};

[StructLayout(LayoutKind.Sequential)]
public struct LipDataV2
{
    public int frame;
    public int time;
    public IntPtr image;
    public PredictionDataV2 prediction_data;
};
