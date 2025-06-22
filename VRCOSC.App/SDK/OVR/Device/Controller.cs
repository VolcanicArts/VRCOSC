// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.OVR.Device;

public record Controller : TrackedDevice
{
    public Controller(string serialNumber)
        : base(serialNumber)
    {
    }

    public InputState Input;
}