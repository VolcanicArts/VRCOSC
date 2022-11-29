// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Framework.Extensions.IEnumerableExtensions;
using VRCOSC.Game.Modules.Util;
using VRCOSC.OSC;

namespace VRCOSC.Game.Modules;

public class ChatBox
{
    private const int chatbox_reset_milli = 1500;
    private const string chatbox_address_typing = "/chatbox/typing";
    private const string chatbox_address_text = "/chatbox/input";

    private readonly OscClient oscClient;
    private readonly ConcurrentQueue<ChatBoxData> queue = new();

    private TimedTask queueTask;
    private ChatBoxData? currentData;
    private DateTimeOffset? sendReset;
    private DateTimeOffset sendExpire;

    public ChatBox(OscClient oscClient)
    {
        this.oscClient = oscClient;
    }

    public void SetTyping(bool typing)
    {
        oscClient.SendValue(chatbox_address_typing, typing);
    }

    public void Init()
    {
        queueTask = new TimedTask(update, 5).Start();
        currentData = null;
        sendExpire = DateTimeOffset.Now;
        sendReset = null;
    }

    public async Task Shutdown()
    {
        clear();
        await queueTask.Stop();
    }

    private void clear()
    {
        oscClient.SendValues(chatbox_address_text, new List<object> { string.Empty, true });
    }

    public DateTimeOffset SetText(string text, int priority, TimeSpan displayLength)
    {
        var data = new ChatBoxData
        {
            Text = text,
            DisplayLength = displayLength
        };

        // ChatBoxMode.Always
        // TODO: Find a way to integrate priority
        if (displayLength == TimeSpan.Zero)
        {
            if (queue.IsEmpty && sendExpire < DateTimeOffset.Now)
            {
                currentData = data;
            }

            return DateTimeOffset.Now;
        }

        // ChatBoxMode.Timed
        queue.Enqueue(new ChatBoxData
        {
            Text = text,
            DisplayLength = displayLength
        });

        var closestNextSendTime = DateTimeOffset.Now;
        queue.ForEach(d => closestNextSendTime += d.DisplayLength);
        return closestNextSendTime;
    }

    private Task update()
    {
        if (!queue.IsEmpty)
        {
            if (sendExpire < DateTimeOffset.Now && queue.TryDequeue(out var data))
            {
                currentData = data;
                sendExpire = DateTimeOffset.Now + data.DisplayLength;
            }
        }

        if (currentData is not null) trySendText(currentData);

        return Task.CompletedTask;
    }

    private void trySendText(ChatBoxData? data)
    {
        if (sendReset is not null && sendReset <= DateTimeOffset.Now) sendReset = null;
        if (sendReset is not null) return;

        oscClient.SendValues(chatbox_address_text, new List<object>() { data!.Text, true });
        sendReset = DateTimeOffset.Now + TimeSpan.FromMilliseconds(chatbox_reset_milli);
    }

    private class ChatBoxData
    {
        public string Text { get; init; }
        public TimeSpan DisplayLength { get; init; }
    }
}
