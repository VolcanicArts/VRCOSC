// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VRCOSC.App.ChatBox.Clips;
using VRCOSC.App.ChatBox.Clips.Variables;
using VRCOSC.App.Settings;
using VRCOSC.App.Utils;

namespace VRCOSC.App.ChatBox;

public class ChatBoxManager
{
    private static ChatBoxManager? instance;
    internal static ChatBoxManager GetInstance() => instance ??= new ChatBoxManager();

    public ObservableCollection<ClipStateReference> StateReferences = new();
    public ObservableCollection<ClipEventReference> EventReferences = new();
    public ObservableCollection<ClipVariableReference> VariableReferences = new();

    public Timeline Timeline { get; } = new();

    private DateTimeOffset startTime;
    private Repeater? sendTask;

    public bool SendEnabled { get; set; }

    public ChatBoxManager()
    {
        Timeline.Layers.Add(new Layer());
        Timeline.Layers.Add(new Layer());
        Timeline.Layers.Add(new Layer());
        Timeline.Layers.Add(new Layer());
        Timeline.Layers.Add(new Layer());
        Timeline.Layers.Add(new Layer());
        Timeline.Layers.Add(new Layer());
        Timeline.Layers.Add(new Layer());

        Timeline.Layers[0].Clips.Add(new Clip
        {
            Name = { Value = "Media" },
            Start = { Value = 0 },
            End = { Value = 20 }
        });
        Timeline.Layers[0].Clips.Add(new Clip
        {
            Name = { Value = "Time" },
            Start = { Value = 40 },
            End = { Value = 60 }
        });
        Timeline.Layers[1].Clips.Add(new Clip
        {
            Name = { Value = "AFK" },
            Start = { Value = 0 },
            End = { Value = 40 }
        });
        Timeline.Layers[2].Clips.Add(new Clip
        {
            Name = { Value = "Weather" },
            Start = { Value = 0 },
            End = { Value = 30 }
        });
    }

    public void Start()
    {
        var sendInterval = SettingsManager.GetInstance().GetValue<int>(VRCOSCSetting.ChatBoxSendInterval);

        startTime = DateTimeOffset.Now;
        sendTask = new Repeater(update);

        sendTask.Start(TimeSpan.FromMilliseconds(sendInterval));
    }

    public async void Stop()
    {
        await (sendTask?.StopAsync() ?? Task.CompletedTask);
    }

    private void update()
    {
        if (!SendEnabled)
        {
            clearChatBox();
            return;
        }
    }

    private void clearChatBox()
    {
    }

    #region States

    public void CreateState(ClipStateReference reference)
    {
        StateReferences.Add(reference);
    }

    public void DeleteState(string moduleID, string stateID)
    {
        var stateToDelete = GetState(moduleID, stateID);
        if (stateToDelete is null) return;

        StateReferences.Remove(stateToDelete);
    }

    public ClipStateReference? GetState(string moduleID, string stateID)
    {
        return StateReferences.FirstOrDefault(reference => reference.ModuleID == moduleID && reference.StateID == stateID);
    }

    #endregion

    #region Events

    public void CreateEvent(ClipEventReference reference)
    {
        EventReferences.Add(reference);
    }

    public void DeleteEvent(string moduleID, string eventID)
    {
        var eventToDelete = GetEvent(moduleID, eventID);
        if (eventToDelete is null) return;

        EventReferences.Remove(eventToDelete);
    }

    public ClipEventReference? GetEvent(string moduleID, string eventID)
    {
        return EventReferences.FirstOrDefault(reference => reference.ModuleID == moduleID && reference.EventID == eventID);
    }

    #endregion

    #region Variables

    public void CreateVariable(ClipVariableReference reference)
    {
        VariableReferences.Add(reference);
    }

    public void DeleteVariable(string moduleID, string variableID)
    {
        var variableToDelete = GetVariable(moduleID, variableID);
        if (variableToDelete is null) return;

        VariableReferences.Remove(variableToDelete);
    }

    public ClipVariableReference? GetVariable(string moduleID, string variableID)
    {
        return VariableReferences.FirstOrDefault(reference => reference.ModuleID == moduleID && reference.VariableID == variableID);
    }

    #endregion
}
