//========= Copyright 2018, HTC Corporation. All rights reserved. ===========

using System.Runtime.InteropServices;
using osuTK;

namespace VRCOSC.Game.SRanipal;

public enum EyeIndex
{
    LEFT,
    RIGHT,
}

public enum GazeIndex
{
    LEFT,
    RIGHT,
    COMBINE
}

public enum SingleEyeDataValidity : int
{
    SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY,
    SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY,
    SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY,
    SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY,
    SINGLE_EYE_DATA_PUPIL_POSITION_IN_SENSOR_AREA_VALIDITY
};

[StructLayout(LayoutKind.Sequential)]
public struct TrackingImprovements
{
    public int count;
    public unsafe fixed int items[10];
};

[StructLayout(LayoutKind.Sequential)]
public struct SingleEyeData
{
    public ulong eye_data_validata_bit_mask;
    public Vector3 gaze_origin_mm;
    public Vector3 gaze_direction_normalized;
    public float pupil_diameter_mm;
    public float eye_openness;
    public Vector2 pupil_position_in_sensor_area;

    public bool GetValidity(SingleEyeDataValidity validity) => (eye_data_validata_bit_mask & (ulong)(1 << (int)validity)) > 0;
}

[StructLayout(LayoutKind.Sequential)]
public struct CombinedEyeData
{
    public SingleEyeData eye_data;
    public byte convergence_distance_validity;
    public float convergence_distance_mm;
}

[StructLayout(LayoutKind.Sequential)]
public struct VerboseData
{
    public SingleEyeData left;
    public SingleEyeData right;
    public CombinedEyeData combined;
    public TrackingImprovements tracking_improvements;
}

public enum EyeShapeV2
{
    None = -1,
    Eye_Left_Blink = 0,
    Eye_Left_Wide,
    Eye_Left_Right,
    Eye_Left_Left,
    Eye_Left_Up,
    Eye_Left_Down,
    Eye_Right_Blink = 6,
    Eye_Right_Wide,
    Eye_Right_Right,
    Eye_Right_Left,
    Eye_Right_Up,
    Eye_Right_Down,
    Eye_Frown = 12,
    Eye_Left_Squeeze,
    Eye_Right_Squeeze,
    Max = 15,
}

[StructLayout(LayoutKind.Sequential)]
public struct SingleEyeExpression
{
    public float eye_wide;
    public float eye_squeeze;
    public float eye_frown;
};

[StructLayout(LayoutKind.Sequential)]
public struct EyeExpression
{
    public SingleEyeExpression left;
    public SingleEyeExpression right;
};

[StructLayout(LayoutKind.Sequential)]
public struct EyeDataV2
{
    public byte no_user;
    public int frame_sequence;
    public int timestamp;
    public VerboseData verbose_data;
    public EyeExpression expression_data;
}
