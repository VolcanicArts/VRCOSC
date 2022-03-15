using System.Collections.Generic;
using Markdig.Helpers;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Platform;
using VRCOSC.Game.Modules.Modules;

namespace VRCOSC.Game.Modules;

public class ModuleManager
{
    public Dictionary<ModuleType, OrderedList<Module>> Modules { get; } = new();

    public readonly BindableBool Running = new();

    public ModuleManager(Storage storage)
    {
        addModule(new TestModule(storage));
        addModule(new HypeRateModule(storage));
    }

    private void addModule(Module module)
    {
        var list = Modules.GetValueOrDefault(module.Type, new OrderedList<Module>());
        list.Add(module);
        Modules.TryAdd(module.Type, list);
    }

    public void Start()
    {
        Running.Value = true;
        Modules.Values.ForEach(modules =>
        {
            modules.ForEach(module =>
            {
                if (module.Data.Enabled) module.Start();
            });
        });
    }

    public void Update()
    {
        if (Running.Value)
        {
            Modules.Values.ForEach(modules =>
            {
                modules.ForEach(module =>
                {
                    if (module.Data.Enabled) module.Update();
                });
            });
        }
    }

    public void Stop()
    {
        Running.Value = false;
        Modules.Values.ForEach(modules =>
        {
            modules.ForEach(module =>
            {
                if (module.Data.Enabled) module.Stop();
            });
        });
    }
}
