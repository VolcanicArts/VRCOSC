// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using VRCOSC.App.ChatBox.Clips.Variables;
using VRCOSC.App.Modules;
using VRCOSC.App.Settings;
using VRCOSC.App.UI.Windows;
using VRCOSC.App.Utils;

namespace VRCOSC.App.ChatBox.Clips;

public class ClipEvent : ClipElement
{
    public string ModuleID { get; init; } = null!;
    public string EventID { get; init; } = null!;

    public Observable<float> Length { get; } = new();
    public Observable<ClipEventBehaviour> Behaviour = new();

    public int BehaviourIndex
    {
        get => (int)Behaviour.Value;
        set => Behaviour.Value = (ClipEventBehaviour)value;
    }

    public Array BehaviourSource => typeof(ClipEventBehaviour).GetEnumValues();

    public override string DisplayName
    {
        get
        {
            var module = ModuleManager.GetInstance().GetModuleOfID(ModuleID);
            var eventReference = ChatBoxManager.GetInstance().GetEvent(ModuleID, EventID);
            Debug.Assert(eventReference is not null);

            return module.Title + " - " + eventReference.DisplayName.Value;
        }
    }

    public override bool ShouldBeVisible
    {
        get
        {
            if (!SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.ShowRelevantModules)) return true;

            var selectedClip = MainWindow.GetInstance().ChatBoxView.SelectedClip;
            Debug.Assert(selectedClip is not null);

            var enabledModuleIDs = ModuleManager.GetInstance().GetEnabledModuleIDs().Where(moduleID => selectedClip.LinkedModules.Contains(moduleID)).OrderBy(moduleID => moduleID);
            return enabledModuleIDs.Contains(ModuleID);
        }
    }

    public override bool IsDefault => base.IsDefault && Length.IsDefault && Behaviour.IsDefault;

    [JsonConstructor]
    public ClipEvent()
    {
    }

    public ClipEvent(ClipEventReference reference)
    {
        ModuleID = reference.ModuleID;
        EventID = reference.EventID;
        Format = new Observable<string>(reference.DefaultFormat);
        ShowTyping = new Observable<bool>(reference.DefaultShowTyping);
        Variables = new ObservableCollection<ClipVariable>(reference.DefaultVariables.Select(clipVariableReference => clipVariableReference.CreateInstance()));
        Length = new Observable<float>(reference.DefaultLength);
        Behaviour = new Observable<ClipEventBehaviour>(reference.DefaultBehaviour);
    }
}

public enum ClipEventBehaviour
{
    /// <summary>
    /// Always override the current event with this one
    /// </summary>
    Override,

    /// <summary>
    /// Queue this event to run after the current event has finished.
    /// The current event includes any overrides that may occur while the queue is waiting
    /// </summary>
    Queue,

    /// <summary>
    /// Ignore this event if there is already a current event
    /// </summary>
    Ignore
}

/// <summary>
/// Used as a reference for what events a module has created.
/// </summary>
public class ClipEventReference
{
    internal string ModuleID { get; init; } = null!;
    internal string EventID { get; init; } = null!;
    internal string DefaultFormat { get; init; } = string.Empty;
    internal bool DefaultShowTyping { get; init; } = false;
    internal List<ClipVariableReference> DefaultVariables { get; init; } = new();
    internal float DefaultLength { get; init; }
    internal ClipEventBehaviour DefaultBehaviour { get; init; }

    public Observable<string> DisplayName { get; } = new("INVALID");
}
