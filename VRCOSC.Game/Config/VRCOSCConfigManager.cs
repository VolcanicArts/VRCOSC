// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Configuration;
using osu.Framework.Platform;
using VRCOSC.Screens.Main.Repo;

namespace VRCOSC.Config;

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
        SetDefault(VRCOSCSetting.TrayOnClose, false);
        SetDefault(VRCOSCSetting.AllowPreReleasePackages, true); // TODO: False on release
        SetDefault(VRCOSCSetting.PackageFilter, (int)(PackageListingFilter.Type_Official | PackageListingFilter.Type_Curated | PackageListingFilter.Type_Community)); // TODO: Remove community on release
        SetDefault(VRCOSCSetting.AutomaticProfileSwitching, false);
        SetDefault(VRCOSCSetting.ModuleLogDebug, false);
        SetDefault(VRCOSCSetting.VRCAutoStart, false);
        SetDefault(VRCOSCSetting.VRCAutoStop, false);
        SetDefault(VRCOSCSetting.OVRAutoClose, false);
        SetDefault(VRCOSCSetting.UseLegacyPorts, false);
        SetDefault(VRCOSCSetting.GlobalPersistence, false);
    }
}

public enum VRCOSCSetting
{
    FirstTimeSetupComplete,
    StartInTray,
    PackageFilter,
    AutomaticProfileSwitching,
    ModuleLogDebug,
    VRCAutoStart,
    VRCAutoStop,
    OVRAutoClose,
    AllowPreReleasePackages,
    UseLegacyPorts,
    TrayOnClose,
    GlobalPersistence
}
