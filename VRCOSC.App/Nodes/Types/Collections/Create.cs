// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Collections;

[Node("Construct List", "Collections")]
public class ConstructListNode<T> : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Items")] [NodeVariableSize] T[] items,
        [NodeValue("List")] Ref<List<T>> outList
    )
    {
        outList.Value = items.ToList();
    }
}