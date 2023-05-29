// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

public sealed class ModuleManager : IEnumerable<ModuleCollection>
{
    private static TerminalLogger terminal => new("VRCOSC");

    private readonly List<IModuleSource> sources = new();
    public readonly Dictionary<string, ModuleCollection> ModuleCollections = new();

    // TODO - Can be made private when migration is complete
    public IReadOnlyList<Module> Modules => ModuleCollections.Values.SelectMany(collection => collection.Modules).ToList();

    public Action? OnModuleEnabledChanged;
    public Action? OnImportFinished;

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

    public void Load(Storage storage, NotificationContainer notification)
    {
        sources.Add(new InternalModuleSource());
        sources.Add(new ExternalModuleSource(storage));

        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(1, new LegacyModuleManagerSerialiser(storage, notification, this));

        ModuleCollections.Clear();

        sources.ForEach(source =>
        {
            foreach (var type in source.Load())
            {
                var module = (Module)Activator.CreateInstance(type)!;
                module.InjectDependencies(host, gameManager, secrets, scheduler, storage, notification);
                module.Load();

                var assemblyLookup = module.ContainingAssembly.GetName().FullName.ToLowerInvariant();

                ModuleCollections.TryAdd(assemblyLookup, new ModuleCollection(module.ContainingAssembly));
                ModuleCollections[assemblyLookup].Modules.Add(module);
            }
        });

        foreach (var module in Modules)
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
        foreach (var module in Modules)
        {
            module.Deserialise();
        }
    }

    public void Start()
    {
        if (ModuleCollections.All(pair => pair.Value.All(module => !module.Enabled.Value)))
            terminal.Log("You have no modules selected!\nSelect some modules to begin using VRCOSC");

        runningModulesCache.Clear();

        foreach (var module in Modules.Where(module => module.Enabled.Value))
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

    public Module? GetModule(string serialisedName) => Modules.SingleOrDefault(module => module.SerialisedName == serialisedName);

    public IEnumerable<string> GetEnabledModuleNames() => Modules.Where(module => module.Enabled.Value).Select(module => module.SerialisedName);

    public string GetModuleName(string serialisedName) => Modules.Single(module => module.SerialisedName == serialisedName).Title;

    public IEnumerator<ModuleCollection> GetEnumerator() => ModuleCollections.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
