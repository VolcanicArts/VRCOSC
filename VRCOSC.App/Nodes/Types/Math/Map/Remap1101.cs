// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Math.Map;

[Node("Remap -1,1 To 0,1", "Math/Map")]
[NodeCollapsed]
public class Remap1101Node<T> : Node where T : IFloatingPoint<T>
{
    public ValueInput<T> Value = new();
    public ValueOutput<T> Result = new();

    protected override void Process(PulseContext c)
    {
        var value = double.CreateChecked(Value.Read(c));

        Result.Write(T.CreateChecked(Utils.Interpolation.Map(value, -1d, 1d, 0d, 1d)), c);
    }
}