// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using VRCOSC.App.ChatBox.Clips;
using VRCOSC.App.Utils;

namespace VRCOSC.App.ChatBox;

public record ClipDroppableArea(int Layer, int Start, int End);

public class Layer : INotifyPropertyChanged
{
    public readonly int ID;

    public Observable<bool> Enabled = new(true);

    public ObservableCollection<Clip> Clips { get; } = new();

    /// <summary>
    /// Areas that don't have clips; basically inverted bounds calculation
    /// </summary>
    public IEnumerable<ClipDroppableArea> DroppableAreas => constructDroppableAreas();

    public Layer(int id)
    {
        ID = id;
    }

    public void Init()
    {
        ChatBoxManager.GetInstance().Timeline.Length.Subscribe(_ => OnPropertyChanged(nameof(DroppableAreas)));

        Clips.CollectionChanged += ClipsOnCollectionChanged;
        ClipsOnCollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Clips));
    }

    private void ClipsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (Clip newClip in e.NewItems)
            {
                newClip.Start.Subscribe(_ => OnPropertyChanged(nameof(DroppableAreas)));
                newClip.End.Subscribe(_ => OnPropertyChanged(nameof(DroppableAreas)));
            }
        }

        OnPropertyChanged(nameof(DroppableAreas));
    }

    private IEnumerable<ClipDroppableArea> constructDroppableAreas()
    {
        var droppableAreas = new List<ClipDroppableArea>();
        GetAllBounds().ForEach(bound => droppableAreas.Add(new ClipDroppableArea(ID, bound.Item1, bound.Item2)));
        return droppableAreas;
    }

    public (int, int) GetBoundsNearestTo(int value, bool end, bool isCreating = false)
    {
        value = Math.Clamp(value, 0, ChatBoxManager.GetInstance().Timeline.LengthSeconds);

        var boundsList = new List<int>();

        Clips.ForEach(clip =>
        {
            if (end)
            {
                if (clip.End.Value != value) boundsList.Add(clip.End.Value);
                boundsList.Add(clip.Start.Value);
            }
            else
            {
                if (clip.Start.Value != value) boundsList.Add(clip.Start.Value);
                boundsList.Add(clip.End.Value);
            }
        });

        boundsList.Add(0);
        boundsList.Add(ChatBoxManager.GetInstance().Timeline.LengthSeconds);
        boundsList.Sort();

        var lowerBound = boundsList.Last(bound => bound <= value);
        var upperBound = boundsList.First(bound => bound >= value && (!isCreating || bound > lowerBound));

        return (lowerBound, upperBound);
    }

    public List<(int, int)> GetAllBounds()
    {
        var boundsList = new List<int>();

        boundsList.Add(0);

        Clips.ForEach(clip =>
        {
            boundsList.Add(clip.Start.Value);
            boundsList.Add(clip.End.Value);
        });

        boundsList.Add(ChatBoxManager.GetInstance().Timeline.LengthSeconds);
        boundsList.Sort();

        var pairedBoundsList = new List<(int, int)>();

        for (var i = 0; i < Clips.Count * 2 + 2; i += 2)
        {
            var lowerBound = boundsList[i];
            var upperBound = boundsList[i + 1];

            if (lowerBound == upperBound) continue;

            pairedBoundsList.Add((lowerBound, upperBound));
        }

        return pairedBoundsList;
    }

    public void UpdateUIBinds()
    {
        Clips.ForEach(clip => clip.UpdateUIBinds());
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
