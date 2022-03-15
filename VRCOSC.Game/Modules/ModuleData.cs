using System.Collections.Generic;

namespace VRCOSC.Game.Modules;

public class ModuleData
{
    public Dictionary<string, ModuleSettingData> Settings { get; } = new();
    public Dictionary<string, ModuleOscParameterData> Parameters { get; } = new();
}
