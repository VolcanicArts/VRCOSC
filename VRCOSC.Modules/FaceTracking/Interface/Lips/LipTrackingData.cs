// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.SRanipal;

namespace VRCOSC.Modules.FaceTracking.Interface.Lips;

public class LipTrackingData
{
    public readonly float[] Shapes = new float[37];

    public void Initialise()
    {
        Shapes.Initialize();
    }

    public unsafe void Update(LipDataV2 lipData)
    {
        for (var i = 0; i < Shapes.Length; i++)
        {
            Shapes[i] = lipData.prediction_data.blend_shape_weight[i];
        }
    }
}
