// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Platform;

namespace VRCOSC.Game.Modules;

public class ModuleDataManager
{
    public bool Enabled { get; internal set; } = true;
    public Dictionary<string, ModuleSetting> Settings { get; } = new();
    public Dictionary<string, ModuleParameter> Parameters { get; } = new();

    private Storage storage { get; }
    internal string ModuleName { get; }

    public ModuleDataManager(Storage storage, string moduleName)
    {
        this.storage = storage;
        ModuleName = moduleName;
    }

    public void LoadData()
    {
        var deserializedData = ModuleStorage.Load(storage, ModuleName);
        if (deserializedData != null) copyDataFrom(deserializedData);

        saveData();
    }

    private void saveData()
    {
        ModuleStorage.Save(storage, this);
    }

    public void SetEnabled(bool value)
    {
        Enabled = value;
        saveData();
    }

    #region Settings

    public T GetSettingAs<T>(string key)
    {
        if (typeof(T) == typeof(string))
        {
            var setting = (StringModuleSetting)Settings[key];
            return (T)Convert.ChangeType(setting.Value, typeof(T));
        }

        if (typeof(T) == typeof(int))
        {
            var setting = (IntModuleSetting)Settings[key];
            return (T)Convert.ChangeType(setting.Value, typeof(T));
        }

        if (typeof(T) == typeof(bool))
        {
            var setting = (BoolModuleSetting)Settings[key];
            return (T)Convert.ChangeType(setting.Value, typeof(T));
        }

        if (typeof(T).IsSubclassOf(typeof(Enum)))
        {
            var setting = (EnumModuleSetting)Settings[key];
            return (T)Convert.ChangeType(setting.Value, typeof(T));
        }

        throw new ArgumentException($"No setting found with key {key} that is of type {typeof(T)}");
    }

    public void SetSetting(Enum key, ModuleSetting setting)
    {
        var keyStr = key.ToString().ToLower();
        SetSetting(keyStr, setting);
    }

    public void SetSetting(string key, ModuleSetting setting)
    {
        if (Settings.TryAdd(key, setting)) return;

        // copying metadata
        setting.DisplayName = Settings[key].DisplayName;
        setting.Description = Settings[key].Description;

        Settings[key] = setting;
    }

    public void UpdateStringSetting(string key, string value)
    {
        var setting = (StringModuleSetting)Settings[key];
        setting.Value = value;
        saveData();
    }

    public void UpdateIntSetting(string key, int value)
    {
        var setting = (IntModuleSetting)Settings[key];
        setting.Value = value;
        saveData();
    }

    public void UpdateBoolSetting(string key, bool value)
    {
        var setting = (BoolModuleSetting)Settings[key];
        setting.Value = value;
        saveData();
    }

    public void UpdateEnumSetting<T>(string key, T value) where T : Enum
    {
        var setting = (EnumModuleSetting)Settings[key];
        setting.Value = value;
        saveData();
    }

    #endregion

    #region Parameters

    public void SetParameter(Enum key, ModuleParameter parameter)
    {
        var keyStr = key.ToString().ToLower();
        SetParameter(keyStr, parameter);
    }

    public void SetParameter(string key, ModuleParameter parameter)
    {
        if (Parameters.TryAdd(key, parameter)) return;

        // copying metadata
        parameter.DisplayName = Parameters[key].DisplayName;
        parameter.Description = Parameters[key].Description;

        Parameters[key] = parameter;
    }

    public void UpdateParameter(string key, string address)
    {
        Parameters[key].Value = address;
        saveData();
    }

    public string GetParameter(Enum key)
    {
        var keyStr = key.ToString().ToLower();
        return GetParameter(keyStr);
    }

    public string GetParameter(string key)
    {
        return Parameters[key].Value;
    }

    #endregion

    private void copyDataFrom(ModuleDataManager dataToCopy)
    {
        Enabled = dataToCopy.Enabled;

        dataToCopy.Settings.ForEach(pair =>
        {
            var (key, setting) = pair;

            if (Settings.ContainsKey(key))
                SetSetting(key, setting);
        });

        dataToCopy.Parameters.ForEach(pair =>
        {
            var (key, parameter) = pair;

            if (Parameters.ContainsKey(key))
                SetParameter(key, parameter);
        });
    }
}
