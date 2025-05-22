// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Math.Utility;

[Node("Is Between", "Math/Utility")]
public class IsBetweenNode<T> : Node where T : INumber<T>
{
    public ValueInput<float> Value = new();
    public ValueInput<float> Min = new();
    public ValueInput<float> Max = new();
    public ValueOutput<bool> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(Value.Read(c) >= Min.Read(c) && Value.Read(c) <= Max.Read(c), c);
    }
}