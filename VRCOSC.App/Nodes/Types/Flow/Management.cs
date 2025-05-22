// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Value Relay", "Utility")]
public sealed class ValueRelayNode<T> : Node
{
    public ValueInput<T> Input = new();
    public ValueOutput<T> Output = new();

    protected override void Process(PulseContext c)
    {
        Output.Write(Input.Read(c), c);
    }
}