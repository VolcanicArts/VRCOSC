// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Speech;

[Node("On Speech Result", "Speech")]
public sealed class OnSpeechResultNode : Node, INodeEventHandler
{
    private readonly GlobalStore<string> textStore = new();

    public FlowContinuation OnResult = new();

    public ValueOutput<string> Text = new();

    protected override async Task Process(PulseContext c)
    {
        Text.Write(textStore.Read(c), c);
        await OnResult.Execute(c);
        textStore.Write(string.Empty, c);
    }

    public bool HandlePartialSpeechResult(PulseContext c, string result)
    {
        textStore.Write(result, c);
        return true;
    }

    public bool HandleFinalSpeechResult(PulseContext c, string result)
    {
        textStore.Write(result, c);
        return true;
    }
}