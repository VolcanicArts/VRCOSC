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
            new HypeRateModule()
        };
    }

    public void Start()
    {
        Running.Value = true;
        Modules.ForEach(module =>
        {
            if (module.Enabled.Value) module.Start();
        });
    }

    public void Update()
    {
        if (Running.Value)
            Modules.ForEach(module => module.Update());
    }

    public void Stop()
    {
        Running.Value = false;
        Modules.ForEach(module =>
        {
            if (module.Enabled.Value) module.Stop();
        });
    }
}
