// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.SDK.Nodes.Types;

[NodeValue]
[NodeFlow(false, 0)]
public class PrintNode : Node
{
    public PrintNode(NodeField nodeField)
        : base(nodeField)
    {
    }

    [NodeProcess]
    private void execute(string? inputA)
    {
        Console.WriteLine(inputA);
    }
}