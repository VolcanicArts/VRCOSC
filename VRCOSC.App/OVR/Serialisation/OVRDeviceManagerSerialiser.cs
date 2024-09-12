// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.OVR.Device;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.OVR.Serialisation;

internal class OVRDeviceManagerSerialiser : Serialiser<OVRDeviceManager, SerialisableOVRDeviceManager>
{
    protected override string Directory => "configuration";
    protected override string FileName => "openvr.json";

    public OVRDeviceManagerSerialiser(Storage storage, OVRDeviceManager reference)
        : base(storage, reference)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableOVRDeviceManager data)
    {
        foreach (var (serialNumber, deviceRole) in data.DeviceRoles)
        {
            Reference.TrackedDevices.Add(serialNumber, new TrackedDevice
            {
                SerialNumber = serialNumber,
                Role = deviceRole
            });
        }

        return false;
    }
}
