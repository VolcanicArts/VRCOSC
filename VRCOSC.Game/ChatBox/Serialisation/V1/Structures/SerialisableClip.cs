// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Extensions.IEnumerableExtensions;
using VRCOSC.Game.ChatBox.Clips;

namespace VRCOSC.Game.ChatBox.Serialisation.V1.Structures;

public class SerialisableClip
{
    [JsonProperty("enabled")]
    public bool Enabled;

    [JsonProperty("name")]
    public string Name = null!;

    [JsonProperty("priority")]
    public int Priority;

    [JsonProperty("associated_modules")]
    public List<string> AssociatedModules = new();

    [JsonProperty("start")]
    public int Start;

    [JsonProperty("end")]
    public int End;

    [JsonProperty("states")]
    public List<SerialisableClipState> States = new();

    [JsonProperty("events")]
    public List<SerialisableClipEvent> Events = new();

    [JsonConstructor]
    public SerialisableClip()
    {
    }

    public SerialisableClip(Clip clip)
    {
        Enabled = clip.Enabled.Value;
        Name = clip.Name.Value;
        Priority = clip.Priority.Value;
        AssociatedModules = clip.AssociatedModules.ToList();
        Start = clip.Start.Value;
        End = clip.End.Value;

        clip.States.ForEach(clipState =>
        {
            if (!clipState.IsDefault) States.Add(new SerialisableClipState(clipState));
        });

        clip.Events.ForEach(clipEvent =>
        {
            if (!clipEvent.IsDefault) Events.Add(new SerialisableClipEvent(clipEvent));
        });
    }
}
