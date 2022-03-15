using System.Collections.Generic;

namespace VRCOSC.Game.Modules;

public class ModuleMetadata
{
    public Dictionary<string, ModuleAttributeMetadata> Settings { get; } = new();
    public Dictionary<string, ModuleAttributeMetadata> Parameters { get; } = new();
}

public struct ModuleAttributeMetadata
{
    public string DisplayName;
    public string Description;
}
