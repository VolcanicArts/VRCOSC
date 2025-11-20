// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.Settings;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.AppSettings;

public partial class SteamVRView
{
    public SteamVRView()
    {
        InitializeComponent();
        DataContext = this;
    }

    public Observable<bool> SteamVRAutoOpen => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.OVRAutoOpen);
    public Observable<bool> SteamVRAutoClose => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.OVRAutoClose);
}