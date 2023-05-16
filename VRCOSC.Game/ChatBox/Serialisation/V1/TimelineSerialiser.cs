// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Immutable;
using System.Linq;
using osu.Framework.Platform;
using VRCOSC.Game.ChatBox.Serialisation.V1.Structures;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Managers;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.ChatBox.Serialisation.V1;

public class TimelineSerialiser : Serialiser<ChatBoxManager, SerialisableTimeline>
{
    protected override string FileName => @"chatbox.json";

    public TimelineSerialiser(Storage storage, NotificationContainer notification, ChatBoxManager chatBoxManager)
        : base(storage, notification, chatBoxManager)
    {
    }

    protected override SerialisableTimeline GetSerialisableData(ChatBoxManager chatBoxManager) => new(chatBoxManager);

    protected override void ExecuteAfterDeserialisation(ChatBoxManager chatBoxManager, SerialisableTimeline data)
    {
        chatBoxManager.Clips.Clear();

        data.Clips.ForEach(clip =>
        {
            clip.AssociatedModules.ToImmutableList().ForEach(moduleName =>
            {
                if (!chatBoxManager.StateMetadata.ContainsKey(moduleName) && !chatBoxManager.EventMetadata.ContainsKey(moduleName))
                {
                    clip.AssociatedModules.Remove(moduleName);

                    clip.States.ToImmutableList().ForEach(clipState =>
                    {
                        clipState.States.RemoveAll(pair => pair.Module == moduleName);
                    });

                    clip.Events.RemoveAll(clipEvent => clipEvent.Module == moduleName);

                    return;
                }

                clip.States.ToImmutableList().ForEach(clipState =>
                {
                    clipState.States.RemoveAll(pair => !chatBoxManager.StateMetadata[pair.Module].ContainsKey(pair.Lookup));
                });

                clip.Events.RemoveAll(clipEvent => !chatBoxManager.EventMetadata[clipEvent.Module].ContainsKey(clipEvent.Lookup));
            });
        });

        data.Clips.ForEach(clip =>
        {
            var newClip = chatBoxManager.CreateClip();

            newClip.Enabled.Value = clip.Enabled;
            newClip.Name.Value = clip.Name;
            newClip.Priority.Value = clip.Priority;
            newClip.Start.Value = clip.Start;
            newClip.End.Value = clip.End;

            newClip.AssociatedModules.AddRange(clip.AssociatedModules);

            clip.States.ForEach(clipState =>
            {
                var stateData = newClip.GetStateFor(clipState.States.Select(state => state.Module), clipState.States.Select(state => state.Lookup));
                if (stateData is null) return;

                stateData.Enabled.Value = clipState.Enabled;
                stateData.Format.Value = clipState.Format;
            });

            clip.Events.ForEach(clipEvent =>
            {
                var eventData = newClip.GetEventFor(clipEvent.Module, clipEvent.Lookup);
                if (eventData is null) return;

                eventData.Enabled.Value = clipEvent.Enabled;
                eventData.Format.Value = clipEvent.Format;
                eventData.Length.Value = clipEvent.Length;
            });

            chatBoxManager.Clips.Add(newClip);
        });

        chatBoxManager.SetTimelineLength(TimeSpan.FromTicks(data.Ticks));
    }
}
