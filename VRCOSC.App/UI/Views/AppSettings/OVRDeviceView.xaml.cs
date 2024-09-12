// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Windows.Controls;
using VRCOSC.App.OVR;
using VRCOSC.App.SDK.OVR;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.AppSettings;

public partial class OVRDeviceView
{
    public ObservableDictionary<string, DeviceRole> TrackedDevices { get; } = [];
    public IEnumerable<DeviceRole> ComboBoxSource => Enum.GetValues<DeviceRole>();

    public OVRDeviceView()
    {
        OVRDeviceManager.GetInstance().TrackedDevices.OnCollectionChanged((newItems, _) =>
        {
            foreach (var pair in newItems)
            {
                TrackedDevices.Add(pair.Key, pair.Value.Role);
            }
        }, true);

        InitializeComponent();

        DataContext = this;
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var element = (ComboBox)sender;
        var serialNumber = (string)element.Tag;

        OVRDeviceManager.GetInstance().TrackedDevices[serialNumber].Role = (DeviceRole)element.SelectedItem;
    }
}

