// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Net;
using osu.Framework.Configuration;
using osu.Framework.Platform;
using VRCOSC.Game.Graphics.Settings;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Config;

public sealed class VRCOSCConfigManager : IniConfigManager<VRCOSCSetting>
{
    protected override string Filename => @"config.ini";

    public VRCOSCConfigManager(Storage storage)
        : base(storage)
    {
    }

    protected override void InitialiseDefaults()
    {
        SetDefault(VRCOSCSetting.Version, string.Empty);
        SetDefault(VRCOSCSetting.AutoStartStop, false);
        SetDefault(VRCOSCSetting.SendAddress, IPAddress.Loopback.ToString());
        SetDefault(VRCOSCSetting.SendPort, 9000);
        SetDefault(VRCOSCSetting.ReceiveAddress, IPAddress.Loopback.ToString());
        SetDefault(VRCOSCSetting.ReceivePort, 9001);
        SetDefault(VRCOSCSetting.UpdateMode, UpdateMode.Auto);
        SetDefault(VRCOSCSetting.Theme, VRCOSCTheme.Dark);
        SetDefault(VRCOSCSetting.ChatBoxTimeSpan, 1500);
        SetDefault(VRCOSCSetting.AutoStopOpenVR, false);
        SetDefault(VRCOSCSetting.AutoStartOpenVR, false);
        SetDefault(VRCOSCSetting.WindowState, WindowState.Maximised);
        SetDefault(VRCOSCSetting.UIScale, 1f);
    }
}

public enum VRCOSCSetting
{
    Version,
    AutoStartStop,
    SendAddress,
    SendPort,
    ReceiveAddress,
    ReceivePort,
    UpdateMode,
    Theme,
    ChatBoxTimeSpan,
    AutoStopOpenVR,
    AutoStartOpenVR,
    WindowState,
    UIScale
}
