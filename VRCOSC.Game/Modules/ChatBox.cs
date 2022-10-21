// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using VRCOSC.Game.Modules.Util;
using VRCOSC.OSC;

namespace VRCOSC.Game.Modules;

public class ChatBox
{
    private readonly OscClient oscClient;
    private TimedTask? updateTask;
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
            updateTask?.Stop();
            updateTask = new TimedTask(reset, priorityTimeMilli).Start();
        }

        if (priority == ChatBoxPriority.Normal && updateTask is not null) return;

        if (nextAvailableSend is not null)
        {
            lastValues = values;
            return;
        }

        oscClient.SendValues("/chatbox/input", values);
        nextAvailableSend = new TimedTask(markAvailable, 1500).Start();
    }

    private void markAvailable()
    {
        nextAvailableSend?.Stop();
        nextAvailableSend = null;

        if (lastValues is null) return;

        oscClient.SendValues("/chatbox/input", lastValues);
        lastValues = null;
    }

    private void reset()
    {
        updateTask?.Stop();
        updateTask = null;
    }
}

public enum ChatBoxPriority
{
    Normal,
    Override
}
