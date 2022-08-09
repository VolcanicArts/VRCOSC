// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Configuration;
using osu.Framework.Platform;

namespace VRCOSC.Game.Config;

public class VRCOSCConfigManager : IniConfigManager<VRCOSCSetting>
{
    protected override string Filename => @"config.ini";

    public VRCOSCConfigManager(Storage storage)
        : base(storage)
    {
    }

    protected override void InitialiseDefaults()
    {
        SetDefault(VRCOSCSetting.AutoStartStop, false);
        SetDefault(VRCOSCSetting.IPAddress, "127.0.0.1");
        SetDefault(VRCOSCSetting.SendPort, 9000);
        SetDefault(VRCOSCSetting.ReceivePort, 9001);
    }
}

public enum VRCOSCSetting
{
    AutoStartStop,
    IPAddress,
    SendPort,
    ReceivePort
}
