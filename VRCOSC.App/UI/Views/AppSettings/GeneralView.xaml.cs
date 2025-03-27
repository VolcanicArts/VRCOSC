// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using VRCOSC.App.Settings;
using VRCOSC.App.UI.Themes;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.AppSettings;

public partial class GeneralView
{
    public GeneralView()
    {
        InitializeComponent();
        DataContext = this;
    }

    public IEnumerable<Theme> ThemeSource => Enum.GetValues<Theme>();

    public Observable<Theme> Theme => SettingsManager.GetInstance().GetObservable<Theme>(VRCOSCSetting.Theme);
    public Observable<bool> StartInTray => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.StartInTray);
    public Observable<bool> TrayOnClose => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.TrayOnClose);
}