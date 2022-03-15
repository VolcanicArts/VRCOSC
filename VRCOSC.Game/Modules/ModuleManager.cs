using Markdig.Helpers;

namespace VRCOSC.Game.Modules;

public class ModuleManager
{
    public OrderedList<Module> Modules { get; }

    private bool running = false;

    public ModuleManager()
    {
        Modules = new OrderedList<Module>
        {
            new TestModule()
        };
    }

    public void Start()
    {
        if (!running)
            Modules.ForEach(module => module.Start());
        running = true;
    }

    public void Stop()
    {
        if (running)
            Modules.ForEach(module => module.Stop());
        running = false;
    }
}
