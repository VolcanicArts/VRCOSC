using System.Collections.Generic;

namespace VRCOSC.Game.Modules;

public class ModuleData
{
    public Dictionary<string, object> Settings { get; } = new();
    public Dictionary<string, string> Parameters { get; } = new();
}
