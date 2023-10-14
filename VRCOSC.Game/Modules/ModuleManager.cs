// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Logging;
using osu.Framework.Platform;

namespace VRCOSC.Game.Modules;

public class ModuleManager
{
    private readonly Storage storage;

    private AssemblyLoadContext? localModulesContext;
    public List<Module>? LocalModules { get; private set; }

    private List<AssemblyLoadContext>? remoteModulesContexts;
    public List<Module>? RemoteModules { get; private set; }

    public ModuleManager(Storage storage)
    {
        this.storage = storage;
    }

    /// <summary>
    /// Reloads all local and remote modules by unloading their assembly contexts and calling <see cref="LoadAllModules"/>
    /// </summary>
    public void ReloadAllModules()
    {
        LocalModules = null;
        RemoteModules = null;

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

        LocalModules = retrieveModuleInstances(localModulesContext);

        Logger.Log($"Final local module count: {LocalModules.Count}");
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

        RemoteModules = new List<Module>();
        remoteModulesContexts.ForEach(remoteModuleContext => RemoteModules.AddRange(retrieveModuleInstances(remoteModuleContext)));

        Logger.Log($"Final remote module count: {RemoteModules.Count}");
    }

    private List<Module> retrieveModuleInstances(AssemblyLoadContext assemblyLoadContext)
    {
        var moduleInstanceList = new List<Module>();
        assemblyLoadContext.Assemblies.ForEach(assembly => moduleInstanceList.AddRange(assembly.ExportedTypes.Where(type => type.IsSubclassOf(typeof(Module)) && !type.IsAbstract).Select(type => (Module)Activator.CreateInstance(type)!)));
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
}
