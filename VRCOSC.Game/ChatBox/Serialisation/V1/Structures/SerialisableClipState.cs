// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using Newtonsoft.Json;
using VRCOSC.Game.ChatBox.Clips;

namespace VRCOSC.Game.ChatBox.Serialisation.V1.Structures;

public class SerialisableClipState
{
    [JsonProperty("states")]
    public List<SerialisableClipStateStates> States = new();

    [JsonProperty("format")]
    public string Format = null!;

    [JsonProperty("enabled")]
    public bool Enabled;

    [JsonConstructor]
    public SerialisableClipState()
    {
    }

    public SerialisableClipState(ClipState clipState)
    {
        clipState.States.ForEach(pair => States.Add(new SerialisableClipStateStates(pair)));
        Format = clipState.Format.Value;
        Enabled = clipState.Enabled.Value;
    }
}

public class SerialisableClipStateStates
{
    [JsonProperty("module")]
    public string Module = null!;

    [JsonProperty("lookup")]
    public string Lookup = null!;

    [JsonConstructor]
    public SerialisableClipStateStates()
    {
    }

    public SerialisableClipStateStates((string, string) pair)
    {
        Module = pair.Item1;
        Lookup = pair.Item2;
    }
}
