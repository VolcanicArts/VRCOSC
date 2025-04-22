// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.SDK.Nodes.Types.Debug;

[Node("Log", "Debug")]
public sealed class LogNode : Node
{
    public LogNode()
    {
        AddFlow("*", ConnectionSide.Input);
    }

    [NodeProcess(["Input"], [])]
    private void process(string str)
    {
        Console.WriteLine(str);
    }
}