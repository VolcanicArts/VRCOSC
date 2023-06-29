// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Platform;
using VRCOSC.Game.ChatBox.Clips;
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
        var createdClips = new List<Clip>();

        data.Clips.ForEach(clip =>
        {
            var newClip = chatBoxManager.CreateClip();

            newClip.Enabled.Value = clip.Enabled;
            newClip.Name.Value = clip.Name;
            newClip.Priority.Value = clip.Priority;
            newClip.Start.Value = clip.Start;
            newClip.End.Value = clip.End;

            newClip.AssociatedModules.AddRange(clip.AssociatedModules.Where(serialisedModuleName => chatBoxManager.GameManager.ModuleManager.DoesModuleExist(serialisedModuleName)));

            clip.States.Where(clipState => clipState.States.All(pair => chatBoxManager.GameManager.ModuleManager.DoesModuleExist(pair.Module))).ForEach(clipState =>
            {
                if (clipState.States.All(pair => chatBoxManager.GameManager.ModuleManager.IsModuleLoaded(pair.Module)))
                {
                    var stateData = newClip.GetStateFor(clipState.States.Select(state => state.Module), clipState.States.Select(state => state.Lookup));
                    if (stateData is null) return;

                    stateData.Enabled.Value = clipState.Enabled;
                    stateData.Format.Value = clipState.Format;
                }
                else
                {
                    // In the case that a custom module has loaded incorrectly still load the data so it doesn't get lost
                    newClip.States.Add(new ClipState
                    {
                        States = clipState.States.Select(state => (state.Lookup, state.Module)).ToList(),
                        Enabled = { Value = clipState.Enabled },
                        Format = { Value = clipState.Format }
                    });
                }
            });

            clip.Events.Where(clipEvent => chatBoxManager.GameManager.ModuleManager.DoesModuleExist(clipEvent.Module)).ForEach(clipEvent =>
            {
                if (chatBoxManager.GameManager.ModuleManager.IsModuleLoaded(clipEvent.Module))
                {
                    var eventData = newClip.GetEventFor(clipEvent.Module, clipEvent.Lookup);
                    if (eventData is null) return;

                    eventData.Enabled.Value = clipEvent.Enabled;
                    eventData.Format.Value = clipEvent.Format;
                    eventData.Length.Value = clipEvent.Length;
                }
                else
                {
                    // In the case that a custom module has loaded incorrectly still load the data so it doesn't get lost
                    newClip.Events.Add(new ClipEvent
                    {
                        Lookup = clipEvent.Lookup,
                        Module = clipEvent.Module,
                        Enabled = { Value = clipEvent.Enabled },
                        Format = { Value = clipEvent.Format },
                        Length = { Value = clipEvent.Length }
                    });
                }
            });

            createdClips.Add(newClip);
        });

        chatBoxManager.Clips.ReplaceItems(createdClips);
        chatBoxManager.SetTimelineLength(TimeSpan.FromTicks(data.Ticks));
    }
}
