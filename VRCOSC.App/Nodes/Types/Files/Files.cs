// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Files;

[Node("Write Text To File", "Files")]
public class WriteTextToFileNode : Node, IFlowInput
{
    public FlowContinuation OnFinished = new("On Finished");
    public FlowContinuation OnFailed = new("On Failed");

    public ValueInput<string> Text = new(string.Empty);
    public ValueInput<string> FilePath = new(string.Empty);

    protected override void Process(PulseContext c)
    {
        var text = Text.Read(c);
        var filePath = FilePath.Read(c);

        if (string.IsNullOrEmpty(filePath)) return;

        if (!File.Exists(filePath))
        {
            OnFailed.Execute(c);
            return;
        }

        try
        {
            File.WriteAllTextAsync(filePath, text, c.Token).Wait(c.Token);
            if (c.IsCancelled) return;

            OnFinished.Execute(c);
        }
        catch (Exception)
        {
            if (c.IsCancelled) return;

            OnFailed.Execute(c);
        }
    }
}