// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace VRCOSC.Game.ChatBox.Clips;

/// <summary>
/// Represents a timespan that contains all information the ChatBox will need for displaying
/// </summary>
public class Clip
{
    public readonly Bindable<bool> Enabled = new(true);
    public readonly Bindable<string> Name = new("New Clip");
    public readonly BindableNumber<int> Priority = new();
    public readonly BindableList<string> AssociatedModules = new();
    public readonly Bindable<int> Start = new();
    public readonly Bindable<int> End = new();
    public readonly BindableList<ClipState> States = new();
    public readonly BindableList<ClipEvent> Events = new();

    public readonly BindableList<ClipVariableMetadata> AvailableVariables = new();
    public int Length => End.Value - Start.Value;
    private ChatBoxManager chatBoxManager = null!;
    private readonly Queue<ClipEvent> eventQueue = new();
    private (ClipEvent, DateTimeOffset)? currentEvent;
    private ClipState? currentState;

    public void InjectDependencies(ChatBoxManager chatBoxManager)
    {
        this.chatBoxManager = chatBoxManager;
        AssociatedModules.BindCollectionChanged((_, e) => onAssociatedModulesChanged(e), true);
        AssociatedModules.BindCollectionChanged((_, _) => chatBoxManager.Save());
        Enabled.BindValueChanged(_ => chatBoxManager.Save());
        Name.BindValueChanged(_ => chatBoxManager.Save());
        Priority.BindValueChanged(_ => chatBoxManager.Save());
        States.BindCollectionChanged((_, _) => chatBoxManager.Save());
        Events.BindCollectionChanged((_, _) => chatBoxManager.Save());

        chatBoxManager.TimelineLength.BindValueChanged(_ =>
        {
            if (chatBoxManager.TimelineLengthSeconds <= Start.Value)
            {
                chatBoxManager.DeleteClip(this);
                return;
            }

            if (chatBoxManager.TimelineLengthSeconds < End.Value) End.Value = chatBoxManager.TimelineLengthSeconds;
        });
    }

    public void Initialise()
    {
        eventQueue.Clear();
        currentEvent = null;
        currentState = null;
    }

    public void Update()
    {
        auditEvents();
        setCurrentEvent();
    }

    public void Save()
    {
        chatBoxManager.Save();
    }

    private void auditEvents()
    {
        chatBoxManager.TriggeredEvents.ForEach(moduleEvent =>
        {
            var (module, lookup) = moduleEvent;

            var clipEvents = Events.Where(clipEvent => clipEvent.Module == module && clipEvent.Lookup == lookup).ToList();
            if (!clipEvents.Any()) return;

            var clipEvent = clipEvents.Single();
            if (!clipEvent.Enabled.Value) return;

            if (currentEvent?.Item1.Module == module)
                // If the new event and current event are from the same module, overwrite the current event
                currentEvent = (clipEvent, DateTimeOffset.Now + TimeSpan.FromSeconds(clipEvent.Length.Value));
            else
                // If the new event and current event are from different modules, queue the new event
                eventQueue.Enqueue(clipEvent);
        });
    }

    private void setCurrentEvent()
    {
        if (currentEvent is not null && currentEvent.Value.Item2 < DateTimeOffset.Now) currentEvent = null;

        if (currentEvent is null && eventQueue.Any())
        {
            var nextEvent = eventQueue.Dequeue();
            currentEvent = (nextEvent, DateTimeOffset.Now + TimeSpan.FromSeconds(nextEvent.Length.Value));
        }
    }

    public ClipEvent? GetEventFor(string module, string lookup)
    {
        try
        {
            return Events.Single(clipEvent => clipEvent.Module == module && clipEvent.Lookup == lookup);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public ClipState? GetStateFor(string module, string lookup) => GetStateFor(new List<string> { module }, new List<string> { lookup });

    public ClipState? GetStateFor(IEnumerable<string> modules, IEnumerable<string> lookups)
    {
        try
        {
            return States.Single(clipState => clipState.ModuleNames.SequenceEqual(modules) && clipState.StateNames.SequenceEqual(lookups));
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public bool Evalulate()
    {
        if (!Enabled.Value) return false;
        if (Start.Value > chatBoxManager.CurrentSecond || End.Value <= chatBoxManager.CurrentSecond) return false;

        if (currentEvent is not null) return true;

        var localStates = States.Select(state => state.Copy(true)).ToList();
        removeDisabledModules(localStates);
        removeLessCompoundedStates(localStates);
        removeInvalidStates(localStates);

        if (localStates.Count != 1) return false;

        var chosenState = localStates.Single();
        if (!chosenState.Enabled.Value) return false;

        currentState = chosenState;
        return true;
    }

    private void removeDisabledModules(List<ClipState> localStates)
    {
        foreach (var clipState in localStates.ToImmutableList())
        {
            var stateValid = clipState.ModuleNames.All(moduleName => chatBoxManager.ModuleEnabledCache[moduleName]);
            if (!stateValid) localStates.Remove(clipState);
        }
    }

    private void removeLessCompoundedStates(List<ClipState> localStates)
    {
        var enabledAndAssociatedModules = AssociatedModules.Where(moduleName => chatBoxManager.ModuleEnabledCache[moduleName]).ToList();
        enabledAndAssociatedModules.Sort();

        foreach (var clipState in localStates.ToImmutableList())
        {
            var clipStateModules = clipState.ModuleNames;
            clipStateModules.Sort();

            if (!clipStateModules.SequenceEqual(enabledAndAssociatedModules)) localStates.Remove(clipState);
        }
    }

    private void removeInvalidStates(List<ClipState> localStates)
    {
        var currentStates = AssociatedModules.Where(moduleName => chatBoxManager.ModuleEnabledCache[moduleName] && chatBoxManager.StateValues.TryGetValue(moduleName, out _)).Select(moduleName => chatBoxManager.StateValues[moduleName]).ToList();
        currentStates.Sort();

        if (!currentStates.Any()) return;

        foreach (var clipState in localStates.ToImmutableList())
        {
            var clipStateStates = clipState.StateNames;
            clipStateStates.Sort();

            if (!clipStateStates.SequenceEqual(currentStates)) localStates.Remove(clipState);
        }
    }

    public string GetFormattedText() => currentEvent is not null ? formatText(currentEvent.Value.Item1) : formatText(currentState!);

    private string formatText(ClipState clipState) => formatText(clipState.Format.Value);
    private string formatText(ClipEvent clipEvent) => formatText(clipEvent.Format.Value);

    private string formatText(string text)
    {
        var returnText = text;

        AvailableVariables.ForEach(clipVariable =>
        {
            if (!chatBoxManager.ModuleEnabledCache[clipVariable.Module]) return;

            chatBoxManager.VariableValues.TryGetValue((clipVariable.Module, clipVariable.Lookup), out var variableValue);

            returnText = returnText.Replace(clipVariable.DisplayableFormat, variableValue ?? string.Empty);
        });

        return returnText;
    }

    private void onAssociatedModulesChanged(NotifyCollectionChangedEventArgs e)
    {
        populateAvailableVariables();
        populateStates(e);
        populateEvents(e);
    }

    private void populateAvailableVariables()
    {
        AvailableVariables.Clear();

        foreach (var module in AssociatedModules)
        {
            AvailableVariables.AddRange(chatBoxManager.VariableMetadata[module].Values);
        }
    }

    private void populateStates(NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null) removeStatesOfRemovedModules(e);
        if (e.NewItems is not null) addStatesOfAddedModules(e);
    }

    private void removeStatesOfRemovedModules(NotifyCollectionChangedEventArgs e)
    {
        foreach (string oldModule in e.OldItems!)
        {
            States.RemoveAll(clipState => clipState.ModuleNames.Contains(oldModule));
        }
    }

    private void addStatesOfAddedModules(NotifyCollectionChangedEventArgs e)
    {
        foreach (string moduleName in e.NewItems!) addStatesOfAddedModule(moduleName);
    }

    private void addStatesOfAddedModule(string moduleName)
    {
        var currentStateCopy = States.Select(clipState => clipState.Copy()).ToList();
        var statesToAdd = chatBoxManager.StateMetadata[moduleName];

        foreach (var (newStateName, newStateMetadata) in statesToAdd)
        {
            var localCurrentStatesCopy = currentStateCopy.Select(clipState => clipState.Copy()).ToList();

            localCurrentStatesCopy.ForEach(newStateLocal =>
            {
                newStateLocal.States.Add((moduleName, newStateName));
                newStateLocal.Enabled.BindValueChanged(_ => chatBoxManager.Save());
                newStateLocal.Format.BindValueChanged(_ => chatBoxManager.Save());
            });

            States.AddRange(localCurrentStatesCopy);
            var singleState = new ClipState(newStateMetadata);
            singleState.Enabled.BindValueChanged(_ => chatBoxManager.Save());
            singleState.Format.BindValueChanged(_ => chatBoxManager.Save());
            States.Add(singleState);
        }
    }

    private void populateEvents(NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null) removeEventsOfRemovedModules(e);
        if (e.NewItems is not null) addEventsOfAddedModules(e);
    }

    private void removeEventsOfRemovedModules(NotifyCollectionChangedEventArgs e)
    {
        foreach (string oldModule in e.OldItems!)
        {
            Events.RemoveAll(clipEvent => clipEvent.Module == oldModule);
        }
    }

    private void addEventsOfAddedModules(NotifyCollectionChangedEventArgs e)
    {
        foreach (string moduleName in e.NewItems!)
        {
            if (!chatBoxManager.EventMetadata.TryGetValue(moduleName, out var events)) continue;

            foreach (var (_, metadata) in events)
            {
                var newEvent = new ClipEvent(metadata);
                newEvent.Enabled.BindValueChanged(_ => chatBoxManager.Save());
                newEvent.Format.BindValueChanged(_ => chatBoxManager.Save());
                newEvent.Length.BindValueChanged(_ => chatBoxManager.Save());
                Events.Add(newEvent);
            }
        }
    }

    public bool Intersects(Clip other)
    {
        if (Start.Value >= other.Start.Value && Start.Value < other.End.Value) return true;
        if (End.Value <= other.End.Value && End.Value > other.Start.Value) return true;
        if (Start.Value < other.Start.Value && End.Value > other.End.Value) return true;

        return false;
    }
}
