// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using VRCOSC.Game.ChatBox.Clips;

namespace VRCOSC.Game.ChatBox;

public class ChatBoxManager
{
    public IReadOnlyList<Clip> Clips => clips;
    private readonly List<Clip> clips = new();

    public readonly Bindable<TimeSpan> TimelineLength = new(TimeSpan.FromMinutes(1));

    public ChatBoxManager()
    {
        AddClip(new Clip());
    }

    public void AddClip(Clip clip)
    {
        clips.Add(clip);
    }

    public void RemoveClip(Clip clip)
    {
        clips.Remove(clip);
    }
}
