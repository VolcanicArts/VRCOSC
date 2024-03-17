// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Settings.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Settings;

public class SettingsManager
{
    private static SettingsManager? instance;
    public static SettingsManager GetInstance() => instance ??= new SettingsManager();

    public readonly Dictionary<VRCOSCSetting, Observable<object>> Settings = new();

    private readonly Storage storage = new NativeStorage($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/VRCOSC-V2-WPF");
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

        serialisationManager.Deserialise();
    }

    private void writeDefaults()
    {
        Settings[VRCOSCSetting.AllowPreReleasePackages] = new Observable<object>(true); // TODO: Change on app release
        Settings[VRCOSCSetting.UseLegacyPorts] = new Observable<object>(false);
    }

    public Observable<object> GetObservable(VRCOSCSetting lookup)
    {
        if (!Settings.ContainsKey(lookup)) throw new InvalidOperationException("Setting doesn't exist");

        return Settings[lookup];
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
    AllowPreReleasePackages,
    UseLegacyPorts
}
