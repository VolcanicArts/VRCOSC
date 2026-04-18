// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Inputs;

[Node("Value")]
public class ValueNode<T> : ValueComputeNode<T>
{
    [NodeProperty("value")]
    public T Value
    {
        get;
        set
        {
            field = value;
            NodeGraph.TriggerTree(this).Forget();
        }
    } = default!;

    protected override T ComputeValue(PulseContext c) => Value;
}