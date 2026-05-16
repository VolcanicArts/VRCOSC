// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
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
            ContainingGraph.TriggerTree(this).Forget();
        }
    } = default!;

    public Type Type => typeof(T);

    protected override T ComputeValue(IPulseContext c) => Value;
}