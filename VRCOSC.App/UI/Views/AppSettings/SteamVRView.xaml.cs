// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Windows.Controls;
using VRCOSC.App.OVR;
using VRCOSC.App.SDK.OVR;
using VRCOSC.App.SDK.OVR.Device;
using VRCOSC.App.Settings;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.AppSettings;

public partial class SteamVRView
{
    public ObservableDictionary<string, TrackedDevice> TrackedDevices { get; } = [];
    public IEnumerable<DeviceRole> DeviceRoleSource => Enum.GetValues<DeviceRole>();

    public SteamVRView()
    {
        InitializeComponent();
        DataContext = this;

        OVRDeviceManager.GetInstance().TrackedDevices.OnCollectionChanged((_, _) => Dispatcher.Invoke(() =>
        {
            TrackedDevices.Clear();

            foreach (var pair in OVRDeviceManager.GetInstance().TrackedDevices)
            {
                TrackedDevices.Add(pair.Key, pair.Value);
            }
        }), true);
    }

    public Observable<bool> SteamVRAutoOpen => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.OVRAutoOpen);
    public Observable<bool> SteamVRAutoClose => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.OVRAutoClose);

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var element = (ComboBox)sender;
        var serialNumber = (string)element.Tag;

        OVRDeviceManager.GetInstance().AddOrUpdateDeviceRole(serialNumber, (DeviceRole)element.SelectedItem);
    }
}