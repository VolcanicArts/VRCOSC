// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Math.Random;

public abstract class RandomNode<T> : Node where T : notnull
{
    [NodeProcess]
    private void process(T min, T max, Ref<T> result) => result.Value = GetRandom(min, max);

    protected abstract T GetRandom(T min, T max);
}