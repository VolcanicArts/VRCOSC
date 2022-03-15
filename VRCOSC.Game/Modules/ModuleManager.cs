using System.Collections.Generic;
using Markdig.Helpers;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Platform;
using osu.Framework.Threading;
using VRCOSC.Game.Modules.Modules;
using VRCOSC.Game.Modules.Modules.Clock;

namespace VRCOSC.Game.Modules;

public class ModuleManager

{
    public Dictionary<ModuleType, OrderedList<Module>> Modules { get; } = new();

    public readonly BindableBool Running = new();

    public ModuleManager(Storage storage)
    {
        addModule(new TestModule(storage));
        addModule(new HypeRateModule(storage));
        addModule(new ClockModule(storage));
    }

    private void addModule(Module module)
    {
        var list = Modules.GetValueOrDefault(module.Type, new OrderedList<Module>());
        list.Add(module);
        Modules.TryAdd(module.Type, list);
    }

    public void Start(Scheduler scheduler)
    {
        Running.Value = true;
        Modules.Values.ForEach(modules =>
        {
            modules.ForEach(module =>
            {
                if (!module.Data.Enabled) return;

                module.Start();
                if (!double.IsPositiveInfinity(module.DeltaUpdate)) scheduler.AddDelayed(module.Update, module.DeltaUpdate, true);
            });
        });
    }

    public void Stop(Scheduler scheduler)
    {
        Running.Value = false;
        scheduler.CancelDelayedTasks();
        Modules.Values.ForEach(modules =>
        {
            modules.ForEach(module =>
            {
                if (module.Data.Enabled) module.Stop();
            });
        });
    }
}
