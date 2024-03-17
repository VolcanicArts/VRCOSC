// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.Settings;

// ReSharper disable UnusedMember.Global

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

    public bool VRCAutoStart
    {
        get => (bool)SettingsManager.GetInstance().GetObservable(VRCOSCSetting.VRCAutoStart).Value;
        set => SettingsManager.GetInstance().GetObservable(VRCOSCSetting.VRCAutoStart).Value = value;
    }

    public bool VRCAutoStop
    {
        get => (bool)SettingsManager.GetInstance().GetObservable(VRCOSCSetting.VRCAutoStop).Value;
        set => SettingsManager.GetInstance().GetObservable(VRCOSCSetting.VRCAutoStop).Value = value;
    }

    public bool ModuleLogDebug
    {
        get => (bool)SettingsManager.GetInstance().GetObservable(VRCOSCSetting.ModuleLogDebug).Value;
        set => SettingsManager.GetInstance().GetObservable(VRCOSCSetting.ModuleLogDebug).Value = value;
    }

    public SettingsPage()
    {
        InitializeComponent();

        DataContext = this;
    }
}
