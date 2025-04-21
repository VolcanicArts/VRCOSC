// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Nodes.Types.Debug;

[Node("Print", "Debug")]
[NodeFlowInput]
[NodeValueInput("String")]
public class PrintNode : Node
{
    [NodeProcess]
    private void process(string str)
    {
        Logger.Log(str, LoggingTarget.Information);
    }
}