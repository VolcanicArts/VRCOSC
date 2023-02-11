// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.SRanipal;

namespace VRCOSC.Modules.FaceTracking.Interface.Lips;

public class LipTrackingData
{
    // VRC remote sync precision
    private const float shape_tolerance = 1f / 128f;
    private static readonly int weighting_count = Enum.GetValues<LipShapeV2>().Length;

    private readonly Dictionary<LipShapeV2, float> shapes = new();

    public IReadOnlyDictionary<LipShapeV2, float> Shapes => shapes;
    public List<LipShapeV2> ChangedShapes = new();

    public void Initialise()
    {
        shapes.Clear();

        for (var i = 0; i < weighting_count; i++)
        {
            shapes.Add((LipShapeV2)i, 0);
        }
    }

    public void Update(LipDataV2 lipData)
    {
        // TODO - Audit shapes and only send those that have changed by the shape_tolerance. Evaluate if needed?
        //auditChangedShapes(lipData);

        unsafe
        {
            for (int i = 0; i < weighting_count; i++)
            {
                shapes[(LipShapeV2)i] = lipData.prediction_data.blend_shape_weight[i];
            }
        }
    }

    // private void auditChangedShapes(LipDataV2 lipData)
    // {
    //     ChangedShapes.Clear();
    //
    //     unsafe
    //     {
    //         for (int i = 0; i < weighting_count; i++)
    //         {
    //             var lookup = (LipShapeV2)i;
    //             var oldShape = shapes[lookup];
    //             var newShape = lipData.prediction_data.blend_shape_weight[i];
    //
    //             if (Math.Abs(oldShape - newShape) > shape_tolerance)
    //             {
    //                 shapes[lookup] = newShape;
    //                 ChangedShapes.Add(lookup);
    //             }
    //         }
    //     }
    //
    //     Logger.Log("Total changed shapes: " + ChangedShapes.Count, LoggingTarget.Information);
    // }
}
