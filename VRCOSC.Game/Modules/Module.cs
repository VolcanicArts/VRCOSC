using System.Collections.Generic;
using osu.Framework.Logging;

namespace VRCOSC.Game.Modules;

public abstract class Module
{
    protected Logger Terminal = Logger.GetLogger("terminal");
    public virtual string Title => "Unknown";
    public virtual string Description => "Unknown description";
    public Dictionary<string, ModuleSetting> Settings { get; } = new();
    public Dictionary<string, ModuleOscParameter> Parameters { get; } = new();

    public abstract void Start();
    public abstract void Stop();

    protected void CreateSetting(string key, string displayName, string description, object defaultValue)
    {
        var moduleSetting = new ModuleSetting
        {
            Key = key,
            DisplayName = displayName,
            Description = description,
            Value = defaultValue,
            Type = defaultValue.GetType()
        };
        Settings.Add(key, moduleSetting);
    }

    protected void CreateParameter(string key, string displayName, string description, string defaultAddress)
    {
        var moduleOscParameter = new ModuleOscParameter
        {
            Key = key,
            DisplayName = displayName,
            Description = description,
            Address = defaultAddress
        };
        Parameters.Add(key, moduleOscParameter);
    }

    protected T GetSettingValue<T>(string key)
    {
        return (T)Settings[key].Value;
    }

    protected string GetParameterAddress(string key)
    {
        return Parameters[key].Address;
    }
}
