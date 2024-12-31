// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
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
    public readonly Dictionary<VRCOSCMetadata, IObservable> Metadata = new();

    private readonly Storage storage = AppManager.GetInstance().Storage;
    private readonly SerialisationManager serialisationManager;

    private SettingsManager()
    {
        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(1, new SettingsManagerSerialiser(storage, this));
    }

    public void Serialise() => serialisationManager.Serialise();

    public void Load()
    {
        writeDefaults();

        Settings.ForEach(pair => pair.Value.Subscribe(() => serialisationManager.Serialise()));
        Metadata.ForEach(pair => pair.Value.Subscribe(() => serialisationManager.Serialise()));

        serialisationManager.Deserialise();
    }

    private void setDefault<T>(VRCOSCSetting lookup, T defaultValue) where T : notnull
    {
        Settings[lookup] = (IObservable)Activator.CreateInstance(typeof(Observable<T>), defaultValue)!;
    }

    private void setDefault<T>(VRCOSCMetadata lookup, T defaultValue) where T : notnull
    {
        Metadata[lookup] = (IObservable)Activator.CreateInstance(typeof(Observable<T>), defaultValue)!;
    }

    private void writeDefaults()
    {
        setDefault(VRCOSCSetting.StartInTray, false);
        setDefault(VRCOSCSetting.AutomaticProfileSwitching, false);
        setDefault(VRCOSCSetting.VRCAutoStart, false);
        setDefault(VRCOSCSetting.VRCAutoStop, false);
        setDefault(VRCOSCSetting.OVRAutoOpen, false);
        setDefault(VRCOSCSetting.OVRAutoClose, false);
        setDefault(VRCOSCSetting.AllowPreReleasePackages, false);
        setDefault(VRCOSCSetting.AutoUpdatePackages, true);
        setDefault(VRCOSCSetting.TrayOnClose, false);
        setDefault(VRCOSCSetting.UpdateChannel, UpdateChannel.Live);
        setDefault(VRCOSCSetting.ChatBoxSendInterval, 1500);
        setDefault(VRCOSCSetting.ChatBoxWorldBlacklist, true);
        setDefault(VRCOSCSetting.FilterByEnabledModules, true);
        setDefault(VRCOSCSetting.Theme, Theme.Dark);
        setDefault(VRCOSCSetting.OutgoingEndpoint, "127.0.0.1:9000");
        setDefault(VRCOSCSetting.IncomingEndpoint, "127.0.0.1:9001");
        setDefault(VRCOSCSetting.UseCustomEndpoints, false);
        setDefault(VRCOSCSetting.EnableAppDebug, false);
        setDefault(VRCOSCSetting.EnableRouter, false);
        setDefault(VRCOSCSetting.SelectedMicrophoneID, string.Empty);
        setDefault(VRCOSCSetting.SpeechModelPath, string.Empty);
        setDefault(VRCOSCSetting.SpeechConfidence, 0.4f);
        setDefault(VRCOSCSetting.SpeechNoiseCutoff, 0.14f);
        setDefault(VRCOSCSetting.SpeechMicVolumeAdjustment, 1f);
        setDefault(VRCOSCSetting.SpeechTranslate, false);

        setDefault(VRCOSCMetadata.InstalledVersion, string.Empty);
        setDefault(VRCOSCMetadata.InstalledUpdateChannel, UpdateChannel.Live);
        setDefault(VRCOSCMetadata.FirstTimeSetupComplete, false);
        setDefault(VRCOSCMetadata.AutoStartQuestionClicked, false);
    }

    public Observable<T> GetObservable<T>(VRCOSCSetting lookup) where T : notnull
    {
        if (!Settings.TryGetValue(lookup, out var observable)) throw new InvalidOperationException("Setting doesn't exist");
        if (observable is not Observable<T> castObservable) throw new InvalidOperationException($"Setting is not of type {typeof(T).ToReadableName()}");

        return castObservable;
    }

    public Observable<T> GetObservable<T>(VRCOSCMetadata lookup) where T : notnull
    {
        if (!Metadata.TryGetValue(lookup, out var observable)) throw new InvalidOperationException("Metadata doesn't exist");
        if (observable is not Observable<T> castObservable) throw new InvalidOperationException($"Metadata is not of type {typeof(T).ToReadableName()}");

        return castObservable;
    }

    public IObservable GetObservable(VRCOSCSetting lookup)
    {
        if (!Settings.TryGetValue(lookup, out var observable)) throw new InvalidOperationException("Setting doesn't exist");

        return observable;
    }

    public IObservable GetObservable(VRCOSCMetadata lookup)
    {
        if (!Metadata.TryGetValue(lookup, out var observable)) throw new InvalidOperationException("Metadata doesn't exist");

        return observable;
    }

    public T GetValue<T>(VRCOSCSetting lookup) => GetObservable<T>(lookup).Value!;
    public T GetValue<T>(VRCOSCMetadata lookup) => GetObservable<T>(lookup).Value!;
}

public enum VRCOSCSetting
{
    StartInTray,
    AutomaticProfileSwitching,
    VRCAutoStart,
    VRCAutoStop,
    OVRAutoOpen,
    OVRAutoClose,
    AllowPreReleasePackages,
    AutoUpdatePackages,
    TrayOnClose,
    UpdateChannel,
    ChatBoxSendInterval,
    ChatBoxWorldBlacklist,
    FilterByEnabledModules,
    Theme,
    OutgoingEndpoint,
    IncomingEndpoint,
    UseCustomEndpoints,
    EnableAppDebug,
    EnableRouter,
    SelectedMicrophoneID,
    SelectedSpeechEngine,
    SpeechModelPath,
    SpeechConfidence,
    SpeechNoiseCutoff,
    SpeechMicVolumeAdjustment,
    SpeechTranslate
}

public enum VRCOSCMetadata
{
    InstalledVersion,
    FirstTimeSetupComplete,
    InstalledUpdateChannel,
    AutoStartQuestionClicked
}