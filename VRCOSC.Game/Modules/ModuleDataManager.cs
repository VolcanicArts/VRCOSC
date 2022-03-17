// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Platform;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Modules;

public class ModuleDataManager
{
    [JsonIgnore]
    private const string storage_directory = "modules";

    [JsonProperty("enabled")]
    public readonly Bindable<bool> Enabled = new(true);

    [JsonProperty("settings")]
    public readonly ModuleSettingsManager Settings = new();

    [JsonProperty]
    private SerialisableBindableDictionary<string, string> parameters { get; } = new();

    [JsonIgnore]
    private readonly Storage Storage;

    [JsonIgnore]
    private readonly string ModuleName;

    public ModuleDataManager(Storage storage, string moduleName)
    {
        Storage = storage;
        ModuleName = moduleName;
    }

    private void bindAllAttributes()
    {
        Settings.StringSettings.CollectionChanged += (_, _) => saveData();
        Settings.IntSettings.CollectionChanged += (_, _) => saveData();
        Settings.BoolSettings.CollectionChanged += (_, _) => saveData();
        parameters.CollectionChanged += (_, _) => saveData();
        Enabled.ValueChanged += _ => saveData();
    }

    private void unbindAllAttributes()
    {
        Settings.StringSettings.UnbindAll();
        Settings.IntSettings.UnbindAll();
        Settings.BoolSettings.UnbindAll();
        parameters.UnbindAll();
        Enabled.UnbindAll();
    }

    public void LoadData()
    {
        unbindAllAttributes();

        var fileName = $"{ModuleName}.conf";
        var moduleStorage = Storage.GetStorageForDirectory(storage_directory);

        using (var fileStream = moduleStorage.GetStream(fileName, FileAccess.ReadWrite))
        {
            using (var streamReader = new StreamReader(fileStream))
            {
                var deserializedData = JsonConvert.DeserializeObject<ModuleDataManager>(streamReader.ReadToEnd());
                if (deserializedData != null) copyDataFrom(deserializedData);
            }
        }

        saveData();
        bindAllAttributes();
    }

    private void saveData()
    {
        var fileName = $"{ModuleName}.conf";
        var moduleStorage = Storage.GetStorageForDirectory(storage_directory);

        moduleStorage.Delete(fileName);

        using var fileStream = moduleStorage.GetStream(fileName, FileAccess.ReadWrite);
        using var streamWriter = new StreamWriter(fileStream);

        var serialisedData = JsonConvert.SerializeObject(this);
        streamWriter.WriteLine(serialisedData);
    }

    public List<string> GetParameterKeys()
    {
        return parameters.Keys.ToList();
    }

    public void SetParameter(Enum key, string address)
    {
        var keyStr = key.ToString().ToLower();
        SetParameter(keyStr, address);
    }

    public void SetParameter(string key, string address)
    {
        if (!parameters.TryAdd(key, address))
            parameters[key] = address;
    }

    public T GetSettingAs<T>(string key)
    {
        if (typeof(T) == typeof(string))
        {
            return (T)Convert.ChangeType(Settings.StringSettings[key], typeof(T));
        }

        if (typeof(T) == typeof(int))
        {
            return (T)Convert.ChangeType(Settings.IntSettings[key], typeof(T));
        }

        if (typeof(T) == typeof(bool))
        {
            return (T)Convert.ChangeType(Settings.BoolSettings[key], typeof(T));
        }

        throw new ArgumentException($"No setting found with key {key} that is of type {typeof(T)}");
    }

    public string GetParameter(Enum key)
    {
        var keyStr = key.ToString().ToLower();
        return GetParameter(keyStr);
    }

    public string GetParameter(string key)
    {
        return parameters[key];
    }

    private void copyDataFrom(ModuleDataManager dataToCopy)
    {
        Settings.CopyDataFrom(dataToCopy.Settings);
        dataToCopy.parameters.ForEach(pair =>
        {
            if (parameters.ContainsKey(pair.Key))
                parameters[pair.Key] = pair.Value;
        });
        Enabled.Value = dataToCopy.Enabled.Value;
    }
}
