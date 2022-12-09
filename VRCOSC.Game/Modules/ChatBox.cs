// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using VRCOSC.Game.Modules.Util;
using VRCOSC.OSC;

namespace VRCOSC.Game.Modules;

public class ChatBox
{
    private const string chatbox_address_typing = "/chatbox/typing";
    private const string chatbox_address_text = "/chatbox/input";

    private readonly OscClient oscClient;
    private readonly ConcurrentQueue<ChatBoxData> timedQueue = new();
    private readonly ConcurrentDictionary<int, ChatBoxData> alwaysDict = new();

    private TimedTask? queueTask;
    private ChatBoxData? currentData;
    private DateTimeOffset? sendReset;
    private DateTimeOffset sendExpire;
    private bool alreadyClear;
    private bool sendEnabled;
    private IBindable<int> resetMilli;

    public ChatBox(OscClient oscClient, IBindable<int> resetMilli)
    {
        this.oscClient = oscClient;
        this.resetMilli = resetMilli;
    }

    public void SetSending(bool canSend)
    {
        sendEnabled = canSend;
    }

    public void SetTyping(bool typing)
    {
        oscClient.SendValue(chatbox_address_typing, typing);
    }

    public void Init()
    {
        alreadyClear = true;
        currentData = null;
        sendExpire = DateTimeOffset.Now;
        sendReset = null;
        sendEnabled = true;
        timedQueue.Clear();
        alwaysDict.Clear();
        queueTask = new TimedTask(update, 5);
        _ = queueTask.Start();
    }

    public async Task Shutdown()
    {
        await (queueTask?.Stop() ?? Task.CompletedTask);
        clear();
    }

    private void clear()
    {
        oscClient.SendValues(chatbox_address_text, new List<object> { string.Empty, true });
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
        timedQueue.Enqueue(new ChatBoxData
        {
            Text = text,
            DisplayLength = displayLength
        });

        var closestNextSendTime = DateTimeOffset.Now;
        timedQueue.ForEach(d => closestNextSendTime += d.DisplayLength);
        return closestNextSendTime;
    }

    private Task update()
    {
        switch (timedQueue.IsEmpty)
        {
            case true when sendExpire < DateTimeOffset.Now:
            {
                var validAlwaysData = alwaysDict.Where(pair => pair.Value.Text is not null).ToImmutableSortedDictionary();
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

        return Task.CompletedTask;
    }

    private void trySendText()
    {
        if (sendReset is not null && sendReset <= DateTimeOffset.Now) sendReset = null;
        if (sendReset is not null) return;

        if (currentData is null)
        {
            if (!alreadyClear) clear();
            return;
        }

        alreadyClear = false;

        if (currentData.Text is null) return;

        if (sendEnabled) oscClient.SendValues(chatbox_address_text, new List<object> { currentData.Text!, true });
        sendReset = DateTimeOffset.Now + TimeSpan.FromMilliseconds(resetMilli.Value);
    }

    private class ChatBoxData
    {
        public string? Text { get; init; }
        public TimeSpan DisplayLength { get; init; }
    }
}
