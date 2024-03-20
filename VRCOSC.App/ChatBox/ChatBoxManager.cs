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

    private const int default_timeline_layer_count = 8;
    private const int default_timeline_length_seconds = 60;

    public ObservableCollection<ClipVariableReference> VariableReferences = new();

    public ObservableCollection<Clip> Clips { get; } = new();

    public Observable<int> TimelineLayerCount { get; } = new(default_timeline_layer_count);
    public Observable<TimeSpan> TimelineLength { get; } = new(TimeSpan.FromSeconds(default_timeline_length_seconds));

    private DateTimeOffset startTime;
    private Repeater? sendTask;

    public bool SendEnabled { get; set; }

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

    #region References

    public void CreateVariable(ClipVariableReference reference)
    {
        VariableReferences.Add(reference);
    }

    public void DeleteVariable(string moduleID, string variableID)
    {
        var variableToDelete = VariableReferences.FirstOrDefault(reference => reference.ModuleID == moduleID && reference.VariableID == variableID);
        if (variableToDelete is null) return;

        VariableReferences.Remove(variableToDelete);
    }

    public ClipVariableReference? GetVariable(string moduleID, string variableID)
    {
        return VariableReferences.FirstOrDefault(reference => reference.ModuleID == moduleID && reference.VariableID == variableID);
    }

    #endregion
}
