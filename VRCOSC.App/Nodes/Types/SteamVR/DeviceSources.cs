// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.Nodes.Types.Base;
using VRCOSC.App.SDK.OVR.Device;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.Nodes.Types.SteamVR;

[Node("HMD", "SteamVR/Devices")]
public sealed class SteamVRHMDSourceNode : SourceNode<string>
{
    public ValueOutput<HMD> Device = new();

    protected override void Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OVRClient.GetHMD(), c);
    }

    protected override string GetValue(PulseContext c) => AppManager.GetInstance().OVRClient.GetHMD().SerialNumber;
}

[Node("Left Controller", "SteamVR/Devices")]
public sealed class SteamVRLeftControllerSourceNode : SourceNode<string>
{
    public ValueOutput<Controller> Device = new();

    protected override void Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OVRClient.GetLeftController(), c);
    }

    protected override string GetValue(PulseContext c) => AppManager.GetInstance().OVRClient.GetLeftController().SerialNumber;
}

[Node("Right Controller", "SteamVR/Devices")]
public sealed class SteamVRRightControllerSourceNode : SourceNode<string>
{
    public ValueOutput<Controller> Device = new();

    protected override void Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OVRClient.GetRightController(), c);
    }

    protected override string GetValue(PulseContext c) => AppManager.GetInstance().OVRClient.GetRightController().SerialNumber;
}