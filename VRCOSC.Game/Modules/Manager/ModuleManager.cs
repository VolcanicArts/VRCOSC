// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Lists;
using osu.Framework.Platform;
using osu.Framework.Threading;
using VRCOSC.Game.Modules.Serialisation;
using VRCOSC.Game.Modules.Sources;

namespace VRCOSC.Game.Modules.Manager;

public sealed class ModuleManager : IModuleManager
{
    private static TerminalLogger terminal => new("ModuleManager");

    private readonly List<IModuleSource> sources = new();
    private readonly SortedList<Module> modules = new();
    private IModuleSerialiser? serialiser;

    public void AddSource(IModuleSource source) => sources.Add(source);
    public bool RemoveSource(IModuleSource source) => sources.Remove(source);
    public void SetSerialiser(IModuleSerialiser serialiser) => this.serialiser = serialiser;

    private GameHost host = null!;
    private GameManager gameManager = null!;
    private IVRCOSCSecrets secrets = null!;
    private Scheduler scheduler = null!;

    public void InjectModuleDependencies(GameHost host, GameManager gameManager, IVRCOSCSecrets secrets, Scheduler scheduler)
    {
        this.host = host;
        this.gameManager = gameManager;
        this.secrets = secrets;
        this.scheduler = scheduler;
    }

    public void Load()
    {
        modules.Clear();

        sources.ForEach(source =>
        {
            foreach (var type in source.Load())
            {
                var module = (Module)Activator.CreateInstance(type)!;
                module.InjectDependencies(host, gameManager, secrets, scheduler);
                modules.Add(module);
            }
        });

        foreach (var module in this)
        {
            module.Load();
            serialiser?.Deserialise(module);
        }
    }

    public void SaveAll()
    {
        foreach (var module in this)
        {
            serialiser?.Serialise(module);
        }
    }

    public void Save(Module module)
    {
        serialiser?.Serialise(module);
    }

    public void Start()
    {
        if (modules.All(module => !module.Enabled.Value))
            terminal.Log("You have no modules selected!\nSelect some modules to begin using VRCOSC");

        foreach (var module in modules)
        {
            module.Start();
        }
    }

    public void Update()
    {
        scheduler.Update();

        foreach (var module in modules)
        {
            module.internalUpdate();
        }
    }

    public void Stop()
    {
        scheduler.CancelDelayedTasks();

        foreach (var module in modules)
        {
            module.Stop();
        }
    }

    public IEnumerator<Module> GetEnumerator() => modules.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
