// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Platform;
using VRCOSC.Game.App;
using VRCOSC.Game.ChatBox;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.ChatBox.Serialisation.V1;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.OSC.VRChat;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Managers;

public class ChatBoxManager
{
    private bool sendEnabled;

    public bool SendEnabled
    {
        get => sendEnabled;
        set
        {
            if (sendEnabled && !value) Clear();
            sendEnabled = value;
        }
    }

    public readonly Bindable<Clip?> SelectedClip = new();

    public BindableList<Clip> Clips = new();

    public readonly Dictionary<string, Dictionary<string, ClipVariableMetadata>> VariableMetadata = new();
    public readonly Dictionary<string, Dictionary<string, ClipStateMetadata>> StateMetadata = new();
    public readonly Dictionary<string, Dictionary<string, ClipEventMetadata>> EventMetadata = new();
    private Bindable<int> sendDelay = null!;
    private VRChatOscClient oscClient = null!;
    private SerialisationManager serialisationManager = null!;

    public readonly Dictionary<(string, string), string?> VariableValues = new();
    public readonly Dictionary<string, string?> StateValues = new();
    public readonly List<(string, string)> TriggeredEvents = new();
    private readonly object triggeredEventsLock = new();

    public readonly Bindable<int> PriorityCount = new(8);
    public readonly Bindable<TimeSpan> TimelineLength = new();
    public int TimelineLengthSeconds => (int)TimelineLength.Value.TotalSeconds;
    public float TimelineResolution => 1f / (float)TimelineLength.Value.TotalSeconds;

    public float CurrentPercentage => ((DateTimeOffset.Now - startTime).Ticks % TimelineLength.Value.Ticks) / (float)TimelineLength.Value.Ticks;
    public int CurrentSecond => (int)Math.Floor((DateTimeOffset.Now - startTime).TotalSeconds) % (int)TimelineLength.Value.TotalSeconds;
    private bool sendAllowed => nextValidTime <= DateTimeOffset.Now;

    private AppManager appManager = null!;
    private DateTimeOffset startTime;
    private DateTimeOffset nextValidTime;
    private bool isClear;

    public void Initialise(Storage storage, AppManager appManager, VRChatOscClient oscClient, NotificationContainer notification, Bindable<int> sendDelay)
    {
        this.appManager = appManager;
        this.oscClient = oscClient;
        this.sendDelay = sendDelay;
        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(1, new TimelineSerialiser(storage, notification, appManager));
    }

    public void Load()
    {
        setDefaults();
        Deserialise();
        bindAttributes();
    }

    private void setDefaults()
    {
        TimelineLength.Value = TimeSpan.FromMinutes(1);
        Clips.ReplaceItems(DefaultTimeline.GenerateDefaultTimeline(this));
    }

    private void bindAttributes()
    {
        TimelineLength.BindValueChanged(_ => Serialise());
        Clips.BindCollectionChanged((_, _) => Serialise());
        Clips.ForEach(clip => clip.Load());
    }

    public void Import(string filePath)
    {
        SelectedClip.Value = null;
        serialisationManager.Deserialise(filePath);
        bindAttributes();
    }

    public void ResetTimeline()
    {
        setDefaults();
        Serialise();
    }

    public void Deserialise()
    {
        if (!serialisationManager.Deserialise()) return;

        Serialise();
    }

    public void Serialise()
    {
        serialisationManager.Serialise();
    }

    public void Start()
    {
        sendEnabled = true;
        startTime = DateTimeOffset.Now;
        nextValidTime = startTime;
        isClear = true;

        Clips.ForEach(clip => clip.Initialise());

        foreach (var pair in VariableValues)
        {
            VariableValues[pair.Key] = null;
        }

        foreach (var pair in StateValues)
        {
            StateValues[pair.Key] = null;
        }

        lock (triggeredEventsLock)
        {
            TriggeredEvents.Clear();
        }
    }

    public Clip CreateClip()
    {
        var newClip = new Clip();
        newClip.InjectDependencies(appManager);
        return newClip;
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

    public void Teardown()
    {
        lock (triggeredEventsLock) { TriggeredEvents.Clear(); }

        SetTyping(false);
        Clear();
        VariableValues.Clear();
        StateValues.Clear();
    }

    public void IncreaseTime(int amount)
    {
        var newTime = TimelineLength.Value + TimeSpan.FromSeconds(amount);
        SetTimelineLength(newTime);
    }

    public void DecreaseTime(int amount)
    {
        var newTime = TimelineLength.Value - TimeSpan.FromSeconds(amount);
        SetTimelineLength(newTime);
    }

    public void SetTimelineLength(TimeSpan newTime)
    {
        if (newTime.TotalSeconds < 1) newTime = TimeSpan.FromSeconds(1);
        if (newTime.TotalSeconds > 4 * 60) newTime = TimeSpan.FromSeconds(4 * 60);

        TimelineLength.Value = newTime;
    }

    private void evaluateClips()
    {
        var validClip = getValidClip();
        handleClip(validClip);
        nextValidTime += TimeSpan.FromMilliseconds(sendDelay.Value);
    }

    private Clip? getValidClip()
    {
        for (var i = PriorityCount.Value - 1; i >= 0; i--)
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
        if (!sendEnabled) return;

        if (clip is null)
        {
            if (!isClear) Clear();
            return;
        }

        isClear = false;
        sendText(clip.GetFormattedText());
    }

    private void sendText(string text)
    {
        var finalText = convertSpecialCharacters(text);
        oscClient.SendValues(VRChatOscConstants.ADDRESS_CHATBOX_INPUT, new List<object> { finalText, true, false });
    }

    private static string convertSpecialCharacters(string input)
    {
        const int required_width = 64;

        input = Regex.Replace(input, @"/v", "\v");

        return Regex.Replace(input, @"/n", match =>
        {
            var spaces = match.Index == 0 ? 0 : (match.Index - input.LastIndexOf(@"/n", match.Index, StringComparison.Ordinal)) % required_width;
            var spaceCount = required_width - spaces;
            return spaces < 0 || spaceCount < 0 ? string.Empty : new string(' ', spaceCount);
        });
    }

    public void Clear()
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
        if (priority > PriorityCount.Value - 1 || priority < 0) return;
        if (Clips.Where(other => other.Priority.Value == priority).Any(clip.Intersects)) return;

        clip.Priority.Value = priority;
    }

    public void DeleteClip(Clip clip)
    {
        Clips.Remove(clip);
        if (SelectedClip.Value == clip) SelectedClip.Value = null;
    }

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

    public void SetVariable(string module, string lookup, string? value, string suffix)
    {
        var finalLookup = string.IsNullOrEmpty(suffix) ? lookup : $"{lookup}_{suffix}";
        VariableValues[(module, finalLookup)] = value;
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

        StateValues.TryAdd(module, lookup);
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
