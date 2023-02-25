// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.InteropServices;
using osuTK;

namespace VRCOSC.Game.SRanipal;

public enum SingleEyeDataValidity
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
