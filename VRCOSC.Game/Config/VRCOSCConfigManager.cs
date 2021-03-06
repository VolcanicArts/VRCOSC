// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Configuration;
using osu.Framework.Platform;
using VRCOSC.Game.Modules;

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
        SetDefault(VRCOSCSetting.Dropdowns, (int)((Math.Pow(2, Enum.GetValues(typeof(ModuleType)).Length) - 1)));
    }
}

public enum VRCOSCSetting
{
    AutoStartStop,
    IPAddress,
    SendPort,
    ReceivePort,
    Dropdowns
}
