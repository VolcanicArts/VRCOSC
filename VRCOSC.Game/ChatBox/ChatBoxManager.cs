// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using VRCOSC.Game.ChatBox.Clips;

namespace VRCOSC.Game.ChatBox;

public class ChatBoxManager
{
    private const int priority_count = 6;

    private bool sendEnabled;

    public bool SendEnabled
    {
        get => sendEnabled;
        set
        {
            if (sendEnabled && !value) clearChatBox();
            sendEnabled = value;
        }
    }

    public readonly BindableList<Clip> Clips = new();

    // TODO Might be best to refactor all this into metadata vs data. The template stuff should be the metadata which gets referenced by all the clips. The stored data for each state and event gets stored in the clips
    // Dictionary<ModuleName, Dictionary<ClipFooLookup, ClipFoo>>
    public readonly Dictionary<string, Dictionary<string, ClipVariable>> TemplateVariables = new();
    public readonly Dictionary<string, Dictionary<string, ClipState>> TemplateStates = new();
    public readonly Dictionary<string, Dictionary<string, ClipEvent>> TemplateEvents = new();
    public IReadOnlyDictionary<string, bool> ModuleEnabledCache = null!;

    // TODO These still get stored here since they're shared between clips for evaluation, but could be abstracted behind some helper methods to make things cleaner
    // Dictionary<ModuleName, Dictionary<VariableLookup, VariableValue>>
    public readonly Dictionary<string, Dictionary<string, string?>> VariableValues = new();

    // Dictionary<ModuleName, ModuleState>
    public readonly Dictionary<string, string> StateValues = new();

    /// <summary>
    /// The events occuring in the current update. Gets cleared after all Clips have been updated to ensure the event is only handled once
    /// </summary>
    // List<ModuleName, OccuredEventLookup>
    public readonly List<(string, string)> TriggeredEvents = new();

    public readonly Bindable<TimeSpan> TimelineLength = new(TimeSpan.FromMinutes(1));
    public float TimelineResolution => 1f / (float)TimelineLength.Value.TotalSeconds;

    public int CurrentSecond => (int)Math.Floor((DateTimeOffset.Now - startTime).TotalSeconds) % (int)TimelineLength.Value.TotalSeconds;

    private DateTimeOffset startTime;
    private bool isClear;

    public ChatBoxManager()
    {
        for (var i = 0; i < priority_count; i++)
        {
            Clips.Add(new Clip(this)
            {
                Priority = { Value = i }
            });
        }
    }

    public Clip CreateClip() => new(this);

    public void Initialise(Dictionary<string, bool> moduleEnabledCache)
    {
        startTime = DateTimeOffset.Now;
        isClear = true;
        ModuleEnabledCache = moduleEnabledCache;

        Clips.ForEach(clip => clip.Initialise());
    }

    public void Update()
    {
        Clips.ForEach(clip => clip.Update());

        Clip? validClip = null;

        for (var i = priority_count - 1; i >= 0; i--)
        {
            Clips.Where(clip => clip.Priority.Value == i).ForEach(clip =>
            {
                if (!clip.Evalulate() || validClip is not null) return;

                validClip = clip;
            });

            if (validClip is not null) break;
        }

        handleClip(validClip);

        TriggeredEvents.Clear();
    }

    public void Shutdown()
    {
        TriggeredEvents.Clear();
        VariableValues.Clear();
        StateValues.Clear();
    }

    private void handleClip(Clip? clip)
    {
        if (clip is null && !isClear)
        {
            clearChatBox();
            return;
        }

        if (clip is null) return;

        // format clip and send to ChatBox
    }

    private void clearChatBox()
    {
        isClear = true;
    }

    public void IncreasePriority(Clip clip) => setPriority(clip, clip.Priority.Value + 1);
    public void DecreasePriority(Clip clip) => setPriority(clip, clip.Priority.Value - 1);

    private void setPriority(Clip clip, int priority)
    {
        if (priority is > priority_count - 1 or < 0) return;
        if (RetrieveClipsWithPriority(priority).Any(clip.Intersects)) return;

        clip.Priority.Value = priority;
    }

    public void RegisterVariable(string module, string lookup, string name, string format)
    {
        var variable = new ClipVariable
        {
            Module = module,
            Lookup = lookup,
            Name = name,
            Format = format
        };

        if (TemplateVariables.TryGetValue(module, out var innerDict))
        {
            innerDict.Add(lookup, variable);
        }
        else
        {
            TemplateVariables.Add(module, new Dictionary<string, ClipVariable>());
            TemplateVariables[module].Add(lookup, variable);
        }
    }

    public void SetVariable(string module, string lookup, string? value)
    {
        if (!VariableValues.ContainsKey(module)) VariableValues.Add(module, new Dictionary<string, string?>());
        VariableValues[module][lookup] = value;
    }

    public void RegisterState(string module, string lookup, string name, string defaultFormat)
    {
        var state = new ClipState
        {
            States = new List<(string, string)> { (module, lookup) },
            Format = { Value = defaultFormat }
        };

        if (TemplateStates.TryGetValue(module, out var innerDict))
        {
            innerDict.Add(lookup, state);
        }
        else
        {
            TemplateStates.Add(module, new Dictionary<string, ClipState>());
            TemplateStates[module].Add(lookup, state);
        }

        if (!StateValues.TryAdd(module, lookup))
        {
            StateValues[module] = lookup;
        }
    }

    public void ChangeStateTo(string module, string lookup)
    {
        StateValues[module] = lookup;
    }

    public void RegisterEvent(string module, string lookup, string name, string defaultFormat, int defaultLength)
    {
        var clipEvent = new ClipEvent
        {
            Module = module,
            Lookup = lookup,
            Name = name,
            Format = { Value = defaultFormat },
            Length = { Value = defaultLength }
        };

        if (TemplateEvents.TryGetValue(module, out var innerDict))
        {
            innerDict.Add(lookup, clipEvent);
        }
        else
        {
            TemplateEvents.Add(module, new Dictionary<string, ClipEvent>());
            TemplateEvents[module].Add(lookup, clipEvent);
        }
    }

    public void TriggerEvent(string module, string lookup)
    {
        TriggeredEvents.Add((module, lookup));
    }

    public IReadOnlyList<Clip> RetrieveClipsWithPriority(int priority) => Clips.Where(clip => clip.Priority.Value == priority).ToList();
}
