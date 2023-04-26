// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace VRCOSC.Game.ChatBox.Serialisation.V1.Structures;

public class SerialisableTimeline
{
    [JsonProperty("version")]
    public int Version = 1;

    [JsonProperty("length")]
    public long Ticks = TimeSpan.FromMinutes(1).Ticks;

    [JsonProperty("clips")]
    public List<SerialisableClip> Clips = new();

    [JsonConstructor]
    public SerialisableTimeline()
    {
    }

    public SerialisableTimeline(ChatBoxManager chatBoxManager)
    {
        Ticks = chatBoxManager.TimelineLength.Value.Ticks;
        chatBoxManager.Clips.ForEach(clip => Clips.Add(new SerialisableClip(clip)));
    }
}
