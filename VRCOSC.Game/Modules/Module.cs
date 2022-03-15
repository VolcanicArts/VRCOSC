using System;
using System.Collections.Generic;
using osu.Framework.Logging;

namespace VRCOSC.Game.Modules;

public abstract class Module
{
    protected Logger Terminal = Logger.GetLogger("terminal");
    public virtual string Title => "Unknown";
    public virtual string Description => "Unknown description";
    public Dictionary<string, KeyValuePair<Type, object>> Settings { get; } = new();
    public Dictionary<string, ModuleSettingMetadata> SettingsMetadata { get; } = new();
    public virtual ModuleParametersManager ParametersManager => new();

    public abstract void Start();
    public abstract void Stop();

    protected void CreateSetting(string key, string displayName, string description, object defaultValue)
    {
        Settings.Add(key, new KeyValuePair<Type, object>(defaultValue.GetType(), defaultValue));
        SettingsMetadata.Add(key, new ModuleSettingMetadata
        {
            DisplayName = displayName,
            Description = description
        });
    }

    public void UpdateSetting(string key, object value)
    {
        Settings[key] = new KeyValuePair<Type, object>(value.GetType(), value);
    }
}
