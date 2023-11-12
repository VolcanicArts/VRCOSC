// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using Module = VRCOSC.Game.Modules.SDK.Module;

namespace VRCOSC.Game.Modules;

public class ModuleManager
{
    private readonly Storage storage;

    private AssemblyLoadContext? localModulesContext;
    private List<AssemblyLoadContext>? remoteModulesContexts;

    public readonly Dictionary<Assembly, List<Module>> LocalModules = new();
    public readonly Dictionary<Assembly, List<Module>> RemoteModules = new();

    public ModuleManager(Storage storage)
    {
        this.storage = storage;
    }

    #region Runtime

    public async void Start()
    {
        await startLocalModules();
        await startRemoteModules();
    }

    private async Task startLocalModules()
    {
        Logger.Log("Starting local modules");

        foreach (var localModule in LocalModules.Values.SelectMany(localModuleList => localModuleList))
        {
            await localModule.Start();
        }
    }

    private async Task startRemoteModules()
    {
        Logger.Log("Starting remote modules");

        foreach (var remoteModule in RemoteModules.Values.SelectMany(remoteModuleList => remoteModuleList))
        {
            await remoteModule.Start();
        }
    }

    #endregion

    #region Management

    /// <summary>
    /// Reloads all local and remote modules by unloading their assembly contexts and calling <see cref="LoadAllModules"/>
    /// </summary>
    public void ReloadAllModules()
    {
        LocalModules.Clear();
        RemoteModules.Clear();

        localModulesContext?.Unload();
        remoteModulesContexts?.ForEach(remoteModuleContext => remoteModuleContext.Unload());

        LoadAllModules();
    }

    /// <summary>
    /// Loads all local and remote modules
    /// </summary>
    public void LoadAllModules()
    {
        loadLocalModules();
        loadRemoteModules();
    }

    private void loadLocalModules()
    {
        Logger.Log("Loading local modules");

        if (localModulesContext is not null)
            throw new InvalidOperationException("Cannot load local modules while local modules are already loaded");

        var localModulesPath = storage.GetStorageForDirectory("modules/local").GetFullPath(string.Empty, true);
        localModulesContext = loadContextFromPath(localModulesPath);
        Logger.Log($"Found {localModulesContext.Assemblies.Count()} assemblies");

        var localModules = retrieveModuleInstances(localModulesContext);

        localModules.ForEach(localModule =>
        {
            if (!LocalModules.ContainsKey(localModule.GetType().Assembly))
            {
                LocalModules[localModule.GetType().Assembly] = new List<Module>();
            }

            LocalModules[localModule.GetType().Assembly].Add(localModule);
        });

        Logger.Log($"Final local module count: {localModules.Count}");
    }

    private void loadRemoteModules()
    {
        Logger.Log("Loading remote modules");

        if (remoteModulesContexts is not null)
            throw new InvalidOperationException("Cannot load remote modules while remote modules are already loaded");

        remoteModulesContexts = new List<AssemblyLoadContext>();

        var remoteModulesDirectory = storage.GetStorageForDirectory("modules/remote").GetFullPath(string.Empty, true);
        Directory.GetDirectories(remoteModulesDirectory).ForEach(moduleDirectory => remoteModulesContexts.Add(loadContextFromPath(moduleDirectory)));
        Logger.Log($"Found {remoteModulesContexts.Sum(remoteModuleContext => remoteModuleContext.Assemblies.Count())} assemblies");

        var remoteModules = new List<Module>();
        remoteModulesContexts.ForEach(remoteModuleContext => remoteModules.AddRange(retrieveModuleInstances(remoteModuleContext)));

        remoteModules.ForEach(remoteModule =>
        {
            if (!RemoteModules.ContainsKey(remoteModule.GetType().Assembly))
            {
                RemoteModules[remoteModule.GetType().Assembly] = new List<Module>();
            }

            RemoteModules[remoteModule.GetType().Assembly].Add(remoteModule);
        });

        Logger.Log($"Final remote module count: {remoteModules.Count}");
    }

    private List<Module> retrieveModuleInstances(AssemblyLoadContext assemblyLoadContext)
    {
        var moduleInstanceList = new List<Module>();

        try
        {
            assemblyLoadContext.Assemblies.ForEach(assembly => moduleInstanceList.AddRange(assembly.ExportedTypes.Where(type => type.IsSubclassOf(typeof(Module)) && !type.IsAbstract).Select(type => (Module)Activator.CreateInstance(type)!)));
        }
        catch
        {
        }

        return moduleInstanceList;
    }

    private AssemblyLoadContext loadContextFromPath(string path)
    {
        var assemblyLoadContext = new AssemblyLoadContext(null, true);
        Directory.GetFiles(path, "*.dll").ForEach(dllPath => loadAssemblyFromPath(assemblyLoadContext, dllPath));
        return assemblyLoadContext;
    }

    private void loadAssemblyFromPath(AssemblyLoadContext context, string path)
    {
        try
        {
            using var assemblyStream = new FileStream(path, FileMode.Open);
            context.LoadFromStream(assemblyStream);
        }
        catch (Exception e)
        {
            Logger.Error(e, "ModuleManager experienced an exception");
        }
    }

    #endregion
}
