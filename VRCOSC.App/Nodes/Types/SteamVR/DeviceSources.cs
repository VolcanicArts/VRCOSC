// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.OVR.Device;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.Nodes.Types.SteamVR;

[Node("HMD", "SteamVR/Devices")]
public sealed class SteamVRHMDSourceNode : UpdateNode<HMD>
{
    public ValueOutput<HMD> Device = new();

    protected override void Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OVRClient.GetHMD(), c);
    }

    protected override HMD GetValue(PulseContext c) => AppManager.GetInstance().OVRClient.GetHMD();
}

[Node("Left Controller", "SteamVR/Devices")]
public sealed class SteamVRLeftControllerSourceNode : UpdateNode<Controller>
{
    public ValueOutput<Controller> Device = new();

    protected override void Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OVRClient.GetLeftController(), c);
    }

    protected override Controller GetValue(PulseContext c) => AppManager.GetInstance().OVRClient.GetLeftController();
}

[Node("Right Controller", "SteamVR/Devices")]
public sealed class SteamVRRightControllerSourceNode : UpdateNode<Controller>
{
    public ValueOutput<Controller> Device = new();

    protected override void Process(PulseContext c)
    {
        Device.Write(AppManager.GetInstance().OVRClient.GetRightController(), c);
    }

    protected override Controller GetValue(PulseContext c) => AppManager.GetInstance().OVRClient.GetRightController();
}