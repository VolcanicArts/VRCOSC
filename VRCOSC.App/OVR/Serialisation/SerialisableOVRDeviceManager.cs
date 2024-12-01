// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VRCOSC.App.SDK.OVR;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.OVR.Serialisation;

[JsonObject(MemberSerialization.OptIn)]
public class SerialisableOVRDeviceManager : SerialisableVersion
{
    [JsonProperty("device_roles")]
    public Dictionary<string, DeviceRole> DeviceRoles = [];

    [JsonConstructor]
    public SerialisableOVRDeviceManager()
    {
    }

    public SerialisableOVRDeviceManager(OVRDeviceManager deviceManager)
    {
        Version = 1;

        DeviceRoles.AddRange(deviceManager.TrackedDevices.Values.Select(trackedDevice => new KeyValuePair<string, DeviceRole>(trackedDevice.SerialNumber, trackedDevice.Role)));
    }
}
