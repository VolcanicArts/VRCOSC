// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using VRCOSC.Game.ChatBox.Clips;

namespace VRCOSC.Game.ChatBox;

public class ChatBoxManager
{
    public IReadOnlyList<Clip> Clips => clips;
    private readonly List<Clip> clips = new();

    public Clip CreateClip()
    {
        var clip = new Clip();
        clips.Add(clip);
        return clip;
    }

    public void DeleteClip(Clip clip)
    {
        clips.Remove(clip);
    }
}
