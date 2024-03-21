// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using VRCOSC.App.Utils;

namespace VRCOSC.App.ChatBox.Clips;

public class Clip : INotifyPropertyChanged
{
    public Observable<bool> Enabled { get; } = new(true);
    public Observable<string> Name { get; } = new("New Clip");

    public Observable<int> Start { get; } = new();
    public Observable<int> End { get; } = new();
    public ObservableCollection<string> LinkedModules { get; } = new();
    public ObservableCollection<ClipState> States { get; } = new();
    public ObservableCollection<ClipEvent> Events { get; } = new();

    public IEnumerable<ClipState> UIStates => States.OrderBy(clipState => clipState.States.Count)
                                                    .ThenBy(state => string.Join(",", state.States.Keys.OrderBy(k => k)))
                                                    .ThenBy(state => string.Join(",", state.States.Values.OrderBy(v => v)));

    public IEnumerable<ClipEvent> UIEvents => Events.OrderBy(clipEvent => clipEvent.ModuleID)
                                                    .ThenBy(clipEvent => clipEvent.EventID);

    public Clip()
    {
        // TODO: This will go AFTER deserialistion
        LinkedModules.CollectionChanged += linkedModulesOnCollectionChanged;
    }

    private void linkedModulesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        populateStates(e);
        populateEvents(e);

        OnPropertyChanged(nameof(UIStates));
        OnPropertyChanged(nameof(UIEvents));
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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
