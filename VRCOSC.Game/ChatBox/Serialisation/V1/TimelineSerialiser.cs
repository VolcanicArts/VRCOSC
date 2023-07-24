// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Platform;
using VRCOSC.Game.App;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.ChatBox.Serialisation.V1.Structures;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.ChatBox.Serialisation.V1;

public class TimelineSerialiser : Serialiser<AppManager, SerialisableTimeline>
{
    protected override string FileName => @"chatbox.json";

    public TimelineSerialiser(Storage storage, NotificationContainer notification, AppManager appManager)
        : base(storage, notification, appManager)
    {
    }

    protected override SerialisableTimeline GetSerialisableData(AppManager appManager) => new(appManager);

    protected override bool ExecuteAfterDeserialisation(AppManager appManager, SerialisableTimeline data)
    {
        var createdClips = new List<Clip>();

        var migrationOccurred = false;

        data.Clips.ForEach(clip =>
        {
            var newClip = appManager.ChatBoxManager.CreateClip();

            newClip.Enabled.Value = clip.Enabled;
            newClip.Name.Value = clip.Name;
            newClip.Priority.Value = clip.Priority;
            newClip.Start.Value = clip.Start;
            newClip.End.Value = clip.End;

            var migrationList = appManager.ModuleManager.GetMigrations();

            migrationList.ForEach(migration =>
            {
                var index = clip.AssociatedModules.FindIndex(module => module == migration.Item1);
                if (index == -1) return;

                clip.AssociatedModules[index] = migration.Item2;
                migrationOccurred = true;
            });

            migrationList.ForEach(migration =>
            {
                clip.States.ForEach(clipState =>
                {
                    var index = clipState.States.FindIndex(state => state.Module == migration.Item1);
                    if (index == -1) return;

                    clipState.States[index].Module = migration.Item2;
                });
                migrationOccurred = true;
            });

            migrationList.ForEach(migration =>
            {
                var index = clip.Events.FindIndex(state => state.Module == migration.Item1);
                if (index == -1) return;

                clip.Events[index].Module = migration.Item2;
                migrationOccurred = true;
            });

            newClip.AssociatedModules.AddRange(clip.AssociatedModules.Where(serialisedModuleName => appManager.ModuleManager.DoesModuleExist(serialisedModuleName)));

            clip.States.Where(clipState => clipState.States.All(pair => appManager.ModuleManager.DoesModuleExist(pair.Module))).ForEach(clipState =>
            {
                if (clipState.States.All(pair => appManager.ModuleManager.IsModuleLoaded(pair.Module)))
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

            clip.Events.Where(clipEvent => appManager.ModuleManager.DoesModuleExist(clipEvent.Module)).ForEach(clipEvent =>
            {
                if (appManager.ModuleManager.IsModuleLoaded(clipEvent.Module))
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

        appManager.ChatBoxManager.Clips.ReplaceItems(createdClips);
        appManager.ChatBoxManager.SetTimelineLength(TimeSpan.FromTicks(data.Ticks));

        return migrationOccurred;
    }
}
