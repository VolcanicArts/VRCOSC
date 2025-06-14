// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Math;

[Node("Ceil", "Math")]
[NodeCollapsed]
public class CeilNode<T> : Node where T : IFloatingPoint<T>
{
    public ValueInput<T> Input = new();
    public ValueOutput<T> Output = new();

    protected override void Process(PulseContext c)
    {
        Output.Write(T.Ceiling(Input.Read(c)), c);
    }
}