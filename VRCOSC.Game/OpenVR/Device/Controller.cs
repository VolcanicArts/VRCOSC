// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.OpenVR.Input;

namespace VRCOSC.Game.OpenVR.Device;

public class Controller : OVRDevice
{
    public readonly InputStates Input = new();
}
