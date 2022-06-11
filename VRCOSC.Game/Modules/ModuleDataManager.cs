// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Platform;

namespace VRCOSC.Game.Modules;

public class ModuleDataManager
{
    public bool Enabled { get; internal set; }
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

    public T GetSettingAs<T>(Enum key)
    {
        return GetSettingAs<T>(key.ToString().ToLower());
    }

    public T GetSettingAs<T>(string key)
    {
        var setting = Settings[key];

        return setting switch
        {
            StringModuleSetting stringModuleSetting => (T)Convert.ChangeType(stringModuleSetting.Value, typeof(T)),
            IntModuleSetting intModuleSetting => (T)Convert.ChangeType(intModuleSetting.Value, typeof(T)),
            BoolModuleSetting boolModuleSetting => (T)Convert.ChangeType(boolModuleSetting.Value, typeof(T)),
            EnumModuleSetting enumModuleSetting => (T)Convert.ChangeType(enumModuleSetting.Value, typeof(T)),
            _ => throw new ArgumentException($"No setting found with key {key} that is of type {typeof(T)}")
        };
    }

    public void CreateSetting(Enum key, string displayName, string description, object defaultValue)
    {
        switch (defaultValue)
        {
            case string stringValue:
                createSetting(key, displayName, description, stringValue);
                break;

            case bool boolValue:
                createSetting(key, displayName, description, boolValue);
                break;

            case int intValue:
                createSetting(key, displayName, description, intValue);
                break;

            case Enum enumValue:
                createSetting(key, displayName, description, enumValue);
                break;

            case long longValue:
                createSetting(key, displayName, description, (int)longValue);
                break;

            case float floatValue:
                createSetting(key, displayName, description, (int)floatValue);
                break;

            case double doubleValue:
                createSetting(key, displayName, description, (int)doubleValue);
                break;

            default:
                throw new InvalidCastException($"Settings must be of types string, int, bool, or enum. They cannot be of type {defaultValue.GetType()}");
        }
    }

    private void createSetting(Enum key, string displayName, string description, string defaultValue)
    {
        var setting = new StringModuleSetting
        {
            DisplayName = displayName,
            Description = description,
            Value = defaultValue
        };
        setSetting(key, setting);
    }

    private void createSetting(Enum key, string displayName, string description, int defaultValue)
    {
        var setting = new IntModuleSetting
        {
            DisplayName = displayName,
            Description = description,
            Value = defaultValue
        };
        setSetting(key, setting);
    }

    private void createSetting(Enum key, string displayName, string description, bool defaultValue)
    {
        var setting = new BoolModuleSetting
        {
            DisplayName = displayName,
            Description = description,
            Value = defaultValue
        };
        setSetting(key, setting);
    }

    private void createSetting(Enum key, string displayName, string description, Enum defaultValue)
    {
        var setting = new EnumModuleSetting
        {
            DisplayName = displayName,
            Description = description,
            Value = defaultValue
        };
        setSetting(key, setting);
    }

    private void setSetting(Enum key, ModuleSetting setting)
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

    public void CreateParameter(Enum key, string displayName, string description, string defaultAddress)
    {
        var parameter = new ModuleParameter
        {
            DisplayName = displayName,
            Description = description,
            Value = defaultAddress
        };
        setParameter(key, parameter);
    }

    private void setParameter(Enum key, ModuleParameter parameter)
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
