// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Nodes.Types.Math;

public abstract class RandomNode<T> : Node where T : notnull
{
    [NodeProcess]
    private void process(T min, T max, ref T outResult) => outResult = GetRandom(min, max);

    protected abstract T GetRandom(T min, T max);
}

[Node("Random Int", "Math/Random")]
public sealed class RandomIntNode : RandomNode<int>
{
    protected override int GetRandom(int min, int max) => Random.Shared.Next(min, max + 1);
}

[Node("Random Float", "Math/Random")]
public sealed class RandomFloatNode : RandomNode<float>
{
    protected override float GetRandom(float min, float max) => Interpolation.Map(Random.Shared.NextSingle(), 0, 1, min, max);
}