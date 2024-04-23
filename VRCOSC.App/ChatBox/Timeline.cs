// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VRCOSC.App.ChatBox.Clips;
using VRCOSC.App.Utils;

namespace VRCOSC.App.ChatBox;

public class Timeline
{
    private const int layer_count = 32;
    private const int default_length = 60;

    public int LayerCount => layer_count;
    public Observable<int> Length { get; } = new(default_length);

    public float Resolution => 1f / Length.Value;

    public int UILength
    {
        set
        {
            var secondsValue = value;
            secondsValue = Math.Clamp(secondsValue, 1, 60 * 4);
            Length.Value = secondsValue;
        }
        get => Length.Value;
    }

    public ObservableCollection<Clip> Clips { get; } = new();
    public ObservableCollection<DroppableArea> DroppableAreas { get; } = new();
    public bool[] LayerEnabled = new bool[layer_count];

    public Timeline()
    {
        Length.Subscribe(_ =>
        {
            Clips.ForEach(clip => clip.ChatBoxLengthChange());
            ChatBoxManager.GetInstance().Serialise();
        });

        Clips.CollectionChanged += (_, e) =>
        {
            if (e.NewItems is not null)
            {
                foreach (Clip newClip in e.NewItems)
                {
                    newClip.Enabled.Subscribe(_ => ChatBoxManager.GetInstance().Serialise());
                    newClip.Name.Subscribe(_ => ChatBoxManager.GetInstance().Serialise());
                    newClip.Layer.Subscribe(_ => ChatBoxManager.GetInstance().Serialise());
                    GenerateDroppableAreas(newClip.Layer.Value);
                }
            }

            if (e.OldItems is not null)
            {
                foreach (Clip oldClip in e.OldItems)
                {
                    GenerateDroppableAreas(oldClip.Layer.Value);
                }
            }

            ChatBoxManager.GetInstance().Serialise();
        };
    }

    public void GenerateDroppableAreas(int layer)
    {
        DroppableAreas.RemoveIf(area => area.Layer == layer);

        var droppableAreas = new List<DroppableArea>();
        var layerClips = Clips.Where(clip => clip.Layer.Value == layer).ToList();

        var boundsList = new List<int>();

        boundsList.Add(0);

        layerClips.ForEach(clip =>
        {
            boundsList.Add(clip.Start.Value);
            boundsList.Add(clip.End.Value);
        });

        boundsList.Add(ChatBoxManager.GetInstance().Timeline.Length.Value);
        boundsList.Sort();

        for (var j = 0; j < layerClips.Count * 2 + 2; j += 2)
        {
            var lowerBound = boundsList[j];
            var upperBound = boundsList[j + 1];

            if (lowerBound == upperBound) continue;

            droppableAreas.Add(new DroppableArea(layer, lowerBound, upperBound));
        }

        DroppableAreas.AddRange(droppableAreas);
    }

    public (int, int) GetBoundsNearestTo(int value, int layer, bool end, bool isCreating = false)
    {
        value = Math.Clamp(value, 0, Length.Value);

        var boundsList = new List<int>();

        Clips.Where(clip => clip.Layer.Value == layer).ForEach(clip =>
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
        boundsList.Add(Length.Value);
        boundsList.Sort();

        var lowerBound = boundsList.Last(bound => bound <= value);
        var upperBound = boundsList.First(bound => bound >= value && (!isCreating || bound > lowerBound));

        return (lowerBound, upperBound);
    }
}

public record DroppableArea(int Layer, int Start, int End);
