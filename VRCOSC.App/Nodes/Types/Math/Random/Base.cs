// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Math.Random;

public abstract class RandomNode<T> : Node where T : notnull
{
    public ValueInput<T> Min = new();
    public ValueInput<T> Max = new();
    public ValueOutput<T> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(GetRandom(Min.Read(c), Max.Read(c)), c);
    }

    protected abstract T GetRandom(T min, T max);
}