// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Collections;

[Node("Construct List", "Collections")]
public class ConstructListNode<T> : Node
{
    public ValueInputList<T> Items = new();
    public ValueOutput<List<T>> List = new();

    protected override void Process(PulseContext c)
    {
        List.Write(Items.Read(c).ToList(), c);
    }
}