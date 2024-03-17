// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.Settings;

namespace VRCOSC.App.Pages.Settings;

public partial class SettingsPage
{
    public bool UseLegacyPorts
    {
        get => (bool)SettingsManager.GetInstance().GetObservable(VRCOSCSetting.UseLegacyPorts).Value;
        set => SettingsManager.GetInstance().GetObservable(VRCOSCSetting.UseLegacyPorts).Value = value;
    }

    public bool AllowPreReleasePackages
    {
        get => (bool)SettingsManager.GetInstance().GetObservable(VRCOSCSetting.AllowPreReleasePackages).Value;
        set => SettingsManager.GetInstance().GetObservable(VRCOSCSetting.AllowPreReleasePackages).Value = value;
    }

    public SettingsPage()
    {
        InitializeComponent();

        DataContext = this;
    }
}
