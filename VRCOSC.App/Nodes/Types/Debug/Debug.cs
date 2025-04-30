// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.Parameters;

namespace VRCOSC.App.Nodes.Types.Debug;

[Node("Log", "Debug")]
public sealed class LogNode : Node, IFlowInput, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs => [new()];

    [NodeProcess]
    private async Task process
    (
        CancellationToken token,
        [NodeValue("String")] string? str
    )
    {
        if (string.IsNullOrEmpty(str)) return;

        Console.WriteLine(str);
        await TriggerFlow(token, 0);
    }
}

[Node("Enum Test", "Debug")]
public sealed class EnumValueInputDebugNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Parameter Type")] ParameterType parameterType
    )
    {
    }
}

[Node("String List", "Debug")]
public class StringListOutputDebugNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("String List")] Ref<List<string>> outStringList
    )
    {
        outStringList.Value = ["Test1", "Test2", "Test3"];
    }
}