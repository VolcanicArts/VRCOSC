// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Extensions.IEnumerableExtensions;
using VRCOSC.Game.App;

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

    public SerialisableTimeline(AppManager appManager)
    {
        Version = 1;
        Ticks = appManager.ChatBoxManager.TimelineLength.Value.Ticks;
        appManager.ChatBoxManager.Clips.ForEach(clip => Clips.Add(new SerialisableClip(clip)));
    }
}
