// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using VRCOSC.App.ChatBox.Clips;
using VRCOSC.App.ChatBox.Clips.Variables;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.ChatBox.Serialisation;

public class SerialisableChatBox : SerialisableVersion
{
    [JsonProperty("timeline")]
    public SerialisableTimeline Timeline = null!;

    [JsonConstructor]
    public SerialisableChatBox()
    {
    }

    public SerialisableChatBox(ChatBoxManager chatBoxManager)
    {
        Version = 1;

        Timeline = new SerialisableTimeline(chatBoxManager.Timeline);
    }
}

public class SerialisableTimeline
{
    [JsonProperty("length")]
    public int Length;

    [JsonProperty("layers")]
    public List<SerialisableLayer> Layers = new();

    [JsonConstructor]
    public SerialisableTimeline()
    {
    }

    public SerialisableTimeline(Timeline timeline)
    {
        Length = timeline.LengthSeconds;
        Layers = timeline.Layers.Where(layer => layer.Clips.Any()).Select(layer => new SerialisableLayer(layer)).ToList();
    }
}

public class SerialisableLayer
{
    [JsonProperty("enabled")]
    public bool Enabled;

    [JsonProperty("clips")]
    public List<SerialisableClip> Clips = new();

    [JsonConstructor]
    public SerialisableLayer()
    {
    }

    public SerialisableLayer(Layer layer)
    {
        Enabled = layer.Enabled.Value;
        Clips = layer.Clips.Select(clip => new SerialisableClip(clip)).ToList();
    }
}

public class SerialisableClip
{
    [JsonProperty("enabled")]
    public bool Enabled;

    [JsonProperty("name")]
    public string Name = "UNSET";

    [JsonProperty("start")]
    public int Start;

    [JsonProperty("end")]
    public int End;

    [JsonProperty("linked_modules")]
    public List<string> LinkedModules = new();

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
        Start = clip.Start.Value;
        End = clip.End.Value;
        LinkedModules = clip.LinkedModules.ToList();
        States = clip.States.Where(clipState => !clipState.IsDefault).Select(clipState => new SerialisableClipState(clipState)).ToList();
        Events = clip.Events.Where(clipEvent => !clipEvent.IsDefault).Select(clipEvent => new SerialisableClipEvent(clipEvent)).ToList();
    }
}

public abstract class SerialisableClipElement
{
    [JsonProperty("enabled")]
    public bool Enabled;

    [JsonProperty("format")]
    public string Format = string.Empty;

    [JsonProperty("variables")]
    public List<SerialisableClipVariable> Variables = new();

    protected SerialisableClipElement()
    {
    }

    protected SerialisableClipElement(ClipElement clipElement)
    {
        Enabled = clipElement.Enabled.Value;
        Format = clipElement.Format.Value;
        Variables = clipElement.Variables.Select(clipVariable => new SerialisableClipVariable(clipVariable)).ToList();
    }
}

public class SerialisableClipState : SerialisableClipElement
{
    [JsonProperty("states")]
    public Dictionary<string, string> States = new();

    [JsonConstructor]
    public SerialisableClipState()
    {
    }

    public SerialisableClipState(ClipState clipState)
        : base(clipState)
    {
        States = clipState.States;
    }
}

public class SerialisableClipEvent : SerialisableClipElement
{
    [JsonProperty("module_id")]
    public string ModuleID = string.Empty;

    [JsonProperty("event_id")]
    public string EventID = string.Empty;

    [JsonProperty("length")]
    public float Length;

    [JsonProperty("behaviour")]
    public ClipEventBehaviour Behaviour;

    [JsonConstructor]
    public SerialisableClipEvent()
    {
    }

    public SerialisableClipEvent(ClipEvent clipEvent)
        : base(clipEvent)
    {
        ModuleID = clipEvent.ModuleID;
        EventID = clipEvent.EventID;
        Length = clipEvent.Length.Value;
        Behaviour = clipEvent.Behaviour.Value;
    }
}

public class SerialisableClipVariable
{
    [JsonProperty("module_id")]
    public string ModuleID = string.Empty;

    [JsonProperty("variable_id")]
    public string VariableID = string.Empty;

    [JsonProperty("options")]
    public Dictionary<string, object?> Options = new();

    [JsonConstructor]
    public SerialisableClipVariable()
    {
    }

    public SerialisableClipVariable(ClipVariable clipVariable)
    {
        ModuleID = clipVariable.ModuleID;
        VariableID = clipVariable.VariableID;

        if (!clipVariable.IsDefault())
            Options = getVariableOptionAttributes(clipVariable, clipVariable.GetType());
    }

    private Dictionary<string, object?> getVariableOptionAttributes(ClipVariable instance, Type? type)
    {
        var options = new Dictionary<string, object?>();

        if (type is null) return options;

        options.AddRange(type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                             .Where(propertyInfo => propertyInfo.GetCustomAttribute<ClipVariableOptionAttribute>() is not null)
                             .Select(propertyInfo => new KeyValuePair<string, object?>(propertyInfo.GetCustomAttribute<ClipVariableOptionAttribute>()!.SerialisedName, propertyInfo.GetValue(instance))));

        return options;
    }
}
