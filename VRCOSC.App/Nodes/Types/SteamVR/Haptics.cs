// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.SDK.OVR.Device;

namespace VRCOSC.App.Nodes.Types.SteamVR;

[Node("Trigger Haptic", "SteamVR")]
public sealed class SteamVRTriggerHaptic : Node, IFlowInput
{
    public FlowContinuation Next = new();

    public ValueInput<TrackedDevice> Device = new();
    public ValueInput<float> DurationSeconds = new("Duration Seconds");
    public ValueInput<float> Frequency = new();
    public ValueInput<float> Amplitude = new();

    protected override async Task Process(PulseContext c)
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

        AppManager.GetInstance().OVRClient.TriggerHaptic(device, duration, frequency, amplitude);
        await Next.Execute(c);
    }
}