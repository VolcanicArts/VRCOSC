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
    public readonly BindableList<Clip> Clips = new();

    public readonly Dictionary<string, Dictionary<string, ClipVariable>> Variables = new();
    public readonly Dictionary<string, Dictionary<string, ClipState>> States = new();
    public readonly Dictionary<string, Dictionary<string, ClipEvent>> Events = new();

    public readonly Dictionary<(string, string), string?> ModuleVariables = new();
    public readonly Dictionary<string, string> ModuleStates = new();
    public readonly List<(string, string)> ModuleEvents = new();
    public IReadOnlyDictionary<string, bool> ModuleEnabledStore => moduleEnabledStore;

    public readonly Bindable<TimeSpan> TimelineLength = new(TimeSpan.FromMinutes(1));

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

    public float Resolution => 1f / (float)TimelineLength.Value.TotalSeconds;
    public int CurrentSecond => (int)Math.Floor((DateTimeOffset.Now - startTime).TotalSeconds) % (int)TimelineLength.Value.TotalSeconds;

    private DateTimeOffset startTime;
    private Dictionary<string, bool> moduleEnabledStore = null!;
    private bool isClear = true;

    public ChatBoxManager()
    {
        for (var i = 0; i < 6; i++)
        {
            Clip clip;

            Clips.Add(clip = new Clip(this)
            {
                Priority = { Value = i }
            });
        }
    }

    public Clip CreateClip() => new(this);

    public void Initialise(Dictionary<string, bool> moduleEnabled)
    {
        startTime = DateTimeOffset.Now;
        moduleEnabledStore = moduleEnabled;

        Clips.ForEach(clip => clip.Initialise());
    }

    public void Update()
    {
        Clips.ForEach(clip => clip.Update());

        Clip? validClip = null;

        for (var i = 5; i >= 0; i--)
        {
            Clips.Where(clip => clip.Priority.Value == i).ForEach(clip =>
            {
                if (!clip.Evalulate() || validClip is not null) return;

                validClip = clip;
            });

            if (validClip is not null) break;
        }

        handleClip(validClip);

        ModuleEvents.Clear();
    }

    public void Shutdown()
    {
        ModuleEvents.Clear();
        ModuleVariables.Clear();
        ModuleStates.Clear();
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
        ModuleVariables[(module, lookup)] = value;
    }

    public void RegisterState(string module, string lookup, string name, string defaultFormat)
    {
        var state = new ClipState
        {
            States = new List<(string, string)> { (module, lookup) },
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
            Module = module,
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
    }

    public void TriggerEvent(string module, string lookup)
    {
        ModuleEvents.Add((module, lookup));
    }

    public IReadOnlyList<Clip> RetrieveClipsWithPriority(int priority) => Clips.Where(clip => clip.Priority.Value == priority).ToList();
}
