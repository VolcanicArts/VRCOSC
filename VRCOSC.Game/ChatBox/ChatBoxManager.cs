// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.OSC.VRChat;

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

    public readonly Bindable<Clip?> SelectedClip = new();

    public readonly BindableList<Clip> Clips = new();

    public readonly Dictionary<string, Dictionary<string, ClipVariableMetadata>> VariableMetadata = new();
    public readonly Dictionary<string, Dictionary<string, ClipStateMetadata>> StateMetadata = new();
    public readonly Dictionary<string, Dictionary<string, ClipEventMetadata>> EventMetadata = new();
    public IReadOnlyDictionary<string, bool> ModuleEnabledCache = null!;
    private Bindable<int> sendDelay = null!;
    private VRChatOscClient oscClient = null!;

    public readonly Dictionary<(string, string), string?> VariableValues = new();
    public readonly Dictionary<string, string> StateValues = new();
    public readonly List<(string, string)> TriggeredEvents = new();
    private readonly object triggeredEventsLock = new();

    public readonly Bindable<TimeSpan> TimelineLength = new(TimeSpan.FromMinutes(1));
    public float TimelineResolution => 1f / (float)TimelineLength.Value.TotalSeconds;

    public int CurrentSecond => (int)Math.Floor((DateTimeOffset.Now - startTime).TotalSeconds) % (int)TimelineLength.Value.TotalSeconds;
    private bool sendAllowed => nextValidTime <= DateTimeOffset.Now;

    private DateTimeOffset startTime;
    private DateTimeOffset nextValidTime;
    private bool isClear;

    public void Load()
    {
        // TODO Check for serialised file. If no file, generate default timeline
        Clips.AddRange(DefaultTimeline.GenerateDefaultTimeline(this));
    }

    public void Initialise(VRChatOscClient oscClient, Bindable<int> sendDelay, Dictionary<string, bool> moduleEnabledCache)
    {
        this.oscClient = oscClient;
        this.sendDelay = sendDelay;
        startTime = DateTimeOffset.Now;
        nextValidTime = startTime;
        isClear = true;
        ModuleEnabledCache = moduleEnabledCache;

        Clips.ForEach(clip => clip.Initialise());
    }

    public void Update()
    {
        lock (triggeredEventsLock)
        {
            Clips.ForEach(clip => clip.Update());
            // Events get handled by clips in the same update cycle they're triggered
            TriggeredEvents.Clear();
        }

        if (sendAllowed) evaluateClips();
    }

    public void Shutdown()
    {
        lock (triggeredEventsLock) { TriggeredEvents.Clear(); }

        VariableValues.Clear();
        StateValues.Clear();
    }

    private void evaluateClips()
    {
        var validClip = getValidClip();
        handleClip(validClip);
        nextValidTime += TimeSpan.FromMilliseconds(sendDelay.Value);
    }

    private Clip? getValidClip()
    {
        for (var i = priority_count - 1; i >= 0; i--)
        {
            foreach (var clip in Clips.Where(clip => clip.Priority.Value == i))
            {
                if (clip.Evalulate()) return clip;
            }
        }

        return null;
    }

    private void handleClip(Clip? clip)
    {
        if (clip is null)
        {
            if (!isClear) clearChatBox();
            return;
        }

        isClear = false;
        sendText(clip.GetFormattedText());
    }

    private void sendText(string text)
    {
        oscClient.SendValues(VRChatOscConstants.ADDRESS_CHATBOX_INPUT, new List<object> { text, true, false });
    }

    private void clearChatBox()
    {
        sendText(string.Empty);
        isClear = true;
    }

    public void SetTyping(bool typing)
    {
        oscClient.SendValue(VRChatOscConstants.ADDRESS_CHATBOX_TYPING, typing);
    }

    public void IncreasePriority(Clip clip) => setPriority(clip, clip.Priority.Value + 1);
    public void DecreasePriority(Clip clip) => setPriority(clip, clip.Priority.Value - 1);

    private void setPriority(Clip clip, int priority)
    {
        if (priority is > priority_count - 1 or < 0) return;
        if (Clips.Where(other => other.Priority.Value == priority).Any(clip.Intersects)) return;

        clip.Priority.Value = priority;
    }

    public void DeleteClip(Clip clip) => Clips.Remove(clip);

    public void RegisterVariable(string module, string lookup, string name, string format)
    {
        var variableMetadata = new ClipVariableMetadata
        {
            Module = module,
            Lookup = lookup,
            Name = name,
            Format = format
        };

        if (!VariableMetadata.ContainsKey(module))
        {
            VariableMetadata.Add(module, new Dictionary<string, ClipVariableMetadata>());
        }

        VariableMetadata[module][lookup] = variableMetadata;
    }

    public void SetVariable(string module, string lookup, string? value)
    {
        VariableValues[(module, lookup)] = value;
    }

    public void RegisterState(string module, string lookup, string name, string defaultFormat)
    {
        var stateMetadata = new ClipStateMetadata
        {
            Module = module,
            Lookup = lookup,
            Name = name,
            DefaultFormat = defaultFormat
        };

        if (!StateMetadata.ContainsKey(module))
        {
            StateMetadata.Add(module, new Dictionary<string, ClipStateMetadata>());
        }

        StateMetadata[module][lookup] = stateMetadata;

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
        var eventMetadata = new ClipEventMetadata
        {
            Module = module,
            Lookup = lookup,
            Name = name,
            DefaultFormat = defaultFormat,
            DefaultLength = defaultLength
        };

        if (!EventMetadata.ContainsKey(module))
        {
            EventMetadata.Add(module, new Dictionary<string, ClipEventMetadata>());
        }

        EventMetadata[module][lookup] = eventMetadata;
    }

    public void TriggerEvent(string module, string lookup)
    {
        lock (triggeredEventsLock) { TriggeredEvents.Add((module, lookup)); }
    }
}
