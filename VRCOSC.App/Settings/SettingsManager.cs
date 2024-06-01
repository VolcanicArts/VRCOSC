// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using VRCOSC.App.Packages;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Settings.Serialisation;
using VRCOSC.App.Themes;
using VRCOSC.App.Updater;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Settings;

public class SettingsManager
{
    private static SettingsManager? instance;
    public static SettingsManager GetInstance() => instance ??= new SettingsManager();

    public readonly Dictionary<VRCOSCSetting, Observable<object>> Settings = new();

    private readonly Storage storage = AppManager.GetInstance().Storage;
    private readonly SerialisationManager serialisationManager;

    private SettingsManager()
    {
        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(1, new SettingsManagerSerialiser(storage, this));
    }

    public void Load()
    {
        writeDefaults();

        Settings.ForEach(pair => pair.Value.Subscribe(_ => serialisationManager.Serialise()));

        ExceptionHandler.SilenceWindow = true;
        serialisationManager.Deserialise();
        ExceptionHandler.SilenceWindow = false;
    }

    private void writeDefaults()
    {
        Settings[VRCOSCSetting.FirstTimeSetupComplete] = new Observable<object>(false);
        Settings[VRCOSCSetting.StartInTray] = new Observable<object>(false);
        Settings[VRCOSCSetting.PackageFilter] = new Observable<object>((int)(PackageListingFilter.Type_Official | PackageListingFilter.Type_Curated | PackageListingFilter.Type_Community)); // TODO: Remove community on release
        Settings[VRCOSCSetting.AutomaticProfileSwitching] = new Observable<object>(false);
        Settings[VRCOSCSetting.ModuleLogDebug] = new Observable<object>(false);
        Settings[VRCOSCSetting.VRCAutoStart] = new Observable<object>(false);
        Settings[VRCOSCSetting.VRCAutoStop] = new Observable<object>(false);
        Settings[VRCOSCSetting.OVRAutoOpen] = new Observable<object>(false);
        Settings[VRCOSCSetting.OVRAutoClose] = new Observable<object>(false);
        Settings[VRCOSCSetting.AllowPreReleasePackages] = new Observable<object>(true); // TODO: Change on app release
        Settings[VRCOSCSetting.TrayOnClose] = new Observable<object>(false);
        Settings[VRCOSCSetting.GlobalPersistence] = new Observable<object>(false);
        Settings[VRCOSCSetting.ReleaseChannel] = new Observable<object>(UpdaterReleaseChannel.Beta); // TODO: Change on app release
        Settings[VRCOSCSetting.ChatBoxSendInterval] = new Observable<object>(1500);
        Settings[VRCOSCSetting.ChatBoxWorldBlacklist] = new Observable<object>(true);
        Settings[VRCOSCSetting.ShowRelevantModules] = new Observable<object>(true);
        Settings[VRCOSCSetting.Theme] = new Observable<object>((int)Theme.Dark);
        Settings[VRCOSCSetting.OutgoingEndpoint] = new Observable<object>("127.0.0.1:9000");
        Settings[VRCOSCSetting.IncomingEndpoint] = new Observable<object>("127.0.0.1:9001");
        Settings[VRCOSCSetting.UseCustomEndpoints] = new Observable<object>(false);
    }

    public Observable<object> GetObservable(VRCOSCSetting lookup)
    {
        if (!Settings.TryGetValue(lookup, out var observable)) throw new InvalidOperationException("Setting doesn't exist");

        return observable;
    }

    public T GetValue<T>(VRCOSCSetting lookup)
    {
        var value = GetObservable(lookup).Value;
        if (value is not T valueType) throw new InvalidOperationException("Requested type doesn't match stored type");

        return valueType;
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
    OVRAutoOpen,
    OVRAutoClose,
    AllowPreReleasePackages,
    TrayOnClose,
    GlobalPersistence,
    ReleaseChannel,
    ChatBoxSendInterval,
    ChatBoxWorldBlacklist,
    ShowRelevantModules,
    Theme,
    OutgoingEndpoint,
    IncomingEndpoint,
    UseCustomEndpoints
}
