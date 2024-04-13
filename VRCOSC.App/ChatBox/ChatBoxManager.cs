// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using VRCOSC.App.ChatBox.Clips;
using VRCOSC.App.ChatBox.Clips.Variables;
using VRCOSC.App.ChatBox.Serialisation;
using VRCOSC.App.Modules;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.Profiles;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Settings;
using VRCOSC.App.Utils;

namespace VRCOSC.App.ChatBox;

public class ChatBoxManager : INotifyPropertyChanged
{
    private static ChatBoxManager? instance;
    internal static ChatBoxManager GetInstance() => instance ??= new ChatBoxManager();

    public ObservableCollection<ClipStateReference> StateReferences = new();
    public ObservableCollection<ClipEventReference> EventReferences = new();
    public ObservableCollection<ClipVariableReference> VariableReferences = new();

    //public readonly Dictionary<(string, string), string?> VariableValues = new();

    // moduleID, stateID
    public readonly Dictionary<string, string?> StateValues = new();

    // moduleID, eventID
    public readonly List<(string, string)> TriggeredEvents = new();

    public Timeline Timeline { get; } = new();

    public float CurrentPercentage => ((DateTimeOffset.Now - startTime).Ticks % Timeline.Length.Value.Ticks) / (float)Timeline.Length.Value.Ticks;
    public int CurrentSecond => (int)Math.Floor((DateTimeOffset.Now - startTime).TotalSeconds) % Timeline.LengthSeconds;
    private DateTimeOffset startTime;

    private Repeater? sendTask;
    private Repeater? updateTask;
    private bool isClear;

    public bool SendEnabled { get; set; }

    private IEnumerable<Clip> allClips => Timeline.Layers.SelectMany(layer => layer.Clips);

    public Visibility ShowIndicator => AppManager.GetInstance().State.Value == AppManagerState.Started ? Visibility.Visible : Visibility.Collapsed;

    private readonly SerialisationManager serialisationManager;

    public ChatBoxManager()
    {
        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(1, new ChatBoxSerialiser(AppManager.GetInstance().Storage, this, ProfileManager.GetInstance().ActiveProfile));
    }

    public void Load()
    {
        Deserialise();
    }

    public void ClearAllReferences()
    {
        StateReferences.Clear();
        EventReferences.Clear();
        VariableReferences.Clear();
    }

    public void Serialise()
    {
        serialisationManager.Serialise();
    }

    public void Deserialise(string filePathOverride = "")
    {
        Timeline.Layers.ForEach(layer => layer.Clips.Clear());

        serialisationManager.Deserialise(string.IsNullOrEmpty(filePathOverride), filePathOverride);

        Timeline.Init();

        if (!string.IsNullOrEmpty(filePathOverride)) Serialise();
    }

    public void Start()
    {
        var sendInterval = SettingsManager.GetInstance().GetValue<int>(VRCOSCSetting.ChatBoxSendInterval);

        startTime = DateTimeOffset.Now;
        sendTask = new Repeater(chatBoxUpdate);
        updateTask = new Repeater(update);
        SendEnabled = true;
        isClear = true;

        sendTask.Start(TimeSpan.FromMilliseconds(sendInterval));
        updateTask.Start(TimeSpan.FromSeconds(1f / 60f));

        allClips.ForEach(clip => clip.ChatBoxStart());

        foreach (var pair in StateValues)
        {
            StateValues[pair.Key] = null;
        }

        TriggeredEvents.Clear();
    }

    public async void Stop()
    {
        await (sendTask?.StopAsync() ?? Task.CompletedTask);
        await (updateTask?.StopAsync() ?? Task.CompletedTask);
        sendTask = null;
        updateTask = null;
        clearChatBox();
    }

    private void update()
    {
        OnPropertyChanged(nameof(CurrentPercentage));
        OnPropertyChanged(nameof(ShowIndicator));
    }

    private void chatBoxUpdate()
    {
        ModuleManager.GetInstance().ChatBoxUpdate();

        allClips.ForEach(clip => clip.Update());
        TriggeredEvents.Clear();

        evaluateClips();
    }

    private void evaluateClips()
    {
        var validClip = getValidClip();
        handleClip(validClip);

        // if (CurrentWorldExtractor.IsCurrentWorldBlacklisted && configManager.Get<bool>(VRCOSCSetting.ChatboxWorldBlock) && validClip is not null)
        // {
        //     var blockedFromSending = validClip.AssociatedModules.Any(moduleId => worldBlacklistModuleBlocklist.Contains(moduleId));
        //
        //     if (blockedFromSending)
        //     {
        //         handleClip(null);
        //         nextValidTime += TimeSpan.FromMilliseconds(SendDelay.Value);
        //         return;
        //     }
        // }
    }

    private Clip? getValidClip()
    {
        return allClips.FirstOrDefault(clip => clip.Evaluate());
    }

    private void handleClip(Clip? clip)
    {
        if (!SendEnabled || clip is null)
        {
            clearChatBox();
            return;
        }

        isClear = false;
        sendText(clip.GetFormattedText());
    }

    private void sendText(string text)
    {
        var finalText = convertSpecialCharacters(text);
        AppManager.GetInstance().VRChatOscClient.SendValues(VRChatOscConstants.ADDRESS_CHATBOX_INPUT, [finalText, true, false]);
    }

    private static string convertSpecialCharacters(string input)
    {
        // Convert new line to vertical tab
        return input.Replace("\n", "\v");
    }

    private void clearChatBox()
    {
        if (isClear) return;

        sendText(string.Empty);
        isClear = true;
    }

    public void SetTyping(bool typing)
    {
        AppManager.GetInstance().VRChatOscClient.SendValue(VRChatOscConstants.ADDRESS_CHATBOX_TYPING, typing);
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

    public void ChangeStateTo(string moduleID, string stateID)
    {
        StateValues[moduleID] = stateID;
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

    public void TriggerEvent(string moduleID, string eventID)
    {
        if (TriggeredEvents.Contains((moduleID, eventID))) return;

        TriggeredEvents.Add((moduleID, eventID));
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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
