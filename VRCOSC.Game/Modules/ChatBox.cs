// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using VRCOSC.Game.Modules.Util;
using VRCOSC.OSC;

namespace VRCOSC.Game.Modules;

public class ChatBox
{
    private readonly OscClient oscClient;
    private TimedTask? overrideTask;
    private TimedTask? nextAvailableSend;
    private List<object>? lastValues;

    public ChatBox(OscClient oscClient)
    {
        this.oscClient = oscClient;
    }

    public void SetTyping(bool typing)
    {
        oscClient.SendValue("/chatbox/typing", typing);
    }

    public void SetText(string text, bool bypassKeyboard = true, ChatBoxPriority priority = ChatBoxPriority.Normal, float priorityTimeMilli = 10000f)
    {
        var values = new List<object>() { text, bypassKeyboard };

        if (priority == ChatBoxPriority.Override)
        {
            overrideTask?.Stop();
            overrideTask = new TimedTask(resetOverride, priorityTimeMilli).Start();
        }

        if (priority == ChatBoxPriority.Normal && overrideTask is not null) return;

        if (nextAvailableSend is not null)
        {
            lastValues = values;
            return;
        }

        oscClient.SendValues("/chatbox/input", values);
        nextAvailableSend = new TimedTask(markAvailable, 2000).Start();
    }

    private void markAvailable()
    {
        nextAvailableSend?.Stop();
        nextAvailableSend = null;

        if (lastValues is null) return;

        oscClient.SendValues("/chatbox/input", lastValues);
        lastValues = null;
    }

    private void resetOverride()
    {
        overrideTask?.Stop();
        overrideTask = null;
        Clear();
    }

    public void Clear()
    {
        SetText(string.Empty, true, ChatBoxPriority.Override, 1);
    }
}

public enum ChatBoxPriority
{
    Normal,
    Override
}
