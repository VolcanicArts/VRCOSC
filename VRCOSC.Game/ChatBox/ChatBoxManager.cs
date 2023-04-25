// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Platform;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.ChatBox.Serialisation.V1;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Modules;
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
    private NotificationContainer notification = null!;
    private TimelineSerialiser serialiser = null!;

    public readonly Dictionary<(string, string), string?> VariableValues = new();
    public readonly Dictionary<string, string> StateValues = new();
    public readonly List<(string, string)> TriggeredEvents = new();
    private readonly object triggeredEventsLock = new();

    public readonly Bindable<TimeSpan> TimelineLength = new(TimeSpan.FromMinutes(1));
    public int TimelineLengthSeconds => (int)TimelineLength.Value.TotalSeconds;
    public float TimelineResolution => 1f / (float)TimelineLength.Value.TotalSeconds;

    public float CurrentPercentage => ((DateTimeOffset.Now - startTime).Ticks % TimelineLength.Value.Ticks) / (float)TimelineLength.Value.Ticks;
    public int CurrentSecond => (int)Math.Floor((DateTimeOffset.Now - startTime).TotalSeconds) % (int)TimelineLength.Value.TotalSeconds;
    private bool sendAllowed => nextValidTime <= DateTimeOffset.Now;

    public GameManager GameManager = null!;

    private DateTimeOffset startTime;
    private DateTimeOffset nextValidTime;
    private bool isClear;
    private bool isLoaded;

    public void Load(Storage storage, GameManager gameManager, NotificationContainer notification)
    {
        GameManager = gameManager;
        serialiser = new TimelineSerialiser(storage);
        this.notification = notification;

        bool clipDataLoaded;

        if (storage.Exists(@"chatbox.json"))
        {
            clipDataLoaded = loadClipData();
        }
        else
        {
            Clips.AddRange(DefaultTimeline.GenerateDefaultTimeline(this));
            clipDataLoaded = true;
        }

        Clips.BindCollectionChanged((_, _) => Save());

        isLoaded = true;

        if (clipDataLoaded) Save();
    }

    private bool loadClipData()
    {
        try
        {
            var clips = serialiser.Deserialise()?.Clips;

            if (clips is null)
            {
                notification.Notify(new ExceptionNotification("Could not parse ChatBox config. Report on the Discord server"));
                return false;
            }

            clips.ForEach(clip =>
            {
                clip.AssociatedModules.ToImmutableList().ForEach(moduleName =>
                {
                    if (!StateMetadata.ContainsKey(moduleName))
                    {
                        clip.AssociatedModules.Remove(moduleName);

                        clip.States.ToImmutableList().ForEach(clipState =>
                        {
                            clipState.States.RemoveAll(pair => pair.Module == moduleName);
                        });

                        clip.Events.RemoveAll(clipEvent => clipEvent.Module == moduleName);

                        return;
                    }

                    clip.States.ToImmutableList().ForEach(clipState =>
                    {
                        clipState.States.RemoveAll(pair => !StateMetadata[pair.Module].ContainsKey(pair.Lookup));
                    });

                    clip.Events.RemoveAll(clipEvent => !EventMetadata[clipEvent.Module].ContainsKey(clipEvent.Lookup));
                });
            });

            clips.ForEach(clip =>
            {
                var newClip = CreateClip();

                newClip.Enabled.Value = clip.Enabled;
                newClip.Name.Value = clip.Name;
                newClip.Priority.Value = clip.Priority;
                newClip.Start.Value = clip.Start;
                newClip.End.Value = clip.End;

                newClip.AssociatedModules.AddRange(clip.AssociatedModules);

                clip.States.ForEach(clipState =>
                {
                    var stateData = newClip.GetStateFor(clipState.States.Select(state => state.Module), clipState.States.Select(state => state.Lookup));
                    if (stateData is null) return;

                    stateData.Enabled.Value = clipState.Enabled;
                    stateData.Format.Value = clipState.Format;
                });

                clip.Events.ForEach(clipEvent =>
                {
                    var eventData = newClip.GetEventFor(clipEvent.Module, clipEvent.Lookup);
                    if (eventData is null) return;

                    eventData.Enabled.Value = clipEvent.Enabled;
                    eventData.Format.Value = clipEvent.Format;
                    eventData.Length.Value = clipEvent.Length;
                });

                Clips.Add(newClip);
            });

            return true;
        }
        catch (JsonReaderException)
        {
            notification.Notify(new ExceptionNotification("Could not load ChatBox config. Report on the Discord server"));
            return false;
        }
    }

    public void Save()
    {
        if (!isLoaded) return;

        serialiser.Serialise(Clips.ToList());
    }

    public void Initialise(VRChatOscClient oscClient, Bindable<int> sendDelay, Dictionary<string, bool> moduleEnabledCache)
    {
        this.oscClient = oscClient;
        this.sendDelay = sendDelay;
        sendEnabled = true;
        startTime = DateTimeOffset.Now;
        nextValidTime = startTime;
        isClear = true;
        ModuleEnabledCache = moduleEnabledCache;

        Clips.ForEach(clip => clip.Initialise());
    }

    public Clip CreateClip()
    {
        var newClip = new Clip();
        newClip.InjectDependencies(this);
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
        if (!sendEnabled) return;

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
