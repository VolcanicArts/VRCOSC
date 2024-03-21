// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VRCOSC.App.ChatBox.Clips;
using VRCOSC.App.Utils;

namespace VRCOSC.App.ChatBox;

public class Layer
{
    public ObservableCollection<Clip> Clips { get; } = new();

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
}
