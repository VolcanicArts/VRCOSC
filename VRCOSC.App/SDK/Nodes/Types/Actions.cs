// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.SDK.Nodes.Types;

[Node("Print")]
[NodeValue]
[NodeFlow(false, 0)]
[NodeInputs("String")]
public class PrintNode : Node
{
    [NodeProcess]
    private void execute(string? str)
    {
        Console.WriteLine(str);
    }
}