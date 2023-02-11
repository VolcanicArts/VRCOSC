// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.SRanipal;

namespace VRCOSC.Modules.FaceTracking.Interface;

public class EyeTrackingData
{
    public Eye Left;
    public Eye Right;

    public float EyesDilation;
    public float EyesPupilDiameter;

    private float maxDilation;
    private float minDilation;

    public void Initialise()
    {
        Left.Initialise();
        Right.Initialise();

        EyesDilation = 0f;
        EyesPupilDiameter = 0f;

        maxDilation = float.MaxValue;
        minDilation = 0;
    }

    public void Update(EyeDataV2 eyeData)
    {
        var dilation = 0f;

        if (eyeData.verbose_data.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY))
        {
            dilation = eyeData.verbose_data.right.pupil_diameter_mm;
            updateMinMaxDilation(eyeData.verbose_data.right.pupil_diameter_mm);
        }
        else if (eyeData.verbose_data.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY))
        {
            dilation = eyeData.verbose_data.left.pupil_diameter_mm;
            updateMinMaxDilation(eyeData.verbose_data.left.pupil_diameter_mm);
        }

        Left.Update(eyeData.verbose_data.left, eyeData.expression_data.left);
        Right.Update(eyeData.verbose_data.right, eyeData.expression_data.right);

        if (dilation == 0) return;

        EyesDilation = (dilation - minDilation) / (maxDilation - minDilation);
        EyesPupilDiameter = dilation > 10 ? 1 : dilation / 10;
    }

    private void updateMinMaxDilation(float readDilation)
    {
        if (readDilation > maxDilation)
            maxDilation = readDilation;

        if (readDilation < minDilation)
            minDilation = readDilation;
    }
}
