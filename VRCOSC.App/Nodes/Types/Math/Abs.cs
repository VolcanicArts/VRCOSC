// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Math;

[Node("Abs", "Math")]
[NodeCollapsed]
public class AbsoluteNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> Input = new();
    public ValueOutput<T> Output = new();

    protected override void Process(PulseContext c)
    {
        Output.Write(T.Abs(Input.Read(c)), c);
    }
}