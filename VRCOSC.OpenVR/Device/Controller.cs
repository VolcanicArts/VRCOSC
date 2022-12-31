// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Valve.VR;
using VRCOSC.OpenVR.Input;

namespace VRCOSC.OpenVR.Device;

public class Controller : OVRDevice
{
    public readonly InputStates Input = new();
    public readonly ETrackedControllerRole Role;

    public Controller(uint id)
        : base(id)
    {
        Role = Valve.VR.OpenVR.System.GetControllerRoleForTrackedDeviceIndex(id);
    }
}
