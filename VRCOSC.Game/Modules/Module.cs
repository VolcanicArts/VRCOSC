using System.Collections.Generic;
using osu.Framework.Logging;

namespace VRCOSC.Game.Modules;

public abstract class Module
{
    protected Logger Terminal = Logger.GetLogger("terminal");
    public virtual string Title => "Unknown";
    public virtual string Description => "Unknown description";
    public Dictionary<string, ModuleSetting> Settings { get; } = new();
    public virtual ModuleParametersManager ParametersManager => new();

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
}
