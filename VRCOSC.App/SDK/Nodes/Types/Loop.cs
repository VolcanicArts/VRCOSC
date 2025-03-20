// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;

namespace VRCOSC.App.SDK.Nodes.Types;

public class WhileNode : Node
{
    public WhileNode(NodeField nodeField)
        : base(nodeField)
    {
    }

    [NodeProcessLoop(0, 1)]
    private Func<int, bool> runLoop()
    {
        return i => i == 2;
    }
}

[NodeValue([typeof(object)])]
public class ElementAtNode : Node
{
    public ElementAtNode(NodeField nodeField)
        : base(nodeField)
    {
    }

    [NodeProcess]
    private void process(IEnumerable<object> enumerable, int index)
    {
        SetOutputValue(0, enumerable.ElementAt(index));
    }
}