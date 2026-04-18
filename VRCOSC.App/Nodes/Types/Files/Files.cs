// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Files;

[Node("Write Text To File", "Files")]
public sealed class WriteTextToFileNode : Node, IFlowInput
{
    public FlowContinuation OnSuccess = new();
    public FlowContinuation OnFailed = new();

    public ValueInput<string> FilePath = new();
    public ValueInput<string> Text = new();

    protected override async Task Process(PulseContext c)
    {
        var text = Text.Read(c);
        var filePath = FilePath.Read(c);

        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            await OnFailed.Execute(c);
            return;
        }

        try
        {
            await File.WriteAllTextAsync(filePath, text, c.Token);
            await OnSuccess.Execute(c);
        }
        catch (Exception)
        {
            await OnFailed.Execute(c);
        }
    }
}

[Node("Read Text From File", "Files")]
public sealed class ReadTextFromFileNode : Node, IFlowInput
{
    public FlowContinuation OnSuccess = new();
    public FlowContinuation OnFailed = new();

    public ValueInput<string> FilePath = new();
    public ValueOutput<string> Text = new();

    protected override async Task Process(PulseContext c)
    {
        var filePath = FilePath.Read(c);

        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            await OnFailed.Execute(c);
            return;
        }

        try
        {
            Text.Write(await File.ReadAllTextAsync(filePath, c.Token), c);
            await OnSuccess.Execute(c);
        }
        catch (Exception)
        {
            await OnFailed.Execute(c);
        }
    }
}