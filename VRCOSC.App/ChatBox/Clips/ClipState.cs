// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using VRCOSC.App.Modules;
using VRCOSC.App.Settings;
using VRCOSC.App.Utils;

namespace VRCOSC.App.ChatBox.Clips;

public class ClipState : ClipElement
{
    // ModuleID, StateID
    public Dictionary<string, string> States { get; } = new();

    public override string DisplayName
    {
        get
        {
            var names = new List<string>();

            States.OrderBy(pair => pair.Key).ThenBy(pair => pair.Value).ForEach(pair =>
            {
                var module = ModuleManager.GetInstance().GetModuleOfID(pair.Key);
                var stateReference = ChatBoxManager.GetInstance().GetState(pair.Key, pair.Value);
                Debug.Assert(stateReference is not null);

                var name = module.Title;

                if (!string.Equals(stateReference.StateID, "default", StringComparison.InvariantCultureIgnoreCase))
                    name += $" ({stateReference.DisplayName.Value})";

                names.Add(name);
            });

            return string.Join(" & ", names);
        }
    }

    public override bool ShouldBeVisible
    {
        get
        {
            if (!SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.ShowRelevantModules)) return true;

            var selectedClip = MainWindow.GetInstance().ChatBoxPage.SelectedClip;
            Debug.Assert(selectedClip is not null);

            var enabledModuleIDs = ModuleManager.GetInstance().GetEnabledModuleIDs().Where(moduleID => selectedClip.LinkedModules.Contains(moduleID)).OrderBy(moduleID => moduleID);
            var clipStateModuleIDs = States.Select(pair => pair.Key).OrderBy(s => s);
            return enabledModuleIDs.SequenceEqual(clipStateModuleIDs);
        }
    }

    [JsonConstructor]
    public ClipState()
    {
    }

    public ClipState(ClipStateReference reference)
    {
        States = new Dictionary<string, string> { { reference.ModuleID, reference.StateID } };
        Format = new Observable<string>(reference.DefaultFormat);
    }

    public ClipState(ClipState original)
    {
        States.AddRange(original.States);
    }

    public ClipState Clone() => new(this);
}

/// <summary>
/// Used as a reference for what states a module has created.
/// </summary>
public class ClipStateReference
{
    internal string ModuleID { get; init; }
    internal string StateID { get; init; }
    internal string DefaultFormat { get; init; }

    public Observable<string> DisplayName { get; } = new("INVALID");
}
