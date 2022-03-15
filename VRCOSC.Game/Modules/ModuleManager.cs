using Markdig.Helpers;
using osu.Framework.Bindables;
using VRCOSC.Game.Modules.Modules;

namespace VRCOSC.Game.Modules;

public class ModuleManager
{
    public OrderedList<Module> Modules { get; }

    public readonly BindableBool Running = new();

    public ModuleManager()
    {
        Modules = new OrderedList<Module>
        {
            new TestModule(),
            new TestModule(),
            new TestModule()
        };
    }

    public void Start()
    {
        Modules.ForEach(module => module.Start());
    }

    public void Stop()
    {
        Modules.ForEach(module => module.Stop());
    }
}
