// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;

namespace VRCOSC.App.SDK.Nodes.Types;

[Node("While")]
public class WhileNode : Node
{
    [NodeProcessLoop(0, 1)]
    private Func<int, bool> runLoop()
    {
        return i => i == 2;
    }
}

[Node("Element At")]
[NodeValue([typeof(object)])]
[NodeInputs("Enumerable", "Index")]
public class ElementAtNode : Node
{
    [NodeProcess]
    private void process(IEnumerable<object> enumerable, int index)
    {
        SetOutput(0, enumerable.ElementAt(index));
    }
}