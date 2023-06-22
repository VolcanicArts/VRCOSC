// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Threading;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Modules;
using VRCOSC.Game.OSC.VRChat;
using Module = VRCOSC.Game.Modules.Module;

namespace VRCOSC.Game.Managers;

public sealed class ModuleManager : IEnumerable<ModuleCollection>
{
    private static TerminalLogger terminal => new("VRCOSC");

    public readonly Dictionary<string, ModuleCollection> ModuleCollections = new();

    public IReadOnlyList<Module> Modules => ModuleCollections.Values.SelectMany(collection => collection.Modules).ToList();

    public Action? OnModuleEnabledChanged;

    private GameHost host = null!;
    private GameManager gameManager = null!;
    private Scheduler scheduler = null!;
    private Storage storage = null!;
    private NotificationContainer notification = null!;

    private readonly List<Module> runningModulesCache = new();

    public void InjectModuleDependencies(GameHost host, GameManager gameManager, Scheduler scheduler, Storage storage, NotificationContainer notification)
    {
        this.host = host;
        this.gameManager = gameManager;
        this.scheduler = scheduler;
        this.storage = storage;
        this.notification = notification;
    }

    public void Load()
    {
        loadModules();
    }

    private void loadModules()
    {
        ModuleCollections.Clear();

        loadInternalModules();
        loadExternalModules();

        foreach (var module in Modules)
        {
            module.Enabled.BindValueChanged(_ =>
            {
                OnModuleEnabledChanged?.Invoke();
                module.Serialise();
            });
        }
    }

    private void loadInternalModules()
    {
        var dllPath = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.AllDirectories).FirstOrDefault(fileName => fileName.Contains("VRCOSC.Modules"))!;
        loadModulesFromAssembly(Assembly.Load(File.ReadAllBytes(dllPath)));
    }

    private void loadExternalModules()
    {
        try
        {
            var moduleDirectoryPath = storage.GetStorageForDirectory("assemblies").GetFullPath(string.Empty, true);
            Directory.GetFiles(moduleDirectoryPath, "*.dll", SearchOption.AllDirectories).ForEach(dllPath => loadModulesFromAssembly(Assembly.Load(File.ReadAllBytes(dllPath))));
        }
        catch (Exception e)
        {
            notification.Notify(new ExceptionNotification("Error when loading external modules"));
            Logger.Error(e, "ModuleManager experienced an exception");
        }
    }

    private void loadModulesFromAssembly(Assembly assembly)
    {
        try
        {
            assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Module)) && !type.IsAbstract).ForEach(type => registerModule(assembly, type));
        }
        catch (Exception e)
        {
            notification.Notify(new ExceptionNotification($"{assembly.GetAssemblyAttribute<AssemblyProductAttribute>()?.Product} could not be loaded. It may require an update"));
            Logger.Error(e, "ModuleManager experienced an exception");
        }
    }

    private void registerModule(Assembly assembly, Type type)
    {
        try
        {
            var module = (Module)Activator.CreateInstance(type)!;
            module.InjectDependencies(host, gameManager, scheduler, storage, notification);
            module.Load();

            var assemblyLookup = assembly.GetName().Name!.ToLowerInvariant();

            ModuleCollections.TryAdd(assemblyLookup, new ModuleCollection(assembly));
            ModuleCollections[assemblyLookup].Modules.Add(module);
        }
        catch (Exception e)
        {
            notification.Notify(new ExceptionNotification($"{type.Name} could not be loaded. It may require an update"));
            Logger.Error(e, "ModuleManager experienced an exception");
        }
    }

    public void Start()
    {
        if (Modules.All(module => !module.Enabled.Value))
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
