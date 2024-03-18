// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.OVR.Input;

namespace VRCOSC.App.SDK.OVR.Device;

public class Controller : OVRDevice
{
    public readonly InputStates Input = new();
}
