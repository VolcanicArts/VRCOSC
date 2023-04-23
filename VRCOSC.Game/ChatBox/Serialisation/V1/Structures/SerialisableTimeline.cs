// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using Newtonsoft.Json;
using VRCOSC.Game.ChatBox.Clips;

namespace VRCOSC.Game.ChatBox.Serialisation.V1.Structures;

public class SerialisableTimeline
{
    [JsonProperty("version")]
    public int Version = 1;

    [JsonProperty("clips")]
    public List<SerialisableClip> Clips = new();

    [JsonConstructor]
    public SerialisableTimeline()
    {
    }

    public SerialisableTimeline(List<Clip> clips)
    {
        clips.ForEach(clip => Clips.Add(new SerialisableClip(clip)));
    }
}
