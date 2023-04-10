// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using NuGet.Packaging;
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
    public readonly BindableList<ClipVariable> AvailableVariables = new();
    public readonly BindableList<ClipState> States = new();
    public readonly BindableList<ClipEvent> Events = new();
    public readonly Bindable<int> Start = new();
    public readonly Bindable<int> End = new(30);
    public int Length => End.Value - Start.Value;

    private readonly ChatBoxManager chatBoxManager;

    public Clip(ChatBoxManager chatBoxManager)
    {
        this.chatBoxManager = chatBoxManager;
        AssociatedModules.BindCollectionChanged((_, e) => onAssociatedModulesChanged(e), true);
    }

    public void Update()
    {
        chatBoxManager.ModuleEvents.ForEach(moduleEvent =>
        {
            var (module, lookup) = moduleEvent;
            var clipEvent = Events[module][lookup];
            ModuleEvents.Add(DateTimeOffset.Now + TimeSpan.FromSeconds(clipEvent.Length.Value));
        });

        ModuleEvents.ForEach(moduleEvent =>
        {
            if (moduleEvent <= DateTimeOffset.Now) ModuleEvents.Remove(moduleEvent);
        });
    }

    public bool Evalulate()
    {
        if (!Enabled.Value) return false;
        if (Start.Value > chatBoxManager.CurrentSecond || End.Value <= chatBoxManager.CurrentSecond) return false;
        if (ModuleEvents.Any()) return true;

        var localStates = States.Select(state => state.Copy()).ToList();
        removeDisabledModules(localStates);
        removeLessCompoundedStates(localStates);
        removeInvalidStates(localStates);

        Debug.Assert(localStates.Count == 1);

        var chosenState = localStates.First();
        return chosenState.Enabled.Value;
    }

    private void removeDisabledModules(List<ClipState> localStates)
    {
        var statesToRemove = new List<ClipState>();

        foreach (ClipState clipState in localStates)
        {
            var stateValid = clipState.ModuleNames.All(moduleName => chatBoxManager.ModuleEnabledStore[moduleName]);
            if (!stateValid) statesToRemove.Add(clipState);
        }

        statesToRemove.ForEach(moduleName => localStates.Remove(moduleName));
    }

    private void removeLessCompoundedStates(List<ClipState> localStates)
    {
        var enabledAndAssociatedModules = AssociatedModules.Where(moduleName => chatBoxManager.ModuleEnabledStore[moduleName]).ToList();
        enabledAndAssociatedModules.Sort();

        var statesToRemove = new List<ClipState>();

        localStates.ForEach(clipState =>
        {
            var clipStateModules = clipState.ModuleNames;
            clipStateModules.Sort();

            if (!clipStateModules.SequenceEqual(enabledAndAssociatedModules)) statesToRemove.Add(clipState);
        });

        statesToRemove.ForEach(clipState => localStates.Remove(clipState));
    }

    private void removeInvalidStates(List<ClipState> localStates)
    {
        var currentStates = AssociatedModules.Where(moduleName => chatBoxManager.ModuleEnabledStore[moduleName]).Select(moduleName => chatBoxManager.ModuleStates[moduleName]).ToList();
        currentStates.Sort();

        var statesToRemove = new List<ClipState>();

        localStates.ForEach(clipState =>
        {
            var clipStateStates = clipState.StateNames;
            clipStateStates.Sort();

            if (!clipStateStates.SequenceEqual(currentStates)) statesToRemove.Add(clipState);
        });

        statesToRemove.ForEach(clipState => localStates.Remove(clipState));
    }

    public void GetFormat()
    {
        // return events if one exists before returning chosen state
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
            AvailableVariables.AddRange(chatBoxManager.Variables[module].Values.ToList());
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
        foreach (string moduleName in e.NewItems!)
        {
            var statesPrevious = States.Select(clipState => clipState.Copy()).ToList();

            var states = chatBoxManager.States[moduleName];

            foreach (var (stateName, clipState) in states)
            {
                var statesPreviousLocal = statesPrevious.Select(clipStateLocal => clipStateLocal.Copy()).ToList();

                statesPreviousLocal.ForEach(localState =>
                {
                    localState.States.Add((moduleName, stateName));
                });

                States.AddRange(statesPreviousLocal);
                States.Add(clipState);
            }
        }
    }

    private void populateEvents(NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (string newModule in e.NewItems)
            {
                if (chatBoxManager.Events.TryGetValue(newModule, out var events))
                {
                    Events.Add(newModule, new Dictionary<string, ClipEvent>());

                    foreach (var (key, value) in events)
                    {
                        Events[newModule][key] = value;
                    }
                }
            }
        }

        if (e.OldItems is not null)
        {
            foreach (string oldModule in e.OldItems)
            {
                Events.Remove(oldModule);
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
