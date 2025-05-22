// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow.Triggers;

[Node("Fire On Change", "Flow")]
public class FireOnChangeNode<T> : Node
{
    public FlowCall OnChange = new("On Change");

    [NodeReactive]
    public ValueInput<T> Input = new();

    public GlobalStore<T> Value = new();

    protected override void Process(PulseContext c)
    {
        var input = Input.Read(c);

        if (!EqualityComparer<T>.Default.Equals(input, Value.Read(c)))
        {
            Value.Write(input, c);
            OnChange.Execute(c);
        }
    }
}