// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using VRCOSC.Game.ChatBox.Clips;

namespace VRCOSC.Game.ChatBox;

public class ChatBoxManager
{
    public readonly BindableList<Clip> Clips = new();

    public readonly Dictionary<string, Dictionary<string, ClipVariable>> Variables = new();
    public readonly Dictionary<string, Dictionary<string, ClipState>> States = new();
    public readonly Dictionary<string, Dictionary<string, ClipEvent>> Events = new();

    public readonly Dictionary<string, string> ModuleStates = new();

    // TODO - module events will become true for 1 update check and then reset to false
    // Each clip evaluation that checks if an event has occured will have to locally store a DateTimeOffset
    public readonly Dictionary<string, Dictionary<string, bool>> ModuleEvents = new();

    public readonly Bindable<TimeSpan> TimelineLength = new(TimeSpan.FromMinutes(1));

    public float Resolution => 1f / (float)TimelineLength.Value.TotalSeconds;

    public ChatBoxManager()
    {
        for (var i = 0; i < 6; i++)
        {
            Clip clip;

            Clips.Add(clip = new Clip(this)
            {
                Priority = { Value = i },
            });
            clip.AssociatedModules.Add("Test " + i);
        }
    }

    public Clip CreateClip() => new(this);

    public void Initialise()
    {
    }

    public void Update()
    {
        // evaluate clips

        // reset module event triggers
    }

    public bool IncreasePriority(Clip clip) => SetPriority(clip, clip.Priority.Value + 1);
    public bool DecreasePriority(Clip clip) => SetPriority(clip, clip.Priority.Value - 1);

    public bool SetPriority(Clip clip, int priority)
    {
        if (priority is > 5 or < 0) return false;
        if (RetrieveClipsWithPriority(priority).Any(clip.Intersects)) return false;

        clip.Priority.Value = priority;
        return true;
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

        if (Variables.TryGetValue(module, out var innerDict))
        {
            innerDict.Add(lookup, variable);
        }
        else
        {
            Variables.Add(module, new Dictionary<string, ClipVariable>());
            Variables[module].Add(lookup, variable);
        }
    }

    public void SetVariable(string module, string lookup, string? value)
    {
        Variables[module][lookup].Value = value;
    }

    public void RegisterState(string module, string lookup, string name, string defaultFormat)
    {
        var state = new ClipState
        {
            Lookup = lookup,
            Name = name,
            Format = { Value = defaultFormat }
        };

        if (States.TryGetValue(module, out var innerDict))
        {
            innerDict.Add(lookup, state);
        }
        else
        {
            States.Add(module, new Dictionary<string, ClipState>());
            States[module].Add(lookup, state);
        }

        if (!ModuleStates.TryAdd(module, lookup))
        {
            ModuleStates[module] = lookup;
        }
    }

    public void ChangeStateTo(string module, string lookup)
    {
        ModuleStates[module] = lookup;
    }

    public void RegisterEvent(string module, string lookup, string name, string defaultFormat, int defaultLength)
    {
        var clipEvent = new ClipEvent
        {
            Lookup = lookup,
            Name = name,
            Format = { Value = defaultFormat },
            Length = { Value = defaultLength }
        };

        if (Events.TryGetValue(module, out var innerDict))
        {
            innerDict.Add(lookup, clipEvent);
        }
        else
        {
            Events.Add(module, new Dictionary<string, ClipEvent>());
            Events[module].Add(lookup, clipEvent);
        }

        if (ModuleEvents.TryGetValue(module, out var innerDict2))
        {
            innerDict2[lookup] = false;
        }
        else
        {
            ModuleEvents.Add(module, new Dictionary<string, bool>());

            if (ModuleEvents[module].ContainsKey(lookup))
            {
                ModuleEvents[module][lookup] = false;
            }
            else
            {
                ModuleEvents[module].Add(lookup, false);
            }
        }
    }

    public void TriggerEvent(string module, string lookup)
    {
        ModuleEvents[module][lookup] = true;
    }

    public IReadOnlyList<Clip> RetrieveClipsWithPriority(int priority) => Clips.Where(clip => clip.Priority.Value == priority).ToList();
}
