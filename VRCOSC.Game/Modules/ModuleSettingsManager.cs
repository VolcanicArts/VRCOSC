using Markdig.Helpers;

namespace VRCOSC.Game.Modules;

public class ModuleSettingsManager
{
    public OrderedList<ModuleSetting> Settings { get; }

    public ModuleSettingsManager()
    {
        Settings = new OrderedList<ModuleSetting>();
    }

    public ModuleSettingsManager(OrderedList<ModuleSetting> initialSettings)
    {
        Settings = initialSettings;
    }
}
