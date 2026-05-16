// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.Nodes.Types.Speech;

[Node("Speech Source", "Speech")]
public sealed class SpeechSourceNode() : ValueSourceNode<string?>("Text")
{
    protected override string? ComputeValue(IPulseContext c) => ContainingGraph.CurrentSpeechText;
}