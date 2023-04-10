// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Bindables;

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
    public readonly BindableDictionary<string, Dictionary<string, ClipState>> States = new();
    public readonly BindableDictionary<string, Dictionary<string, ClipEvent>> Events = new();
    public readonly Bindable<int> Start = new();
    public readonly Bindable<int> End = new(30);
    public int Length => End.Value - Start.Value;

    public readonly Dictionary<string, Dictionary<string, DateTimeOffset>> ModuleEvents = new();

    private readonly ChatBoxManager chatBoxManager;

    public Clip(ChatBoxManager chatBoxManager)
    {
        this.chatBoxManager = chatBoxManager;
        AssociatedModules.BindCollectionChanged((_, e) => onAssociatedModulesChanged(e), true);
    }

    public void Update()
    {
        // check for events
    }

    public bool Evalulate()
    {
        if (!Enabled.Value) return false;

        // check states and compound states for what state we're currently in and check to see if that state is enabled
        // check if there are any ongoing events

        return true;
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
        if (e.NewItems is not null)
        {
            foreach (string newModule in e.NewItems)
            {
                var states = chatBoxManager.States[newModule];

                States.Add(newModule, new Dictionary<string, ClipState>());

                foreach (var (key, value) in states)
                {
                    States[newModule][key] = value;
                }
            }
        }

        if (e.OldItems is not null)
        {
            foreach (string oldModule in e.OldItems)
            {
                States.Remove(oldModule);
            }
        }

        // compound state calculation
    }

    private void populateEvents(NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (string newModule in e.NewItems)
            {
                var events = chatBoxManager.Events[newModule];

                Events.Add(newModule, new Dictionary<string, ClipEvent>());

                foreach (var (key, value) in events)
                {
                    Events[newModule][key] = value;
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
