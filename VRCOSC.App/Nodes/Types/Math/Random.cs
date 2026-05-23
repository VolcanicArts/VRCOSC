// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace VRCOSC.App.Nodes.Types.Math;

[Node("Random", "Math")]
public sealed class RandomNode<T>() : ResultComputeNode<T>("Min", "Max") where T : INumberBase<T>
{
    private const int max_retries = 10;

    private readonly bool isIntegerType = typeof(T).GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBinaryInteger<>));

    public GlobalStore<T> PrevValue = new();

    public ValueInput<bool> NoRepeat = new();

    protected override T ComputeResult(T min, T max, IPulseContext c)
    {
        // Inclusive integer
        if (isIntegerType) max++;
        var noRepeat = NoRepeat.Read(c);

        if (!noRepeat) return Utils.Interpolation.Map(Random.Shared.NextDouble(), 0d, 1d, min, max);

        var prevValue = PrevValue.Read(c);

        if (EqualityComparer<T>.Default.Equals(min, max))
            return min;

        T value;
        var attempt = 0;

        do
        {
            value = Utils.Interpolation.Map(Random.Shared.NextDouble(), 0d, 1d, min, max);
            attempt++;

            if (attempt >= max_retries)
                break;
        } while (EqualityComparer<T>.Default.Equals(value, prevValue));

        PrevValue.Write(value, c);
        return value;
    }
}