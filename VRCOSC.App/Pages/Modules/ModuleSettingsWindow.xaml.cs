// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Pages.Modules;

public partial class ModuleSettingsWindow
{
    private readonly Repeater updateTask;

    public ModuleSettingsWindow(Module module)
    {
        InitializeComponent();

        Title = $"{module.Title}'s Settings";

        DataContext = module;

        updateTask = new Repeater(() => module.Settings.ForEach(pair => pair.Value.CheckIsEnabled()));
        updateTask.Start(TimeSpan.FromSeconds(1d / 10d));
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        _ = updateTask.StopAsync();
    }
}
