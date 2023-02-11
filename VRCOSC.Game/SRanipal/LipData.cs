using System;
using System.Runtime.InteropServices;

namespace VRCOSC.Game.SRanipal;

public enum LipShapeV2
{
    //None = -1,
    JawRight = 0,
    JawLeft = 1,
    JawForward = 2,
    JawOpen = 3,
    MouthApeShape = 4,
    MouthUpperRight = 5,
    MouthUpperLeft = 6,
    MouthLowerRight = 7,
    MouthLowerLeft = 8,
    MouthUpperOverturn = 9,
    MouthLowerOverturn = 10,
    MouthPout = 11,
    MouthSmileRight = 12,
    MouthSmileLeft = 13,
    MouthSadRight = 14,
    MouthSadLeft = 15,
    CheekPuffRight = 16,
    CheekPuffLeft = 17,
    CheekSuck = 18,
    MouthUpperUpRight = 19,
    MouthUpperUpLeft = 20,
    MouthLowerDownRight = 21,
    MouthLowerDownLeft = 22,
    MouthUpperInside = 23,
    MouthLowerInside = 24,
    MouthLowerOverlay = 25,
    TongueLongStep1 = 26,
    TongueLongStep2 = 32,
    TongueDown = 30,
    TongueUp = 29,
    TongueRight = 28,
    TongueLeft = 27,
    TongueRoll = 31,
    TongueUpLeftMorph = 34,
    TongueUpRightMorph = 33,
    TongueDownLeftMorph = 36,
    TongueDownRightMorph = 35,
    //Max = 37
}

[StructLayout(LayoutKind.Sequential)]
public struct PredictionDataV2
{
    public unsafe fixed float blend_shape_weight[60];
};

[StructLayout(LayoutKind.Sequential)]
public struct LipDataV2
{
    public int frame;
    public int time;
    public IntPtr image;
    public PredictionDataV2 prediction_data;
};
