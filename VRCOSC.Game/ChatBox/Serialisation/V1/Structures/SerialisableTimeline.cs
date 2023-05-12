// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Extensions.IEnumerableExtensions;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.ChatBox.Serialisation.V1.Structures;

public class SerialisableTimeline
{
    [JsonProperty("version")]
    public int Version;

    [JsonProperty("length")]
    public long Ticks;

    [JsonProperty("clips")]
    public List<SerialisableClip> Clips = new();

    [JsonConstructor]
    public SerialisableTimeline()
    {
    }

    public SerialisableTimeline(ChatBoxManager chatBoxManager)
    {
        Version = 1;
        Ticks = chatBoxManager.TimelineLength.Value.Ticks;
        chatBoxManager.Clips.ForEach(clip => Clips.Add(new SerialisableClip(clip)));
    }
}
