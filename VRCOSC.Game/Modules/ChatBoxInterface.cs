// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using VRCOSC.OSC.Client;
using VRCOSC.OSC.VRChat;

namespace VRCOSC.Game.Modules;

public class ChatBoxInterface
{
    private readonly ConcurrentQueue<ChatBoxData> timedQueue = new();
    private readonly ConcurrentDictionary<int, ChatBoxData> alwaysDict = new();
    private readonly OscClient oscClient;
    private readonly IBindable<int> resetMilli;

    private ChatBoxData? currentData;
    private DateTimeOffset? sendReset;
    private DateTimeOffset sendExpire;
    private bool alreadyClear;
    private bool running;

    private bool sendEnabled;

    public bool SendEnabled
    {
        get => sendEnabled;
        set
        {
            sendEnabled = value;
            if (!sendEnabled) clear();
        }
    }

    public ChatBoxInterface(OscClient oscClient, IBindable<int> resetMilli)
    {
        this.oscClient = oscClient;
        this.resetMilli = resetMilli;
    }

    public void SetTyping(bool typing)
    {
        oscClient.SendValue(VRChatOscConstants.ADDRESS_CHATBOX_TYPING, typing);
    }

    public void Initialise()
    {
        currentData = null;
        sendReset = null;
        alreadyClear = true;
        SendEnabled = true;
        sendExpire = DateTimeOffset.Now;
        timedQueue.Clear();
        alwaysDict.Clear();
        running = true;
    }

    public void Shutdown()
    {
        running = false;
        clear();
    }

    private void clear()
    {
        sendText(string.Empty);
        alreadyClear = true;
    }

    public DateTimeOffset SetText(string? text, int priority, TimeSpan displayLength)
    {
        var data = new ChatBoxData
        {
            Text = text,
            DisplayLength = displayLength
        };

        // ChatBoxMode.Always
        if (displayLength == TimeSpan.Zero)
        {
            alwaysDict[priority] = data;
            return DateTimeOffset.Now;
        }

        // ChatBoxMode.Timed

        if (text is null) return DateTimeOffset.Now;

        timedQueue.Enqueue(new ChatBoxData
        {
            Text = text,
            DisplayLength = displayLength
        });

        var closestNextSendTime = DateTimeOffset.Now;
        timedQueue.ForEach(d => closestNextSendTime += d.DisplayLength);
        return closestNextSendTime;
    }

    public void Update()
    {
        if (!running) return;

        switch (timedQueue.IsEmpty)
        {
            case true when sendExpire < DateTimeOffset.Now:
            {
                var validAlwaysData = alwaysDict.Where(data => data.Value.Text is not null).ToImmutableSortedDictionary();
                currentData = validAlwaysData.IsEmpty ? null : validAlwaysData.Last().Value;
                break;
            }

            case false:
            {
                if (sendExpire < DateTimeOffset.Now && timedQueue.TryDequeue(out var data))
                {
                    currentData = data;
                    sendExpire = DateTimeOffset.Now + data.DisplayLength;
                }

                break;
            }
        }

        trySendText();
    }

    private void trySendText()
    {
        if (sendReset is not null && sendReset <= DateTimeOffset.Now) sendReset = null;
        if (sendReset is not null) return;

        if (currentData is null)
        {
            if (!alreadyClear)
            {
                clear();
                sendReset = DateTimeOffset.Now + TimeSpan.FromMilliseconds(resetMilli.Value);
            }

            return;
        }

        alreadyClear = false;

        if (currentData.Text is null) return;

        if (sendEnabled) sendText(currentData.Text);
        sendReset = DateTimeOffset.Now + TimeSpan.FromMilliseconds(resetMilli.Value);
    }

    private void sendText(string text)
    {
        oscClient.SendValues(VRChatOscConstants.ADDRESS_CHATBOX_INPUT, new List<object> { text, true, false });
    }

    private class ChatBoxData
    {
        public string? Text { get; init; }
        public TimeSpan DisplayLength { get; init; }
    }
}
