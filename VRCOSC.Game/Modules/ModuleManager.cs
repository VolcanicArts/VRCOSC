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
using osu.Framework.Timing;
using VRCOSC.Config;
using VRCOSC.Modules.Persistence;
using VRCOSC.Modules.Serialisation;
using VRCOSC.OSC.VRChat;
using VRCOSC.SDK;
using VRCOSC.SDK.Avatars;
using VRCOSC.Serialisation;
using Module = VRCOSC.SDK.Module;

namespace VRCOSC.Modules;

public class ModuleManager
{
    private readonly GameHost host;
    private readonly Storage storage;
    private readonly IClock clock;
    private readonly AppManager appManager;
    private readonly VRCOSCConfigManager configManager;

    private AssemblyLoadContext? localModulesContext;
    private Dictionary<string, AssemblyLoadContext>? remoteModulesContexts;

    public readonly Dictionary<Assembly, List<Module>> LocalModules = new();
    public readonly Dictionary<Assembly, List<Module>> RemoteModules = new();

    private IEnumerable<Module> modules => LocalModules.Values.SelectMany(moduleList => moduleList).Concat(RemoteModules.Values.SelectMany(moduleList => moduleList)).ToList();
    private IEnumerable<Module> runningModules => modules.Where(module => module.State.Value == ModuleState.Started);

    public ModuleManager(GameHost host, Storage storage, IClock clock, AppManager appManager, VRCOSCConfigManager configManager)
    {
        this.host = host;
        this.storage = storage;
        this.clock = clock;
        this.appManager = appManager;
        this.configManager = configManager;
    }

    #region Runtime

    public Task StartAsync()
    {
        return Task.WhenAll(modules.Where(module => module.Enabled.Value).Select(module => module.Start()));
    }

    public Task StopAsync()
    {
        return Task.WhenAll(runningModules.Select(module => module.Stop()));
    }

    public void FrameworkUpdate()
    {
        runningModules.ForEach(module => module.FrameworkUpdate());
    }

    public void PlayerUpdate()
    {
        runningModules.OfType<AvatarModule>().ForEach(module => module.PlayerUpdate());
    }

    public void ParameterReceived(VRChatOscMessage vrChatOscMessage)
    {
        runningModules.ForEach(module => module.OnParameterReceived(vrChatOscMessage));
    }

    #endregion

    #region Management

    /// <summary>
    /// Reloads all local and remote modules by unloading their assembly contexts and calling <see cref="LoadAllModules"/>
    /// </summary>
    public void ReloadAllModules()
    {
        Logger.Log("Module reload requested");

        UnloadAllModules();
        LoadAllModules();
    }

    public void UnloadAllModules()
    {
        Logger.Log("Unloading all modules");

        LocalModules.Clear();
        RemoteModules.Clear();

        localModulesContext?.Unload();
        remoteModulesContexts?.ForEach(remoteModuleContextPair => remoteModuleContextPair.Value.Unload());

        localModulesContext = null;
        remoteModulesContexts = null;

        Logger.Log("Unloading successful");
    }

    /// <summary>
    /// Loads all local and remote modules from file
    /// </summary>
    public void LoadAllModules()
    {
        try
        {
            loadLocalModules();
            loadRemoteModules();

            modules.ForEach(module =>
            {
                var moduleSerialisationManager = new SerialisationManager();
                moduleSerialisationManager.RegisterSerialiser(1, new ModuleSerialiser(storage, module, appManager.ProfileManager.ActiveProfile));

                var modulePersistenceSerialisationManager = new SerialisationManager();
                modulePersistenceSerialisationManager.RegisterSerialiser(1, new ModulePersistenceSerialiser(storage, module, appManager.ProfileManager.ActiveProfile, configManager.GetBindable<bool>(VRCOSCSetting.GlobalPersistence)));

                module.InjectDependencies(host, clock, appManager, moduleSerialisationManager, modulePersistenceSerialisationManager, configManager);
                module.Load();
            });
        }
        catch (Exception)
        {
            // TODO: Handle this
        }
    }

    private void loadLocalModules()
    {
        Logger.Log("Loading local modules");

        if (localModulesContext is not null)
            throw new InvalidOperationException("Cannot load local modules while local modules are already loaded");

        var localModulesPath = storage.GetStorageForDirectory("packages/local").GetFullPath(string.Empty, true);
        localModulesContext = loadContextFromPath(localModulesPath);
        Logger.Log($"Found {localModulesContext.Assemblies.Count()} assemblies");

        if (!localModulesContext.Assemblies.Any()) return;

        var localModules = retrieveModuleInstances("local", localModulesContext);

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

        remoteModulesContexts = new Dictionary<string, AssemblyLoadContext>();

        var remoteModulesDirectory = storage.GetStorageForDirectory("packages/remote").GetFullPath(string.Empty, true);
        Directory.GetDirectories(remoteModulesDirectory).ForEach(moduleDirectory =>
        {
            // TODO: Improve this to use the package manager
            var packageId = moduleDirectory.Split('\\').Last();
            remoteModulesContexts.Add(packageId, loadContextFromPath(moduleDirectory));
        });
        Logger.Log($"Found {remoteModulesContexts.Values.Sum(remoteModuleContext => remoteModuleContext.Assemblies.Count())} assemblies");

        if (!remoteModulesContexts.Any()) return;

        var remoteModules = new List<Module>();
        remoteModulesContexts.ForEach(pair => remoteModules.AddRange(retrieveModuleInstances(pair.Key, pair.Value)));

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

    private List<Module> retrieveModuleInstances(string packageId, AssemblyLoadContext assemblyLoadContext)
    {
        var moduleInstanceList = new List<Module>();

        try
        {
            assemblyLoadContext.Assemblies.ForEach(assembly => moduleInstanceList.AddRange(assembly.ExportedTypes.Where(type => type.IsSubclassOf(typeof(Module)) && !type.IsAbstract).Select(type =>
            {
                var module = (Module)Activator.CreateInstance(type)!;
                module.PackageId = packageId;
                return module;
            })));
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error in ModuleManager");
        }

        return moduleInstanceList;
    }

    private AssemblyLoadContext loadContextFromPath(string path)
    {
        var assemblyLoadContext = new AssemblyLoadContext(null, true);

        assemblyLoadContext.Resolving += (_, name) =>
        {
            Logger.Log($"Could not load assembly {name.Name} - {name.Version}");

            var foundAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(assembly => assembly.GetName().Name == name.Name && assembly.GetName().Version != name.Version).ToList();
            foundAssemblies.ForEach(assembly => Logger.Log($"Candidate: {assembly.GetName().Name} - {assembly.GetName().Version}"));

            var fallbackAssembly = foundAssemblies.FirstOrDefault();

            if (fallbackAssembly is null)
            {
                Logger.Log("No suitable fallback found");
                return null;
            }

            Logger.Log($"Falling back to {fallbackAssembly.GetName().Name} - {fallbackAssembly.GetName().Version}");
            return assemblyLoadContext.LoadFromStream(new MemoryStream(File.ReadAllBytes(fallbackAssembly.Location)));
        };

        Directory.GetFiles(path, "*.dll").ForEach(dllPath => loadAssemblyFromPath(assemblyLoadContext, dllPath));
        return assemblyLoadContext;
    }

    private static void loadAssemblyFromPath(AssemblyLoadContext context, string path)
    {
        try
        {
            using var assemblyStream = new FileStream(path, FileMode.Open);
            loadAssemblyFromStream(context, assemblyStream);
        }
        catch (Exception e)
        {
            Logger.Error(e, "ModuleManager experienced an exception");
        }
    }

    private static void loadAssemblyFromStream(AssemblyLoadContext context, Stream assemblyStream)
    {
        try
        {
            context.LoadFromStream(assemblyStream);
        }
        catch (Exception e)
        {
            Logger.Error(e, "ModuleManager experienced an exception");
        }
    }

    #endregion
}
