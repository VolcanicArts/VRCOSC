// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using VRCOSC.Game.Modules.Util;
using VRCOSC.OSC;

namespace VRCOSC.Game.Modules;

public class ChatBox
{
    private readonly OscClient oscClient;
    private TimedTask? nextSend;
    private TimedTask? priorityTask;
    private int lastSentPriority;

    public ChatBox(OscClient oscClient)
    {
        this.oscClient = oscClient;
    }

    public void SetTyping(bool typing)
    {
        oscClient.SendValue("/chatbox/typing", typing);
    }

    public void SetText(string text, bool bypassKeyboard = true, int priority = 0, float priorityTimeMilli = 10000f)
    {
        var values = new List<object>() { text, bypassKeyboard };

        if (priority >= lastSentPriority)
        {
            trySendValues(values, priority > lastSentPriority);
            lastSentPriority = priority;
            priorityTask?.Stop();
            priorityTask = new TimedTask(resetPriority, priorityTimeMilli).Start();
        }
    }

    private void trySendValues(List<object> values, bool shouldBurst)
    {
        if (nextSend is not null && !shouldBurst) return;

        oscClient.SendValues("/chatbox/input", values);
        nextSend = new TimedTask(resetNextSend, 1500).Start();
    }

    private void resetNextSend()
    {
        nextSend?.Stop();
        nextSend = null;
    }

    private void resetPriority()
    {
        priorityTask?.Stop();
        priorityTask = null;
        lastSentPriority = 0;
    }

    public void Clear(int priority)
    {
        var priorityToSend = lastSentPriority == priority ? priority + 1 : priority;
        SetText(string.Empty, true, priorityToSend, 1);
    }
}
