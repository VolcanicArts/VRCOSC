// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.SDK.Modules.Attributes.Settings;
using VRCOSC.App.UI.Core;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Windows.Modules;

public partial class ModuleSettingsWindow : IManagedWindow
{
    public Module Module { get; }

    public ModuleSettingsWindow(Module module)
    {
        InitializeComponent();

        Title = $"{module.Title.Pluralise()} Settings";

        Module = module;
        DataContext = this;
    }

    public List<object> GroupsFormatted
    {
        get
        {
            var settingsInGroup = new List<string>();

            var groupsFormatted = new List<object>();

            Module.Groups.ForEach(group =>
            {
                var moduleSettings = new List<ModuleSetting>();

                group.Settings.ForEach(moduleSettingLookup =>
                {
                    moduleSettings.Add(Module.Settings[moduleSettingLookup]);
                    settingsInGroup.Add(moduleSettingLookup);
                });
                groupsFormatted.Add(new SettingsGroupFormatted(group.Title, group.Description, moduleSettings));
            });

            var miscModuleSettings = new List<ModuleSetting>();
            Module.Settings.Where(pair => !settingsInGroup.Contains(pair.Key)).ForEach(pair => miscModuleSettings.Add(pair.Value));
            if (miscModuleSettings.Count != 0) groupsFormatted.Add(new SettingsGroupFormatted("Miscellaneous", string.Empty, miscModuleSettings));

            return groupsFormatted;
        }
    }

    private void ModuleSettingsWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        Module.Serialise();
    }

    public object GetComparer() => Module;
}

internal class StringToVerticalAlignmentConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string strValue) return VerticalAlignment.Stretch;

        return string.IsNullOrEmpty(strValue) ? VerticalAlignment.Center : VerticalAlignment.Top;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

public class SettingStyleSelector : StyleSelector
{
    public Style? Style1 { get; set; }

    public override Style? SelectStyle(object? item, DependencyObject container) => item switch
    {
        SettingsGroupFormatted => Style1,
        _ => base.SelectStyle(item, container)
    };
}

public record SettingsGroupFormatted(string Title, string Description, List<ModuleSetting> Settings);