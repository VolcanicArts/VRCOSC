// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Configuration;
using osu.Framework.Platform;
using VRCOSC.Game.Screens.Main.Repo;

namespace VRCOSC.Game.Config;

public class VRCOSCConfigManager : IniConfigManager<VRCOSCSetting>
{
    protected override string Filename => "app.ini";

    public VRCOSCConfigManager(Storage storage)
        : base(storage)
    {
    }

    protected override void InitialiseDefaults()
    {
        SetDefault(VRCOSCSetting.FirstTimeSetupComplete, false);
        SetDefault(VRCOSCSetting.StartInTray, false);
        SetDefault(VRCOSCSetting.PackageFilter, (int)(PackageListingFilter.Type_Official | PackageListingFilter.Type_Curated | PackageListingFilter.Type_Community));
        SetDefault(VRCOSCSetting.AutomaticProfileSwitching, false);
        SetDefault(VRCOSCSetting.ModuleLogDebug, false);
        SetDefault(VRCOSCSetting.AutoStartStop, false);
    }
}

public enum VRCOSCSetting
{
    FirstTimeSetupComplete,
    StartInTray,
    PackageFilter,
    AutomaticProfileSwitching,
    ModuleLogDebug,
    AutoStartStop
}
