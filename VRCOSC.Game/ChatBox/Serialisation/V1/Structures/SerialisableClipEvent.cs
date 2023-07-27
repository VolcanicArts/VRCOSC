// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;
using VRCOSC.Game.ChatBox.Clips;

namespace VRCOSC.Game.ChatBox.Serialisation.V1.Structures;

public class SerialisableClipEvent
{
    [JsonProperty("module")]
    public string Module = null!;

    [JsonProperty("lookup")]
    public string Lookup = null!;

    [JsonProperty("format")]
    public string Format = null!;

    [JsonProperty("enabled")]
    public bool Enabled;

    [JsonProperty("length")]
    public float Length;

    [JsonConstructor]
    public SerialisableClipEvent()
    {
    }

    public SerialisableClipEvent(ClipEvent clipEvent)
    {
        Module = clipEvent.Module;
        Lookup = clipEvent.Lookup;
        Format = clipEvent.Format.Value;
        Enabled = clipEvent.Enabled.Value;
        Length = clipEvent.Length.Value;
    }
}
