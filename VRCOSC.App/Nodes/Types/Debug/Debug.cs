// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Debug;

[Node("Log", "Debug")]
public sealed class LogNode : Node, IFlowInput
{
    public FlowContinuation Next = new();

    public ValueInput<string> Text = new();

    protected override void Process(PulseContext c)
    {
        var text = Text.Read(c);

        if (string.IsNullOrEmpty(text)) return;

        Console.WriteLine(text);
        Next.Execute(c);
    }
}