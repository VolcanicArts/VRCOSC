// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using VRCOSC.OSC;

namespace VRCOSC.Game.Modules;

public class ChatBox
{
    private const int chatbox_reset_milli = 1500;
    private const string chatbox_address_typing = "/chatbox/typing";
    private const string chatbox_address_text = "/chatbox/input";

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
        oscClient.SendValue(chatbox_address_typing, typing);
    }

    public void SetText(string text, bool bypassKeyboard = true, int priority = 0, int priorityTimeMilli = 0)
    {
        if (isTyping) return;

        tryResetPriority();
        tryResetSend();

        if (priority < lastSentPriority) return;

        trySendValues(new List<object>() { text, bypassKeyboard }, priority > lastSentPriority);
        lastSentPriority = priority;
        priorityReset = DateTimeOffset.Now + TimeSpan.FromMilliseconds(priorityTimeMilli);
    }

    public void Clear(int priority)
    {
        SetText(string.Empty, true, priority + 1, chatbox_reset_milli);
    }

    private void tryResetPriority()
    {
        if (priorityReset is not null && priorityReset <= DateTimeOffset.Now)
        {
            lastSentPriority = 0;
            priorityReset = null;
        }
    }

    private void tryResetSend()
    {
        if (sendReset is not null && sendReset <= DateTimeOffset.Now)
        {
            sendReset = null;
        }
    }

    private void trySendValues(List<object> values, bool burst)
    {
        if (sendReset is not null && !burst) return;

        oscClient.SendValues(chatbox_address_text, values);
        sendReset = DateTimeOffset.Now + TimeSpan.FromMilliseconds(chatbox_reset_milli);
    }
}
