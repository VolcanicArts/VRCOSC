using Markdig.Helpers;

namespace VRCOSC.Game.Modules;

public class ModuleManager
{
    public OrderedList<Module> Modules { get; }

    public ModuleManager()
    {
        Modules = new OrderedList<Module>
        {
            new TestModule()
        };
    }

    public void Start()
    {
        Modules.ForEach(module => module.Start());
    }
}
