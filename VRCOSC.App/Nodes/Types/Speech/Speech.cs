// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Speech;

[Node("Speech Source", "Speech")]
public sealed class SpeechSourceNode : UpdateNode<string>
{
    public ValueOutput<string> Text = new();

    protected override Task Process(PulseContext c)
    {
        Text.Write(c.GetSpeechText(), c);
        return Task.CompletedTask;
    }

    protected override Task<string> GetValue(PulseContext c) => Task.FromResult(c.GetSpeechText());
}