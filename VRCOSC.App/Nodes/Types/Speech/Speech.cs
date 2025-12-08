// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Speech;

[Node("Speech Source", "Speech")]
public sealed class SpeechSourceNode : Node, INodeEventHandler
{
    private readonly GlobalStore<string> textStore = new();

    public ValueOutput<string> Text = new();

    protected override Task Process(PulseContext c)
    {
        Text.Write(textStore.Read(c), c);
        return Task.CompletedTask;
    }

    public Task<bool> HandlePartialSpeechResult(PulseContext c, string result)
    {
        if (result == textStore.Read(c)) return Task.FromResult(false);

        textStore.Write(result, c);
        return Task.FromResult(true);
    }

    public Task<bool> HandleFinalSpeechResult(PulseContext c, string result)
    {
        if (result == textStore.Read(c)) return Task.FromResult(false);

        textStore.Write(result, c);
        return Task.FromResult(true);
    }
}