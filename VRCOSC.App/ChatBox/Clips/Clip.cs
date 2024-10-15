// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
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

    public Observable<int> Layer { get; } = new();
    public Observable<int> Start { get; } = new();
    public Observable<int> End { get; } = new();
    public ObservableCollection<string> LinkedModules { get; } = new();

    public ObservableCollection<ClipState> States { get; } = new();
    public ObservableCollection<ClipEvent> Events { get; } = new();

    public IEnumerable<ClipElement> Elements => States.Cast<ClipElement>().Concat(Events);

    public IEnumerable<ClipState> UIStates => States.OrderBy(clipState => clipState.States.Count).ThenBy(clipState => clipState.DisplayName);
    public IEnumerable<ClipEvent> UIEvents => Events.OrderBy(clipEvent => clipEvent.DisplayName);

    public Observable<bool> IsChosenClip { get; } = new();

    // TODO: This doesn't update when, for example, the counter module updates
    // This can be solved by listening to when an attribute attempts to serialise and then update this
    // Same for UIStates and UIEvents
    public Dictionary<string, List<ClipVariableReference>> UIVariables
    {
        get
        {
            var finalDict = new Dictionary<string, List<ClipVariableReference>>();

            var builtInVariables = new List<ClipVariableReference>();
            ChatBoxManager.GetInstance().VariableReferences.Where(clipVariableReference => clipVariableReference.ModuleID is null).OrderBy(reference => reference.DisplayName.Value).ForEach(clipVariableReference => builtInVariables.Add(clipVariableReference));
            finalDict.Add("Built-In", builtInVariables);

            var modules = LinkedModules.Select(moduleID => ModuleManager.GetInstance().GetModuleOfID(moduleID)).OrderBy(module => module.Title);

            foreach (var module in modules)
            {
                finalDict.Add(module.Title, ChatBoxManager.GetInstance().VariableReferences.Where(reference => reference.ModuleID is not null && reference.ModuleID == module.FullID).OrderBy(reference => reference.DisplayName.Value).ToList());
            }

            return finalDict;
        }
    }

    public bool HasStates => States.Any();
    public bool HasEvents => Events.Any();

    private readonly Queue<ClipEvent> eventQueue = new();
    private (ClipEvent, DateTimeOffset)? currentEvent;

    private ClipState? currentState;

    public Clip()
    {
        States.Add(new ClipState(new ClipStateReference
        {
            IsBuiltIn = true
        }));

        LinkedModules.CollectionChanged += linkedModulesOnCollectionChanged;
    }

    public void ChatBoxStart()
    {
        eventQueue.Clear();
        currentState = null;
        currentEvent = null;

        Elements.ForEach(clipElement => clipElement.Variables.ForEach(clipVariable => clipVariable.Start()));
    }

    public void ChatBoxLengthChange()
    {
        var timelineLength = ChatBoxManager.GetInstance().Timeline.Length.Value;

        if (timelineLength <= Start.Value)
        {
            ChatBoxManager.GetInstance().Timeline.Clips.Remove(this);
            return;
        }

        if (timelineLength < End.Value) End.Value = timelineLength;

        OnPropertyChanged(nameof(Start));
        OnPropertyChanged(nameof(End));
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

        if (currentEvent is null && eventQueue.Count != 0)
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

        if (States.Count == 1 && States[0].IsBuiltIn)
        {
            if (!States[0].Enabled.Value)
            {
                currentState = null;
                return false;
            }

            currentState = States[0];
            return true;
        }

        var chosenState = calculateValidClipState();

        if (chosenState is null || !chosenState.Enabled.Value)
        {
            currentState = null;
            return false;
        }

        currentState = chosenState;
        return true;
    }

    private ClipState? calculateValidClipState()
    {
        try
        {
            var runningLinkedModules = LinkedModules.Where(moduleID => ModuleManager.GetInstance().IsModuleRunning(moduleID)).ToList();

            var activeStateIDs = runningLinkedModules.Where(moduleID => ChatBoxManager.GetInstance().StateValues.GetValueOrDefault(moduleID) is not null)
                                                     .Select(moduleID => $"{moduleID}.{ChatBoxManager.GetInstance().StateValues[moduleID]}")
                                                     .ToList();

            // takes a copy of the states to not remove from the original collection
            // first removes if a state has non-running modules or less/more compounded states compared to the running modules
            // second removes if a state has invalid module states compared to the active module states
            // because modules can have the same state names, we fully qualify the states based on their owning modules
            return States.ToList().RemoveIf(clipState => !clipState.States.Keys.ContainsSame(runningLinkedModules) || !activeStateIDs.ContainsSame(clipState.States.Select(state => $"{state.Key}.{state.Value}"))).SingleOrDefault();
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, "More than 1 clip state was chosen which isn't possible");
            return null;
        }
    }

    public string GetFormattedText() => currentEvent is not null ? formatText(currentEvent.Value.Item1) : formatText(currentState);
    public bool ShouldShowTyping() => currentEvent is not null ? currentEvent.Value.Item1.ShowTyping.Value : currentState?.ShowTyping.Value ?? false;
    public bool ShouldUseMinimalBackground() => currentEvent is not null ? currentEvent.Value.Item1.UseMinimalBackground.Value : currentState?.UseMinimalBackground.Value ?? false;

    private string formatText(ClipElement? element) => element?.RunFormatting() ?? string.Empty;

    #endregion

    #region Linked Modules

    private void linkedModulesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        populateStates(e);
        populateEvents(e);

        if (e.Action == NotifyCollectionChangedAction.Remove) removeAbsentVariables();

        OnPropertyChanged(nameof(UIStates));
        OnPropertyChanged(nameof(UIEvents));
        OnPropertyChanged(nameof(UIVariables));
        OnPropertyChanged(nameof(HasStates));
        OnPropertyChanged(nameof(HasEvents));
    }

    private void removeAbsentVariables()
    {
        Elements.ForEach(clipElement => clipElement.Variables.RemoveIf(clipVariable => clipVariable.ModuleID is not null && !LinkedModules.Contains(clipVariable.ModuleID)));
    }

    private void populateStates(NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add) States.RemoveIf(clipState => clipState.IsBuiltIn);

        if (e.OldItems is not null) removeStatesOfRemovedModules(e);
        if (e.NewItems is not null) addStatesOfAddedModules(e);

        if (e.Action == NotifyCollectionChangedAction.Remove && !States.Any())
        {
            States.Add(new ClipState(new ClipStateReference
            {
                IsBuiltIn = true
            }));
        }
    }

    private void removeStatesOfRemovedModules(NotifyCollectionChangedEventArgs e)
    {
        foreach (string oldModuleID in e.OldItems!)
        {
            States.RemoveIf(clipState => clipState.States.ContainsKey(oldModuleID));
        }
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
        foreach (string oldModuleID in e.OldItems!)
        {
            Events.RemoveIf(clipEvent => clipEvent.ModuleID == oldModuleID);
        }
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
