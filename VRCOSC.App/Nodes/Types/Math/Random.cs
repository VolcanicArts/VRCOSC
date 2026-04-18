// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Numerics;

namespace VRCOSC.App.Nodes.Types.Math;

[Node("Random", "Math")]
public sealed class RandomNode<T> : ValueComputeNode<T> where T : INumberBase<T>
{
    private readonly Random random = new();

    public ValueInput<T> Min = new();
    public ValueInput<T> Max = new();

    protected override T ComputeValue(PulseContext c) => Utils.Interpolation.Map(random.NextDouble(), 0d, 1d, Min.Read(c), Max.Read(c));
}