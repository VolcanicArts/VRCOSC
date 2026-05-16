// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.OpenVR.Device;

namespace VRCOSC.App.Nodes.Types.SteamVR;

[Node("Trigger Haptic", "SteamVR")]
public sealed class SteamVRTriggerHapticNode : ActionNode
{
    public ValueInput<TrackedDevice> Device = new();
    public ValueInput<float> DurationSeconds = new();
    public ValueInput<float> Frequency = new();
    public ValueInput<float> Amplitude = new();

    protected override void DoAction(IPulseContext c)
    {
        var device = Device.Read(c);
        if (device is null) return;

        var duration = DurationSeconds.Read(c);
        if (duration == 0) return;

        var frequency = Frequency.Read(c);
        frequency = float.Clamp(frequency, 0f, 1f);
        frequency *= 100;

        var amplitude = Amplitude.Read(c);
        amplitude = float.Clamp(amplitude, 0f, 1f);

        AppManager.GetInstance().OpenVRManager.TriggerHaptic(device, duration, frequency, amplitude);
    }
}

[Node("Shutdown Device", "SteamVR")]
public sealed class SteamVRShutdownDeviceNode() : ActionValueConsumeNode<TrackedDevice?>("Device")
{
    protected override void ConsumeValue(TrackedDevice? device, IPulseContext c)
    {
        if (device is null) return;

        AppManager.GetInstance().SteamVRManager.ShutdownDevice(device);
    }
}