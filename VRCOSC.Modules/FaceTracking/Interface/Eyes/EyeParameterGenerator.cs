// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Modules.FaceTracking.Interface.Eyes;

public static class EyeParameterGenerator
{
    public static readonly Dictionary<EyeParam, Func<EyeTrackingData, float>> FLOAT_VALUES = new()
    {
        { EyeParam.EyesX, eye => eye.Combined.Look.X },
        { EyeParam.EyesY, eye => eye.Combined.Look.Y },
        { EyeParam.LeftEyeX, eye => eye.Left.Look.Y },
        { EyeParam.LeftEyeY, eye => eye.Left.Look.Y },
        { EyeParam.RightEyeX, eye => eye.Right.Look.Y },
        { EyeParam.RightEyeY, eye => eye.Right.Look.Y },

        { EyeParam.EyesWiden, eye => eye.Combined.Widen },
        { EyeParam.LeftEyeWiden, eye => eye.Left.Widen },
        { EyeParam.RightEyeWiden, eye => eye.Right.Widen },

        { EyeParam.EyesSqueeze, eye => eye.Combined.Squeeze },
        { EyeParam.LeftEyeSqueeze, eye => eye.Left.Squeeze },
        { EyeParam.RightEyeSqueeze, eye => eye.Right.Squeeze },

        { EyeParam.EyesDilation, eye => eye.EyesDilation },
        { EyeParam.EyesPupilDiameter, eye => eye.EyesPupilDiameter },

        { EyeParam.LeftEyeLid, eye => eye.Left.Openness },
        { EyeParam.RightEyeLid, eye => eye.Right.Openness },
        { EyeParam.CombinedEyeLid, eye => (eye.Left.Openness + eye.Right.Openness) / 2f },

        { EyeParam.LeftEyeLidExpanded, eye => eye.Left.Widen > 0 ? map(eye.Left.Widen, 0, 1, 0.8f, 1) : map(eye.Left.Openness, 0, 1, 0, 0.8f) },
        { EyeParam.RightEyeLidExpanded, eye => eye.Right.Widen > 0 ? map(eye.Right.Widen, 0, 1, 0.8f, 1) : map(eye.Right.Openness, 0, 1, 0, 0.8f) },
        { EyeParam.CombinedEyeLidExpanded, eye => eye.Combined.Widen > 0 ? map(eye.Combined.Widen, 0, 1, 0.8f, 1) : map(eye.Combined.Openness, 0, 1, 0, 0.8f) },

        {
            EyeParam.LeftEyeLidExpandedSqueeze, eye =>
            {
                if (eye.Left.Widen > 0) return map(eye.Left.Widen, 0, 1, 0.8f, 1);
                if (eye.Left.Squeeze > 0) return eye.Left.Squeeze * -1;

                return map(eye.Left.Openness, 0, 1, 0, 0.8f);
            }
        },
        {
            EyeParam.RightEyeLidExpandedSqueeze, eye =>
            {
                if (eye.Right.Widen > 0) return map(eye.Right.Widen, 0, 1, 0.8f, 1);
                if (eye.Right.Squeeze > 0) return eye.Right.Squeeze * -1;

                return map(eye.Right.Openness, 0, 1, 0, 0.8f);
            }
        },
        {
            EyeParam.CombinedEyeLidExpandedSqueeze, eye =>
            {
                if (eye.Combined.Widen > 0) return map(eye.Combined.Widen, 0, 1, 0.8f, 1);
                if (eye.Combined.Squeeze > 0) return eye.Combined.Squeeze * -1;

                return map(eye.Combined.Openness, 0, 1, 0, 0.8f);
            }
        },
    };

    public static readonly Dictionary<EyeParam, Func<EyeTrackingData, bool>> BOOL_VALUES = new()
    {
        { EyeParam.LeftEyeWidenToggle, eye => eye.Left.Widen > 0 },
        { EyeParam.RightEyeWidenToggle, eye => eye.Right.Widen > 0 },
        { EyeParam.EyesWidenToggle, eye => eye.Combined.Widen > 0 },

        { EyeParam.LeftEyeSqueezeToggle, eye => eye.Left.Squeeze > 0 },
        { EyeParam.RightEyeSqueezeToggle, eye => eye.Right.Squeeze > 0 },
        { EyeParam.EyesSqueezeToggle, eye => eye.Combined.Squeeze > 0 }
    };

    private static float map(float source, float sMin, float sMax, float dMin, float dMax) => dMin + (dMax - dMin) * ((source - sMin) / (sMax - sMin));
}

public enum EyeParam
{
    EyesX,
    EyesY,
    LeftEyeX,
    LeftEyeY,
    RightEyeX,
    RightEyeY,
    EyesWiden,
    LeftEyeWiden,
    RightEyeWiden,
    EyesSqueeze,
    LeftEyeSqueeze,
    RightEyeSqueeze,
    EyesDilation,
    EyesPupilDiameter,
    LeftEyeLid,
    RightEyeLid,
    CombinedEyeLid,
    LeftEyeLidExpanded,
    RightEyeLidExpanded,
    CombinedEyeLidExpanded,
    LeftEyeLidExpandedSqueeze,
    RightEyeLidExpandedSqueeze,
    CombinedEyeLidExpandedSqueeze,
    LeftEyeWidenToggle,
    RightEyeWidenToggle,
    EyesWidenToggle,
    LeftEyeSqueezeToggle,
    RightEyeSqueezeToggle,
    EyesSqueezeToggle
}
