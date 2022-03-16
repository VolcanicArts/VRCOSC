// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Modules;

public class ModuleDataManager
{
    [JsonIgnore]
    private const string storage_directory = "modules";

    [JsonProperty("enabled")]
    public readonly Bindable<bool> Enabled = new(true);

    [JsonProperty]
    private SerialisableBindableDictionary<string, object> settings { get; } = new();

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
        Logger.Log("Binding attributes");
        settings.CollectionChanged += (_, _) => saveData();
        parameters.CollectionChanged += (_, _) => saveData();
        Enabled.ValueChanged += _ => saveData();
    }

    public void LoadData()
    {
        Logger.Log("Loading data");
        var fileName = $"{ModuleName}.conf";
        var moduleStorage = Storage.GetStorageForDirectory(storage_directory);

        using (var fileStream = moduleStorage.GetStream(fileName, FileAccess.ReadWrite))
        {
            using (var streamReader = new StreamReader(fileStream))
            {
                var deserializedData = JsonConvert.DeserializeObject<ModuleDataManager>(streamReader.ReadToEnd(), new JsonSerializerSettings
                {

                });
                if (deserializedData != null) copyDataFrom(deserializedData);
            }
        }

        saveData();
        bindAllAttributes();
    }

    private void saveData()
    {
        Logger.Log("Saving data");
        var fileName = $"{ModuleName}.conf";
        var moduleStorage = Storage.GetStorageForDirectory(storage_directory);

        moduleStorage.Delete(fileName);

        using var fileStream = moduleStorage.GetStream(fileName, FileAccess.ReadWrite);
        using var streamWriter = new StreamWriter(fileStream);

        var serialisedData = JsonConvert.SerializeObject(this);
        streamWriter.WriteLine(serialisedData);
    }

    public List<string> GetSettingKeys()
    {
        return settings.Keys.ToList();
    }

    public List<string> GetParameterKeys()
    {
        return parameters.Keys.ToList();
    }

    public void SetSetting(Enum key, object value)
    {
        var keyStr = key.ToString().ToLower();
        SetSetting(keyStr, value);
    }

    public void SetSetting(string key, object value)
    {
        if (!settings.TryAdd(key, value))
            settings[key] = value;
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

    public object? GetSetting(Enum key)
    {
        var keyStr = key.ToString().ToLower();
        return GetSetting(keyStr);
    }

    public object? GetSetting(string key)
    {
        return !settings.ContainsKey(key) ? null : settings[key];
    }

    public T GetSettingAs<T>(Enum key)
    {
        var keyStr = key.ToString().ToLower();
        return GetSettingAs<T>(keyStr)!;
    }

    public T? GetSettingAs<T>(string key)
    {
        return !settings.ContainsKey(key) ? default : (T)settings[key];
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
        Logger.Log("Copying data");
        dataToCopy.settings.ForEach(pair =>
        {
            if (settings.ContainsKey(pair.Key))
                settings[pair.Key] = pair.Value;
        });
        dataToCopy.parameters.ForEach(pair =>
        {
            if (parameters.ContainsKey(pair.Key))
                parameters[pair.Key] = pair.Value;
        });
        Enabled.Value = dataToCopy.Enabled.Value;
    }
}
