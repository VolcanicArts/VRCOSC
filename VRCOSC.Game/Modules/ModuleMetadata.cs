using System.Collections.Generic;

namespace VRCOSC.Game.Modules;

public class ModuleMetadata
{
    public Dictionary<string, ModuleSettingMetadata> Settings { get; } = new();
    public Dictionary<string, ModuleOscParameterMetadata> Parameters { get; } = new();
}
