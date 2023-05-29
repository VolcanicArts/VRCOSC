// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Lists;
using osu.Framework.Platform;
using osu.Framework.Threading;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Serialisation.Legacy;
using VRCOSC.Game.Modules.Sources;
using VRCOSC.Game.OSC.VRChat;
using VRCOSC.Game.Serialisation;
using Module = VRCOSC.Game.Modules.Module;

namespace VRCOSC.Game.Managers;

public sealed class ModuleManager : IEnumerable<Module>
{
    private static TerminalLogger terminal => new("ModuleManager");

    private readonly List<IModuleSource> sources = new();
    private readonly SortedList<Module> modules = new();

    public Action? OnModuleEnabledChanged;

    public void AddSource(IModuleSource source) => sources.Add(source);
    public bool RemoveSource(IModuleSource source) => sources.Remove(source);

    private GameHost host = null!;
    private GameManager gameManager = null!;
    private IVRCOSCSecrets secrets = null!;
    private Scheduler scheduler = null!;
    private SerialisationManager serialisationManager = null!;

    private readonly List<Module> runningModulesCache = new();

    public void InjectModuleDependencies(GameHost host, GameManager gameManager, IVRCOSCSecrets secrets, Scheduler scheduler)
    {
        this.host = host;
        this.gameManager = gameManager;
        this.secrets = secrets;
        this.scheduler = scheduler;
    }

    public Dictionary<Assembly, List<Module>> GroupedModules()
    {
        var dict = new Dictionary<string, (Assembly, List<Module>)>();

        modules.ForEach(module =>
        {
            var assemblyKey = module.ContainingAssembly.GetName().FullName.ToLowerInvariant();

            if (!dict.ContainsKey(assemblyKey))
            {
                dict.Add(assemblyKey, (module.ContainingAssembly, new List<Module>()));
            }

            dict[assemblyKey].Item2.Add(module);
        });

        var finalDict = new Dictionary<Assembly, List<Module>>();

        dict.ForEach(pair =>
        {
            finalDict.Add(pair.Value.Item1, pair.Value.Item2);
        });

        return finalDict;
    }

    public void Load(Storage storage, NotificationContainer notification)
    {
        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(1, new LegacyModuleManagerSerialiser(storage, notification, this));

        modules.Clear();

        sources.ForEach(source =>
        {
            foreach (var type in source.Load())
            {
                var module = (Module)Activator.CreateInstance(type)!;
                module.InjectDependencies(host, gameManager, secrets, scheduler, storage, notification);
                module.Load();
                modules.Add(module);
            }
        });

        foreach (var module in this)
        {
            module.Enabled.BindValueChanged(_ =>
            {
                OnModuleEnabledChanged?.Invoke();
                module.Serialise();
            });
        }

        if (storage.Exists("modules.json"))
        {
            serialisationManager.Deserialise();
            storage.Delete("modules.json");
        }

        // TODO - Move into module.Load() when migration is complete
        foreach (var module in this)
        {
            module.Deserialise();
        }
    }

    public void Start()
    {
        if (modules.All(module => !module.Enabled.Value))
            terminal.Log("You have no modules selected!\nSelect some modules to begin using VRCOSC");

        runningModulesCache.Clear();

        foreach (var module in modules.Where(module => module.Enabled.Value))
        {
            module.Start();
            runningModulesCache.Add(module);
        }
    }

    public void Update()
    {
        scheduler.Update();
    }

    public void Stop()
    {
        scheduler.CancelDelayedTasks();

        foreach (var module in runningModulesCache)
        {
            module.Stop();
        }
    }

    public void ParamaterReceived(VRChatOscData data)
    {
        foreach (var module in runningModulesCache)
        {
            module.OnParameterReceived(data);
        }
    }

    public void PlayerUpdate()
    {
        foreach (var module in runningModulesCache)
        {
            module.PlayerUpdate();
        }
    }

    public IEnumerable<string> GetEnabledModuleNames() => modules.Where(module => module.Enabled.Value).Select(module => module.SerialisedName);

    public string GetModuleName(string serialisedName) => modules.Single(module => module.SerialisedName == serialisedName).Title;

    public IEnumerator<Module> GetEnumerator() => modules.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
