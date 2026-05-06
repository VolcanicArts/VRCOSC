// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

#if DEBUG
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Debug;

[Node("Log", "Debug")]
public sealed class LogNode : ActionNode
{
    public ValueInput<string> Text = new();

    protected override void DoAction(PulseContext c)
    {
        var text = Text.Read(c);
        if (text is not null) Logger.Log(text, LoggingTarget.Information);
    }
}
#endif