// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.SDK.Modules.Attributes.Settings;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Pages.Modules;

public partial class ModuleSettingsWindow
{
    public Module Module { get; }

    private readonly Repeater updateTask;

    public ModuleSettingsWindow(Module module)
    {
        InitializeComponent();

        Title = $"{module.Title.Pluralise()} Settings";

        Module = module;
        DataContext = this;

        updateTask = new Repeater(() => module.Settings.ForEach(pair => pair.Value.CheckIsEnabled()));
        updateTask.Start(TimeSpan.FromSeconds(1d / 10d));
    }

    public Dictionary<string, List<ModuleSetting>> GroupsFormatted
    {
        get
        {
            var settingsInGroup = new List<string>();

            var groupsFormatted = new Dictionary<string, List<ModuleSetting>>();

            Module.Groups.ForEach(pair =>
            {
                var moduleSettings = new List<ModuleSetting>();
                pair.Value.ForEach(moduleSettingLookup =>
                {
                    moduleSettings.Add(Module.Settings[moduleSettingLookup]);
                    settingsInGroup.Add(moduleSettingLookup);
                });
                groupsFormatted.Add(pair.Key, moduleSettings);
            });

            var miscModuleSettings = new List<ModuleSetting>();
            Module.Settings.Where(pair => !settingsInGroup.Contains(pair.Key)).ForEach(pair => miscModuleSettings.Add(pair.Value));
            if (miscModuleSettings.Any()) groupsFormatted.Add("Miscellaneous", miscModuleSettings);

            return groupsFormatted;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        _ = updateTask.StopAsync();
    }
}
