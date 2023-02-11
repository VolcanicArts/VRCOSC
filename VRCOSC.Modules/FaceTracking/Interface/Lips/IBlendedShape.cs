// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.SRanipal;

namespace VRCOSC.Modules.FaceTracking.Interface.Lips;

public interface IBlendedShape
{
    public float GetBlendedShape(float[] values);
}

public class PercentageShape : IBlendedShape
{
    private readonly int positiveKey;
    private readonly int negativeKey;

    public PercentageShape(LipShapeV2 positiveKey, LipShapeV2 negativeKey)
    {
        this.positiveKey = (int)positiveKey;
        this.negativeKey = (int)negativeKey;
    }

    public float GetBlendedShape(float[] values)
    {
        var positiveValue = values[positiveKey];
        var negativeValue = values[negativeKey] * -1;
        return positiveValue + negativeValue;
    }
}

public class SteppedPercentageShape : IBlendedShape
{
    private readonly int positiveKey;
    private readonly int negativeKey;

    public SteppedPercentageShape(LipShapeV2 positiveKey, LipShapeV2 negativeKey)
    {
        this.positiveKey = (int)positiveKey;
        this.negativeKey = (int)negativeKey;
    }

    public float GetBlendedShape(float[] values)
    {
        var positiveValue = values[positiveKey];
        var negativeValue = values[negativeKey] * -1;
        return positiveValue - negativeValue - 1;
    }
}

public class PercentageAveragedShape : IBlendedShape
{
    private readonly IEnumerable<int> positiveKeys;
    private readonly IEnumerable<int> negativeKeys;

    public PercentageAveragedShape(IEnumerable<LipShapeV2> positiveShapes, IEnumerable<LipShapeV2> negativeShapes)
    {
        positiveKeys = positiveShapes.Select(k => (int)k);
        negativeKeys = negativeShapes.Select(k => (int)k);
    }

    public float GetBlendedShape(float[] values)
    {
        var positive = positiveKeys.Sum(k => values[k]);
        var negative = negativeKeys.Sum(k => values[k] * -1);

        return positive / positiveKeys.Count() + negative / negativeKeys.Count();
    }
}

public class MaxAveragedShape : IBlendedShape
{
    private readonly IEnumerable<int> positiveKeys;
    private readonly IEnumerable<int> negativeKeys;

    public MaxAveragedShape(IEnumerable<LipShapeV2> positiveShapes, IEnumerable<LipShapeV2> negativeShapes)
    {
        positiveKeys = positiveShapes.Select(k => (int)k);
        negativeKeys = negativeShapes.Select(k => (int)k);
    }

    public float GetBlendedShape(float[] values)
    {
        var positiveValues = positiveKeys.Select(k => values[k]);
        var negativeValues = negativeKeys.Select(k => values[k]);

        return positiveValues.Max() + -1 * negativeValues.Max();
    }
}
