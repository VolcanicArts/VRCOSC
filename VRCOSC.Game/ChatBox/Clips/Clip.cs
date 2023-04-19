// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace VRCOSC.Game.ChatBox.Clips;

/// <summary>
/// Represents a timespan that contains all information the ChatBox will need for displaying
/// </summary>
public class Clip
{
    public readonly BindableBool Enabled = new(true);
    public readonly Bindable<string> Name = new("New Clip");
    public readonly BindableNumber<int> Priority = new();
    public readonly BindableList<string> AssociatedModules = new();
    public readonly BindableList<ClipVariableMetadata> AvailableVariables = new();
    public readonly BindableList<ClipState> States = new();
    public readonly BindableList<ClipEvent> Events = new();
    public readonly Bindable<int> Start = new();
    public readonly Bindable<int> End = new(30);
    public int Length => End.Value - Start.Value;

    private readonly ChatBoxManager chatBoxManager;

    // TODO Dumb just store the ClipEvent somehow
    private ((string, string), DateTimeOffset)? currentEvent;
    private ClipState? currentState;

    public Clip(ChatBoxManager chatBoxManager)
    {
        this.chatBoxManager = chatBoxManager;
        AssociatedModules.BindCollectionChanged((_, e) => onAssociatedModulesChanged(e), true);
    }

    public void Initialise()
    {
        currentEvent = null;
        currentState = null;
    }

    public void Update()
    {
        chatBoxManager.TriggeredEvents.ForEach(moduleEvent =>
        {
            var (module, lookup) = moduleEvent;

            // TODO if new event's module name is equal to the current event's module name, it should replace
            // TODO if new event's module name is different, add to a queue to be put into current event when current event expires

            var clipEvent = Events.Where(clipEvent => clipEvent.Module == module && clipEvent.Lookup == lookup);
        });

        if (currentEvent?.Item2 <= DateTimeOffset.Now) currentEvent = null;
    }

    public bool Evalulate()
    {
        if (!Enabled.Value) return false;
        if (Start.Value > chatBoxManager.CurrentSecond || End.Value <= chatBoxManager.CurrentSecond) return false;

        if (currentEvent is not null) return true;

        var localStates = States.Select(state => state.Copy()).ToList();
        removeDisabledModules(localStates);
        removeLessCompoundedStates(localStates);
        removeInvalidStates(localStates);

        Debug.Assert(localStates.Count is 0 or 1);

        if (!localStates.Any()) return false;

        var chosenState = localStates.First();

        currentState = chosenState.Enabled.Value ? chosenState : null;
        return chosenState.Enabled.Value;
    }

    private void removeDisabledModules(List<ClipState> localStates)
    {
        foreach (var clipState in localStates)
        {
            var stateValid = clipState.ModuleNames.All(moduleName => chatBoxManager.ModuleEnabledCache[moduleName]);
            if (!stateValid) localStates.Remove(clipState);
        }
    }

    private void removeLessCompoundedStates(List<ClipState> localStates)
    {
        var enabledAndAssociatedModules = AssociatedModules.Where(moduleName => chatBoxManager.ModuleEnabledCache[moduleName]).ToList();
        enabledAndAssociatedModules.Sort();

        foreach (var clipState in localStates)
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

        foreach (var clipState in localStates)
        {
            var clipStateStates = clipState.StateNames;
            clipStateStates.Sort();

            if (!clipStateStates.SequenceEqual(currentStates)) localStates.Remove(clipState);
        }
    }

    public string GetFormattedText()
    {
        if (currentEvent is not null)
            return formatText(Events.Single(clipEvent => clipEvent.Module == currentEvent.Value.Item1.Item1 && clipEvent.Lookup == currentEvent.Value.Item1.Item2));

        return formatText(currentState!);
    }

    private string formatText(ClipState clipState) => formatText(clipState.Format.Value);
    private string formatText(ClipEvent clipEvent) => formatText(clipEvent.Format.Value);

    private string formatText(string text)
    {
        var returnText = text;

        AvailableVariables.ForEach(clipVariable =>
        {
            var variableValue = chatBoxManager.VariableValues[(clipVariable.Module, clipVariable.Lookup)] ?? string.Empty;
            returnText = returnText.Replace(clipVariable.DisplayableFormat, variableValue);
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
            });

            States.AddRange(localCurrentStatesCopy);
            States.Add(new ClipState(newStateMetadata));
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
                Events.Add(new ClipEvent(metadata));
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
