// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Files;

[Node("Write Text To File", "Files")]
public class WriteTextToFileNode : Node, IFlowInput, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs =>
    [
        new("On Write Finished"),
        new("On Write Failed")
    ];

    [NodeProcess]
    private async Task process
    (
        FlowContext context,
        [NodeValue("Text")] string? text,
        [NodeValue("File Path")] string? filePath
    )
    {
        if (text is null || filePath is null) return;

        if (!File.Exists(filePath))
        {
            await TriggerFlow(context, 1);
            return;
        }

        try
        {
            await File.WriteAllTextAsync(filePath, text, context.Token);
            if (context.Token.IsCancellationRequested) return;

            await TriggerFlow(context, 0);
        }
        catch (Exception)
        {
            if (context.Token.IsCancellationRequested) return;

            await TriggerFlow(context, 1);
        }
    }
}