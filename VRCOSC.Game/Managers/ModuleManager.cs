// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Threading;
using VRCOSC.Game.App;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Avatar;
using VRCOSC.Game.OSC.VRChat;
using VRCOSC.Game.Util;
using Module = VRCOSC.Game.Modules.Module;

namespace VRCOSC.Game.Managers;

public sealed class ModuleManager : IEnumerable<ModuleCollection>
{
    private static TerminalLogger terminal => new("VRCOSC");

    private readonly List<AssemblyLoadContext> assemblyContexts = new();
    public readonly Dictionary<string, ModuleCollection> ModuleCollections = new();

    public IReadOnlyList<Module> Modules => ModuleCollections.Values.SelectMany(collection => collection.Modules).ToList();

    public Action? OnModuleEnabledChanged;

    private GameHost host = null!;
    private AppManager appManager = null!;
    private Scheduler scheduler = null!;
    private Storage storage = null!;

    private readonly List<Module> runningModulesCache = new();

    public void Initialise(GameHost host, AppManager appManager, Scheduler scheduler, Storage storage)
    {
        this.host = host;
        this.appManager = appManager;
        this.scheduler = scheduler;
        this.storage = storage;
    }

    public void Load()
    {
        loadModules();
    }

    public bool DoesModuleExist(string serialisedName) => assemblyContexts.Any(context => context.Assemblies.Any(assembly => assembly.ExportedTypes.Where(type => type.IsSubclassOf(typeof(Module)) && !type.IsAbstract).Any(type => type.Name.ToLowerInvariant() == serialisedName)));
    public bool IsModuleLoaded(string serialisedName) => GetModule(serialisedName) is not null;
    public bool IsModuleEnabled(string serialisedName) => GetModule(serialisedName)?.Enabled.Value ?? false;
    public List<(string, string)> GetMigrations() => Modules.Where(module => module.LegacySerialisedName is not null && IsModuleLoaded(module.SerialisedName)).Select(module => (module.LegacySerialisedName!, module.SerialisedName)).ToList();

    private void loadModules()
    {
        assemblyContexts.Clear();
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

        var context = new AssemblyLoadContext(Guid.NewGuid().ToString());
        assemblyContexts.Add(context);
        loadAssemblyFromPath(context, dllPath);
    }

    private void loadExternalModules()
    {
        try
        {
            var moduleDirectoryPath = storage.GetStorageForDirectory("assemblies").GetFullPath(string.Empty, true);

            // This is just for backwards compatibility for modules that don't require dependencies
            // This may need to kept so that users can test their modules without having to put it in a folder if
            //  a folder is required to contain vrcosc.json
            loadContextFromPath(moduleDirectoryPath);

            Directory.GetDirectories(moduleDirectoryPath).ForEach(loadContextFromPath);
        }
        catch (Exception e)
        {
            Notifications.Notify(new ExceptionNotification("Error when loading external modules"));
            Logger.Error(e, "ModuleManager experienced an exception");
        }
    }

    private void loadContextFromPath(string path)
    {
        var context = new AssemblyLoadContext(Guid.NewGuid().ToString());
        assemblyContexts.Add(context);

        foreach (var dllPath in Directory.GetFiles(path, "*.dll"))
        {
            loadAssemblyFromPath(context, dllPath);
        }
    }

    private void loadAssemblyFromPath(AssemblyLoadContext context, string path)
    {
        Assembly? assembly = null;

        try
        {
            using var assemblyStream = new FileStream(path, FileMode.Open);
            assembly = context.LoadFromStream(assemblyStream);
            assembly.ExportedTypes.Where(type => type.IsSubclassOf(typeof(Module)) && !type.IsAbstract).ForEach(type => registerModule(assembly, type));
        }
        catch (Exception e)
        {
            Notifications.Notify(new ExceptionNotification($"{assembly?.GetAssemblyAttribute<AssemblyProductAttribute>()?.Product} could not be loaded. It may require an update"));
            Logger.Error(e, "ModuleManager experienced an exception");
        }
    }

    private void registerModule(Assembly assembly, Type type)
    {
        try
        {
            var module = (Module)Activator.CreateInstance(type)!;
            module.InjectDependencies(host, appManager, scheduler, storage);
            module.Load();

            var assemblyLookup = assembly.GetName().Name!.ToLowerInvariant();

            ModuleCollections.TryAdd(assemblyLookup, new ModuleCollection(assembly));
            ModuleCollections[assemblyLookup].Modules.Add(module);
        }
        catch (Exception e)
        {
            Notifications.Notify(new ExceptionNotification($"{type.Name} could not be loaded. It may require an update"));
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

    public void ChatBoxUpdate()
    {
        foreach (var module in runningModulesCache.Where(module => module.GetType().IsSubclassOf(typeof(AvatarModule))).Select(module => (AvatarModule)module))
        {
            module.ChatBoxUpdate();
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

    public void ParameterReceived(VRChatOscMessage message)
    {
        foreach (var module in runningModulesCache)
        {
            module.OnParameterReceived(message);
        }
    }

    public void PlayerUpdate()
    {
        foreach (var module in runningModulesCache.Where(module => module.GetType().IsSubclassOf(typeof(AvatarModule))).Select(module => (AvatarModule)module))
        {
            module.PlayerUpdate();
        }
    }

    public Module? GetModule(string serialisedName) => Modules.SingleOrDefault(module => module.SerialisedName == serialisedName);

    public IEnumerable<Module> GetRunningModules() => runningModulesCache;

    public IEnumerable<string> GetEnabledModuleNames() => Modules.Where(module => module.Enabled.Value).Select(module => module.SerialisedName);

    public string GetModuleName(string serialisedName) => Modules.Single(module => module.SerialisedName == serialisedName).Title;

    public IEnumerator<ModuleCollection> GetEnumerator() => ModuleCollections.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
