// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.SDK.Modules.Attributes.Settings;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Pages.Modules;

public partial class ModuleSettingsWindow
{
    public Module Module { get; }

    public ModuleSettingsWindow(Module module)
    {
        InitializeComponent();

        Title = $"{module.Title.Pluralise()} Settings";

        Module = module;
        DataContext = this;
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
}
