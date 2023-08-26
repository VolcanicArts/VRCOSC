// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
using WinRT;
using Module = VRCOSC.Game.Modules.Module;

namespace VRCOSC.Game.Managers;

public sealed class ModuleManager : IEnumerable<ModuleCollection>
{
    private static TerminalLogger terminal => new("VRCOSC");

    private readonly List<AssemblyLoadContext> assemblyContexts = new();
    public readonly Dictionary<string, ModuleCollection> ModuleCollections = new();

    internal IReadOnlyList<Module> Modules => ModuleCollections.Values.SelectMany(collection => collection.Modules).ToList();

    internal Action? OnModuleEnabledChanged;

    private GameHost host = null!;
    private AppManager appManager = null!;
    private Scheduler scheduler = null!;
    private Storage storage = null!;

    private readonly List<Module> runningModulesCache = new();

    internal void Initialise(GameHost host, AppManager appManager, Scheduler scheduler, Storage storage)
    {
        this.host = host;
        this.appManager = appManager;
        this.scheduler = scheduler;
        this.storage = storage;
    }

    internal void Load()
    {
        loadModules();
    }

    internal bool IsModuleRunning(Module module) => runningModulesCache.Contains(module);

    internal IEnumerable<T> GetModulesOfType<T>(bool useRunningCache) => (useRunningCache ? runningModulesCache : Modules).Where(module => module.GetType().IsSubclassOf(typeof(T))).Select(module => module.As<T>());

    /// <summary>
    /// Gets a module. Null if the module doesn't exist OR isn't loaded correctly
    /// </summary>
    internal Module? GetModule(string serialiseName) => TryGetModule(serialiseName, out var module) ? module : null;

    /// <summary>
    /// Attempts to get a module. Returns true ONLY if the module exists and is loaded with no errors
    /// </summary>
    internal bool TryGetModule(string serialisedName, [NotNullWhen(true)] out Module? module)
    {
        if (IsModuleLoaded(serialisedName))
        {
            module = getModule(serialisedName)!;
            return true;
        }

        module = null;
        return false;
    }

    internal bool DoesModuleExist(string serialisedName)
    {
        try
        {
            return assemblyContexts.Any(context => context.Assemblies.Any(assembly => assembly.ExportedTypes.Where(type => type.IsSubclassOf(typeof(Module)) && !type.IsAbstract).Any(type => type.Name.ToLowerInvariant() == serialisedName)));
        }
        catch (Exception)
        {
            return false;
        }
    }

    internal bool IsModuleLoaded(string serialisedName) => DoesModuleExist(serialisedName) && getModule(serialisedName) is not null;

    private Module? getModule(string serialisedName) => Modules.SingleOrDefault(module => module.SerialisedName == serialisedName);

    internal List<LegacySerialiseNameMigration> GetMigrations() => Modules.Where(module => module.LegacySerialisedName is not null && IsModuleLoaded(module.SerialisedName)).Select(module => new LegacySerialiseNameMigration(module.LegacySerialisedName!, module.SerialisedName)).ToList();

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

    #region Events

    internal void Start()
    {
        if (Modules.All(module => !module.Enabled.Value))
            terminal.Log("You have no modules selected\nEnable a module in the module listing screen");

        Debug.Assert(!runningModulesCache.Any());

        foreach (var module in Modules.Where(module => module.Enabled.Value))
        {
            module.Start();
            runningModulesCache.Add(module);
        }
    }

    internal void ChatBoxUpdate()
    {
        GetModulesOfType<AvatarModule>(true).ForEach(module => module.ChatBoxUpdate());
    }

    internal void Update()
    {
        scheduler.Update();
    }

    internal void Stop()
    {
        scheduler.CancelDelayedTasks();

        foreach (var module in runningModulesCache)
        {
            module.Stop();
        }

        runningModulesCache.Clear();
    }

    internal void ParameterReceived(VRChatOscMessage message)
    {
        runningModulesCache.ForEach(module => module.OnParameterReceived(message));
    }

    internal void PlayerUpdate()
    {
        GetModulesOfType<AvatarModule>(true).ForEach(module => module.PlayerUpdate());
    }

    #endregion
    internal IEnumerable<string> GetEnabledModuleNames() => Modules.Where(module => module.Enabled.Value).Select(module => module.SerialisedName);

    public IEnumerator<ModuleCollection> GetEnumerator() => ModuleCollections.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal record LegacySerialiseNameMigration(string LegacySerialisedName, string SerialisedName);
