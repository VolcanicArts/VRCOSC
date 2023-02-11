// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osuTK;
using VRCOSC.Game;
using VRCOSC.Game.SRanipal;

namespace VRCOSC.Modules.FaceTracking.Interface.Eyes;

public struct Eye
{
    public Vector2 Look;
    public float Openness;
    public float Widen;
    public float Squeeze;

    public void Initialise()
    {
        Look = new Vector2();
        Openness = 0f;
        Widen = 0f;
        Squeeze = 0f;
    }

    public void Update(SingleEyeData eyeData, SingleEyeExpression? expression = null)
    {
        if (eyeData.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY))
            Look = eyeData.gaze_direction_normalized.Invert().ToVec2();

        Openness = eyeData.eye_openness;

        if (expression is null) return;

        Widen = expression.Value.eye_wide;
        Squeeze = expression.Value.eye_squeeze;
    }
}
