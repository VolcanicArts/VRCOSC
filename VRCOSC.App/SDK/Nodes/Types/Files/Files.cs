// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace VRCOSC.App.SDK.Nodes.Types.Files;

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
        CancellationToken token,
        [NodeValue("Text")] string? text,
        [NodeValue("File Path")] string? filePath
    )
    {
        if (text is null || filePath is null) return;

        if (!File.Exists(filePath))
        {
            await TriggerFlow(token, 1);
            return;
        }

        try
        {
            await File.WriteAllTextAsync(filePath, text, token);
            if (token.IsCancellationRequested) return;

            await TriggerFlow(token, 0);
        }
        catch (Exception)
        {
            if (token.IsCancellationRequested) return;

            await TriggerFlow(token, 1);
        }
    }
}