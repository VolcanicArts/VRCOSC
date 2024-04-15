// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using VRCOSC.App.ChatBox.Clips.Variables;
using VRCOSC.App.Modules;
using VRCOSC.App.Utils;

namespace VRCOSC.App.ChatBox.Clips;

public class Clip : INotifyPropertyChanged
{
    public Observable<bool> Enabled { get; } = new(true);
    public Observable<string> Name { get; } = new("New Clip");

    public Observable<int> Start { get; } = new();
    public Observable<int> End { get; } = new();
    public ObservableCollection<string> LinkedModules { get; } = new();

    // TODO: When there's no linked modules there should be a single state letting people type text

    public ObservableCollection<ClipState> States { get; } = new();
    public ObservableCollection<ClipEvent> Events { get; } = new();

    public IEnumerable<ClipState> UIStates => States.OrderBy(clipState => clipState.States.Count)
                                                    .ThenBy(state => string.Join(",", state.States.Keys.OrderBy(k => k)))
                                                    .ThenBy(state => string.Join(",", state.States.Values.OrderBy(v => v)));

    public IEnumerable<ClipEvent> UIEvents => Events.OrderByDescending(clipEvent => clipEvent.ModuleID)
                                                    .ThenByDescending(clipEvent => clipEvent.EventID);

    public Dictionary<string, List<ClipVariableReference>> UIVariables
    {
        get
        {
            var modules = LinkedModules.Select(moduleID => ModuleManager.GetInstance().GetModuleOfID(moduleID)).OrderBy(module => module.Title);
            return modules.ToDictionary(module => module.Title, module => ChatBoxManager.GetInstance().VariableReferences.Where(reference => reference.ModuleID == module.SerialisedName).OrderBy(reference => reference.DisplayName.Value).ToList());
        }
    }

    private readonly Queue<ClipEvent> eventQueue = new();
    private (ClipEvent, DateTimeOffset)? currentEvent;

    private ClipState? currentState;

    public Clip()
    {
        LinkedModules.CollectionChanged += linkedModulesOnCollectionChanged;

        ChatBoxManager.GetInstance().Timeline.Length.Subscribe(_ =>
        {
            if (ChatBoxManager.GetInstance().Timeline.LengthSeconds <= Start.Value)
            {
                ChatBoxManager.GetInstance().Timeline.FindLayerOfClip(this).Clips.Remove(this);
                return;
            }

            if (ChatBoxManager.GetInstance().Timeline.LengthSeconds < End.Value) End.Value = ChatBoxManager.GetInstance().Timeline.LengthSeconds;
        });
    }

    public void ChatBoxStart()
    {
        eventQueue.Clear();
        currentState = null;
        currentEvent = null;

        States.ForEach(clipState => clipState.Variables.ForEach(clipVariable => clipVariable.Start()));
        Events.ForEach(clipEvent => clipEvent.Variables.ForEach(clipVariable => clipVariable.Start()));
    }

    public ClipElement? FindElementFromVariable(ClipVariable variable)
    {
        var clipState = States.FirstOrDefault(clipState => clipState.Variables.Contains(variable));
        if (clipState is not null) return clipState;

        var clipEvent = Events.FirstOrDefault(clipEvent => clipEvent.Variables.Contains(variable));
        if (clipEvent is not null) return clipEvent;

        return null;
    }

    public bool Intersects(Clip other)
    {
        if (Start.Value >= other.Start.Value && Start.Value < other.End.Value) return true;
        if (End.Value <= other.End.Value && End.Value > other.Start.Value) return true;
        if (Start.Value < other.Start.Value && End.Value > other.End.Value) return true;

        return false;
    }

    #region Update

    public void UpdateUIBinds()
    {
        OnPropertyChanged(nameof(Start));
        OnPropertyChanged(nameof(End));
    }

    public void Update()
    {
        auditEvents();
        setCurrentEvent();
    }

    private void auditEvents()
    {
        ChatBoxManager.GetInstance().TriggeredEvents.ForEach(moduleEvent =>
        {
            var (moduleID, eventID) = moduleEvent;

            var clipEvent = Events.SingleOrDefault(clipEvent => clipEvent.ModuleID == moduleID && clipEvent.EventID == eventID);
            if (clipEvent is null || !clipEvent.Enabled.Value) return;

            switch (clipEvent.Behaviour.Value)
            {
                case ClipEventBehaviour.Override:
                    currentEvent = (clipEvent, DateTimeOffset.Now + TimeSpan.FromSeconds(clipEvent.Length.Value));
                    break;

                case ClipEventBehaviour.Queue:
                    eventQueue.Enqueue(clipEvent);
                    break;

                case ClipEventBehaviour.Ignore:
                    currentEvent ??= (clipEvent, DateTimeOffset.Now + TimeSpan.FromSeconds(clipEvent.Length.Value));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(clipEvent.Behaviour));
            }
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

    #endregion

    #region Evaluation

    public bool Evaluate()
    {
        if (!Enabled.Value) return false;

        if (Start.Value > ChatBoxManager.GetInstance().CurrentSecond || End.Value <= ChatBoxManager.GetInstance().CurrentSecond) return false;

        if (currentEvent is not null) return true;

        var localStates = States.Select(state => state.Clone(true)).ToList();

        removeAbsentModules(localStates);
        removeDisabledModules(localStates);
        removeLessCompoundedStates(localStates);
        removeInvalidStates(localStates);

        if (localStates.Count != 1) return false;

        var chosenState = localStates.Single();
        if (!chosenState.Enabled.Value) return false;

        currentState = chosenState;
        return true;
    }

    private void removeAbsentModules(List<ClipState> localStates)
    {
        foreach (var clipState in localStates.ToImmutableList())
        {
            var stateValid = clipState.States.Keys.All(moduleID => ModuleManager.GetInstance().IsModuleLoaded(moduleID));
            if (!stateValid) localStates.Remove(clipState);
        }
    }

    private void removeDisabledModules(List<ClipState> localStates)
    {
        foreach (var clipState in localStates.ToImmutableList())
        {
            var stateValid = clipState.States.Keys.All(moduleID => ModuleManager.GetInstance().IsModuleRunning(moduleID));
            if (!stateValid) localStates.Remove(clipState);
        }
    }

    private void removeLessCompoundedStates(List<ClipState> localStates)
    {
        var enabledAndLinkedModules = LinkedModules.Where(moduleID => ModuleManager.GetInstance().IsModuleRunning(moduleID)).ToList();
        enabledAndLinkedModules.Sort();

        foreach (var clipState in localStates.ToImmutableList())
        {
            var clipStateModules = clipState.States.Keys.ToList();
            clipStateModules.Sort();

            if (!clipStateModules.SequenceEqual(enabledAndLinkedModules)) localStates.Remove(clipState);
        }
    }

    private void removeInvalidStates(List<ClipState> localStates)
    {
        var currentStates = LinkedModules.Where(moduleID => ModuleManager.GetInstance().IsModuleRunning(moduleID) && ChatBoxManager.GetInstance().StateValues.ContainsKey(moduleID) && ChatBoxManager.GetInstance().StateValues[moduleID] is not null).Select(moduleName => ChatBoxManager.GetInstance().StateValues[moduleName]).ToList();
        currentStates.Sort();

        foreach (var clipState in localStates.ToImmutableList())
        {
            var clipStateStates = clipState.States.Values.ToList();
            clipStateStates.Sort();

            if (!clipStateStates.SequenceEqual(currentStates)) localStates.Remove(clipState);
        }
    }

    public string GetFormattedText() => currentEvent is not null ? formatText(currentEvent.Value.Item1) : formatText(currentState!);

    private string formatText(ClipElement element)
    {
        return element.RunFormatting();
    }

    #endregion

    #region Linked Modules

    private void linkedModulesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        populateStates(e);
        populateEvents(e);

        // TODO: When modules are removed, remove all variable instances of that module
        // evaluateVariables(e);

        OnPropertyChanged(nameof(UIStates));
        OnPropertyChanged(nameof(UIEvents));
        OnPropertyChanged(nameof(UIVariables));
    }

    private void populateStates(NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null) removeStatesOfRemovedModules(e);
        if (e.NewItems is not null) addStatesOfAddedModules(e);
    }

    private void removeStatesOfRemovedModules(NotifyCollectionChangedEventArgs e)
    {
        var statesToRemove = new List<ClipState>();

        foreach (string oldModuleID in e.OldItems!)
        {
            foreach (var clipState in States)
            {
                if (clipState.States.ContainsKey(oldModuleID))
                {
                    statesToRemove.Add(clipState);
                }
            }
        }

        statesToRemove.ForEach(clipState => States.Remove(clipState));
    }

    private void addStatesOfAddedModules(NotifyCollectionChangedEventArgs e)
    {
        foreach (string moduleID in e.NewItems!)
        {
            var statesCopy = States.Select(clipState => clipState.Clone()).ToList();
            var statesToAdd = ChatBoxManager.GetInstance().StateReferences.Where(reference => reference.ModuleID == moduleID);

            foreach (var stateReference in statesToAdd)
            {
                var innerStatesCopy = statesCopy.Select(clipState => clipState.Clone()).ToList();

                innerStatesCopy.ForEach(innerClipStateCopy => innerClipStateCopy.States.Add(stateReference.ModuleID, stateReference.StateID));

                States.AddRange(innerStatesCopy);
                States.Add(new ClipState(stateReference));
            }
        }
    }

    private void populateEvents(NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null) removeEventsOfRemovedModules(e);
        if (e.NewItems is not null) addEventsOfAddedModules(e);
    }

    private void removeEventsOfRemovedModules(NotifyCollectionChangedEventArgs e)
    {
        var eventsToRemove = new List<ClipEvent>();

        foreach (string oldModuleID in e.OldItems!)
        {
            foreach (var clipEvent in Events)
            {
                if (clipEvent.ModuleID == oldModuleID)
                {
                    eventsToRemove.Add(clipEvent);
                }
            }
        }

        eventsToRemove.ForEach(clipEvent => Events.Remove(clipEvent));
    }

    private void addEventsOfAddedModules(NotifyCollectionChangedEventArgs e)
    {
        foreach (string moduleID in e.NewItems!)
        {
            var eventsToAdd = ChatBoxManager.GetInstance().EventReferences.Where(reference => reference.ModuleID == moduleID);

            foreach (var eventReference in eventsToAdd)
            {
                Events.Add(new ClipEvent(eventReference));
            }
        }
    }

    #endregion

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
