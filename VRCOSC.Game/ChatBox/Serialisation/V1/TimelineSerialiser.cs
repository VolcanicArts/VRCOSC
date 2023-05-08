// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Platform;
using VRCOSC.Game.ChatBox.Serialisation.V1.Structures;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.ChatBox.Serialisation.V1;

public class TimelineSerialiser : Serialiser<ChatBoxManager, SerialisableTimeline>
{
    protected override string FileName => @"chatbox.json";

    public TimelineSerialiser(Storage storage, NotificationContainer notification, ChatBoxManager chatBoxManager)
        : base(storage, notification, chatBoxManager)
    {
    }

    protected override object GetSerialisableData(ChatBoxManager chatBoxManager) => new SerialisableTimeline(chatBoxManager);
}
