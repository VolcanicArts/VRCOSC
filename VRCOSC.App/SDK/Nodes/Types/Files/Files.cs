// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
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
    private async Task<int> process
    (
        [NodeValue("Text")] string? text,
        [NodeValue("File Path")] string? filePath
    )
    {
        if (text is null || filePath is null) return -1;
        if (!File.Exists(filePath)) return 1;

        try
        {
            await File.WriteAllTextAsync(filePath, text);
            return 0;
        }
        catch (Exception)
        {
            return 1;
        }
    }
}