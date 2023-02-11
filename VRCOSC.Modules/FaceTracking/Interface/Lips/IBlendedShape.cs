// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.SRanipal;

namespace VRCOSC.Modules.FaceTracking.Interface.Lips;

public abstract class BlendedShape
{
    // VRC remote sync precision
    private const float shape_tolerance = 1f / 128f;
    private float previousValue;
    private float value;

    public bool HasChanged => Math.Abs(previousValue - value) > shape_tolerance;

    public float GetShape(float[] values)
    {
        previousValue = value;
        value = CalculateShape(values);
        return value;
    }

    protected abstract float CalculateShape(float[] values);
}

public class DirectShape : BlendedShape
{
    private readonly int key;

    public DirectShape(LipShapeV2 key)
    {
        this.key = (int)key;
    }

    protected override float CalculateShape(float[] values) => values[key];
}

public class PercentageShape : BlendedShape
{
    private readonly int positiveKey;
    private readonly int negativeKey;

    public PercentageShape(LipShapeV2 positiveKey, LipShapeV2 negativeKey)
    {
        this.positiveKey = (int)positiveKey;
        this.negativeKey = (int)negativeKey;
    }

    protected override float CalculateShape(float[] values)
    {
        var positiveValue = values[positiveKey];
        var negativeValue = values[negativeKey] * -1;
        return positiveValue + negativeValue;
    }
}

public class SteppedPercentageShape : BlendedShape
{
    private readonly int positiveKey;
    private readonly int negativeKey;

    public SteppedPercentageShape(LipShapeV2 positiveKey, LipShapeV2 negativeKey)
    {
        this.positiveKey = (int)positiveKey;
        this.negativeKey = (int)negativeKey;
    }

    protected override float CalculateShape(float[] values)
    {
        var positiveValue = values[positiveKey];
        var negativeValue = values[negativeKey] * -1;
        return positiveValue - negativeValue - 1;
    }
}

public class PercentageAveragedShape : BlendedShape
{
    private readonly IEnumerable<int> positiveKeys;
    private readonly IEnumerable<int> negativeKeys;
    private readonly int positiveLength;
    private readonly int negativeLength;

    public PercentageAveragedShape(IReadOnlyCollection<LipShapeV2> positiveShapes, IReadOnlyCollection<LipShapeV2> negativeShapes)
    {
        positiveKeys = positiveShapes.Select(k => (int)k);
        negativeKeys = negativeShapes.Select(k => (int)k);
        positiveLength = positiveShapes.Count;
        negativeLength = negativeShapes.Count;
    }

    protected override float CalculateShape(float[] values)
    {
        var positiveAverage = positiveKeys.Sum(k => values[k]) / positiveLength;
        var negativeAverage = negativeKeys.Sum(k => values[k] * -1) / negativeLength;
        return positiveAverage + negativeAverage;
    }
}

public class MaxAveragedShape : BlendedShape
{
    private readonly IEnumerable<int> positiveKeys;
    private readonly IEnumerable<int> negativeKeys;

    public MaxAveragedShape(IEnumerable<LipShapeV2> positiveShapes, IEnumerable<LipShapeV2> negativeShapes)
    {
        positiveKeys = positiveShapes.Select(k => (int)k);
        negativeKeys = negativeShapes.Select(k => (int)k);
    }

    protected override float CalculateShape(float[] values)
    {
        var positiveMax = positiveKeys.Select(k => values[k]).Max();
        var negativeMax = negativeKeys.Select(k => values[k]).Max() * -1;
        return positiveMax + negativeMax;
    }
}
