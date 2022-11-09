// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using VRCOSC.OSC;

namespace VRCOSC.Game.Modules;

public class ChatBox
{
    private readonly OscClient oscClient;
    private DateTimeOffset? sendReset;
    private DateTimeOffset? priorityReset;
    private int lastSentPriority;
    private bool isTyping;

    public ChatBox(OscClient oscClient)
    {
        this.oscClient = oscClient;
    }

    public void SetTyping(bool typing)
    {
        isTyping = typing;
        oscClient.SendValue("/chatbox/typing", typing);
    }

    public void SetText(string text, bool bypassKeyboard = true, int priority = 0, int priorityTimeMilli = 1)
    {
        if (isTyping) return;

        var values = new List<object>() { text, bypassKeyboard };

        if (priorityReset is not null && priorityReset < DateTimeOffset.Now)
        {
            lastSentPriority = 0;
            priorityReset = null;
        }

        if (sendReset is not null && sendReset < DateTimeOffset.Now)
        {
            sendReset = null;
        }

        if (priority >= lastSentPriority)
        {
            trySendValues(values, priority > lastSentPriority);
            lastSentPriority = priority;
            priorityReset = DateTimeOffset.Now + TimeSpan.FromMilliseconds(priorityTimeMilli);
        }
    }

    private void trySendValues(List<object> values, bool shouldBurst)
    {
        if (sendReset is not null && !shouldBurst) return;

        oscClient.SendValues("/chatbox/input", values);
        sendReset = DateTimeOffset.Now + TimeSpan.FromMilliseconds(1500);
    }

    public void Clear(int priority)
    {
        var priorityToSend = lastSentPriority == priority ? priority + 1 : priority;
        SetText(string.Empty, true, priorityToSend, 1);
    }
}
