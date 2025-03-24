// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.SDK.Nodes.Types;

[Node("Print")]
[NodeFlowInput]
[NodeValueInput("String")]
public class PrintNode : Node
{
    [NodeProcess]
    private void process(string str)
    {
        Console.WriteLine(str);
    }
}