// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using VRCOSC.Game.ChatBox.Clips;

namespace VRCOSC.Game.ChatBox;

public class ChatBoxManager
{
    public readonly BindableList<Clip> Clips = new();

    public readonly Bindable<TimeSpan> TimelineLength = new(TimeSpan.FromMinutes(1));

    public float Resolution => 1f / (float)TimelineLength.Value.TotalSeconds;

    public ChatBoxManager()
    {
        for (var i = 0; i < 6; i++)
        {
            Clips.Add(new Clip
            {
                Priority = { Value = i }
            });
        }
    }

    public bool IncreasePriority(Clip clip) => SetPriority(clip, clip.Priority.Value + 1);
    public bool DecreasePriority(Clip clip) => SetPriority(clip, clip.Priority.Value - 1);

    public bool SetPriority(Clip clip, int priority)
    {
        if (priority is > 5 or < 0) return false;
        if (RetrieveClipsWithPriority(priority).Any(clip.Intersects)) return false;

        clip.Priority.Value = priority;
        return true;
    }

    public IReadOnlyList<Clip> RetrieveClipsWithPriority(int priority) => Clips.Where(clip => clip.Priority.Value == priority).ToList();
}
