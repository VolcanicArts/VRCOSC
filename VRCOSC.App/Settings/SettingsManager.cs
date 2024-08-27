// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using VRCOSC.App.Packages;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Settings.Serialisation;
using VRCOSC.App.UI.Themes;
using VRCOSC.App.Updater;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Settings;

public class SettingsManager
{
    private static SettingsManager? instance;
    public static SettingsManager GetInstance() => instance ??= new SettingsManager();

    public readonly Dictionary<VRCOSCSetting, IObservable> Settings = new();

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

        Settings.ForEach(pair => pair.Value.Subscribe(() => serialisationManager.Serialise()));

        serialisationManager.Deserialise();
    }

    private void setDefault<T>(VRCOSCSetting lookup, T? defaultValue)
    {
        Settings[lookup] = (IObservable)Activator.CreateInstance(typeof(Observable<T>), defaultValue)!;
    }

    private void writeDefaults()
    {
        setDefault(VRCOSCSetting.FirstTimeSetupComplete, false);
        setDefault(VRCOSCSetting.StartInTray, false);
        setDefault(VRCOSCSetting.PackageFilter, (int)(PackageListingFilter.Type_Official | PackageListingFilter.Type_Curated | PackageListingFilter.Type_Community)); // TODO: Remove community on release
        setDefault(VRCOSCSetting.AutomaticProfileSwitching, false);
        setDefault(VRCOSCSetting.VRCAutoStart, false);
        setDefault(VRCOSCSetting.VRCAutoStop, false);
        setDefault(VRCOSCSetting.OVRAutoOpen, false);
        setDefault(VRCOSCSetting.OVRAutoClose, false);
        setDefault(VRCOSCSetting.AllowPreReleasePackages, true); // TODO: Change on app release
        setDefault(VRCOSCSetting.TrayOnClose, false);
        setDefault(VRCOSCSetting.UpdateChannel, UpdateChannel.Beta); // TODO: Change on app release
        setDefault(VRCOSCSetting.ChatBoxSendInterval, 1500);
        setDefault(VRCOSCSetting.ChatBoxWorldBlacklist, true);
        setDefault(VRCOSCSetting.ShowRelevantModules, true);
        setDefault(VRCOSCSetting.Theme, Theme.Dark);
        setDefault(VRCOSCSetting.OutgoingEndpoint, "127.0.0.1:9000");
        setDefault(VRCOSCSetting.IncomingEndpoint, "127.0.0.1:9001");
        setDefault(VRCOSCSetting.UseCustomEndpoints, false);
        setDefault(VRCOSCSetting.EnableAppDebug, false);
        setDefault(VRCOSCSetting.AutoSwitchMicrophone, true);
        setDefault(VRCOSCSetting.SelectedInputDeviceID, string.Empty);
        setDefault(VRCOSCSetting.SelectedSpeechEngine, SpeechEngine.Whisper);
        setDefault(VRCOSCSetting.SpeechConfidence, 0.4f);
        setDefault(VRCOSCSetting.Whisper_ModelPath, string.Empty);
    }

    public Observable<T> GetObservable<T>(VRCOSCSetting lookup)
    {
        if (!Settings.TryGetValue(lookup, out var observable)) throw new InvalidOperationException("Setting doesn't exist");
        if (observable is not Observable<T> castObservable) throw new InvalidOperationException($"Setting is not of type {typeof(T).ToReadableName()}");

        return castObservable;
    }

    public IObservable GetObservable(VRCOSCSetting lookup)
    {
        if (!Settings.TryGetValue(lookup, out var observable)) throw new InvalidOperationException("Setting doesn't exist");

        return observable;
    }

    public T GetValue<T>(VRCOSCSetting lookup) => GetObservable<T>(lookup).Value!;
}

public enum VRCOSCSetting
{
    FirstTimeSetupComplete,
    StartInTray,
    PackageFilter,
    AutomaticProfileSwitching,
    VRCAutoStart,
    VRCAutoStop,
    OVRAutoOpen,
    OVRAutoClose,
    AllowPreReleasePackages,
    TrayOnClose,
    UpdateChannel,
    ChatBoxSendInterval,
    ChatBoxWorldBlacklist,
    ShowRelevantModules,
    Theme,
    OutgoingEndpoint,
    IncomingEndpoint,
    UseCustomEndpoints,
    EnableAppDebug,
    AutoSwitchMicrophone,
    SelectedInputDeviceID,
    SelectedSpeechEngine,
    Whisper_ModelPath,
    SpeechConfidence
}

public enum SpeechEngine
{
    Whisper
}
