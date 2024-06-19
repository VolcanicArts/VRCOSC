// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Threading.Tasks;
using VRCOSC.App.ChatBox;
using VRCOSC.App.Modules.Serialisation;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.Profiles;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Settings;
using VRCOSC.App.Utils;
using Logger = VRCOSC.App.Utils.Logger;

namespace VRCOSC.App.Modules;

public class ModuleManager : INotifyPropertyChanged
{
    private static ModuleManager? instance;
    internal static ModuleManager GetInstance() => instance ??= new ModuleManager();

    private readonly Storage storage = AppManager.GetInstance().Storage;

    private AssemblyLoadContext? localModulesContext;
    private Dictionary<string, AssemblyLoadContext>? remoteModulesContexts;

    private IEnumerable<AssemblyLoadContext?> joinedAssemblyLoadContexts => remoteModulesContexts?.Values.Concat(new[] { localModulesContext }) ?? new[] { localModulesContext };

    public ObservableDictionary<ModulePackage, List<Module>> Modules { get; } = new();

    public Dictionary<ModulePackage, List<Module>> UIModules
    {
        get
        {
            // TODO: Order packages so that local is always at the bottom and official modules are always at the top

            var orderedModules = new Dictionary<ModulePackage, List<Module>>();
            foreach (var pair in Modules) orderedModules.Add(pair.Key, pair.Value.OrderBy(module => module.Type).ThenBy(module => module.Title).ToList());
            return orderedModules;
        }
    }

    private IEnumerable<Module> modules => Modules.Values.SelectMany(moduleList => moduleList);
    public IEnumerable<Module> RunningModules => modules.Where(module => module.State.Value == ModuleState.Started);

    #region Runtime

    public Task StartAsync()
    {
        return Task.WhenAll(modules.Where(module => module.Enabled.Value).Select(module => module.Start()));
    }

    public Task StopAsync()
    {
        return Task.WhenAll(RunningModules.Select(module => module.Stop()));
    }

    public void PlayerUpdate()
    {
        RunningModules.OfType<AvatarModule>().ForEach(module => module.PlayerUpdate());
    }

    public void ParameterReceived(VRChatOscMessage vrChatOscMessage)
    {
        RunningModules.ForEach(module => module.OnParameterReceived(vrChatOscMessage));
    }

    public void ChatBoxUpdate()
    {
        RunningModules.OfType<ChatBoxModule>().ForEach(module => module.ChatBoxUpdate());
    }

    #endregion

    #region Management

    public IEnumerable<T> GetModulesOfType<T>() => modules.Where(module => module.GetType().IsAssignableTo(typeof(T))).Cast<T>();

    public IEnumerable<T> GetRunningModulesOfType<T>() => modules.Where(module => RunningModules.Contains(module) && module.GetType().IsAssignableTo(typeof(T))).Cast<T>();

    public Module GetModuleOfID(string moduleID) => modules.First(module => module.FullID == moduleID);

    public IEnumerable<string> GetEnabledModuleIDs() => modules.Where(module => module.Enabled.Value).Select(module => module.FullID);

    public bool IsModuleRunning(string moduleID) => RunningModules.Any(module => module.FullID == moduleID);

    internal bool DoesModuleExist(string moduleID)
    {
        try
        {
            return joinedAssemblyLoadContexts.Where(context => context is not null).Any(context => context!.Assemblies.Any(assembly => assembly.ExportedTypes.Where(type => type.IsSubclassOf(typeof(Module)) && !type.IsAbstract).Any(type => moduleID.EndsWith(type.Name.ToLowerInvariant()))));
        }
        catch (Exception)
        {
            return false;
        }
    }

    internal bool IsModuleLoaded(string moduleID) => DoesModuleExist(moduleID) && getModule(moduleID) is not null;

    private Module? getModule(string moduleID) => modules.SingleOrDefault(module => module.FullID == moduleID);

    /// <summary>
    /// Reloads all local and remote modules by unloading their assembly contexts and calling <see cref="LoadAllModules"/>
    /// </summary>
    public void ReloadAllModules()
    {
        Logger.Log("Module reload requested");

        ChatBoxManager.GetInstance().Unload();
        UnloadAllModules();
        LoadAllModules();
        ChatBoxManager.GetInstance().Load();
    }

    public void UnloadAllModules()
    {
        Logger.Log("Unloading all modules");

        Modules.Clear();

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

            var failedModulesList = new List<(Module, Exception)>();

            modules.ForEach(module =>
            {
                try
                {
                    var moduleSerialisationManager = new SerialisationManager();
                    moduleSerialisationManager.RegisterSerialiser(1, new ModuleSerialiser(storage, module, ProfileManager.GetInstance().ActiveProfile));

                    var modulePersistenceSerialisationManager = new SerialisationManager();
                    modulePersistenceSerialisationManager.RegisterSerialiser(1, new ModulePersistenceSerialiser(storage, module, ProfileManager.GetInstance().ActiveProfile, SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.GlobalPersistence)));

                    module.InjectDependencies(moduleSerialisationManager, modulePersistenceSerialisationManager);
                    module.Load();
                }
                catch (Exception e)
                {
                    failedModulesList.Add((module, e));
                }
            });

            failedModulesList.ForEach(instance =>
            {
                var module = instance.Item1;

                foreach (var pair in Modules)
                {
                    if (pair.Value.Contains(module))
                    {
                        pair.Value.Remove(module);
                    }
                }
            });

            if (failedModulesList.Any())
            {
                var message = "The following modules failed to load:\n";
                message += string.Join("\n", failedModulesList.Select(instance => instance.Item1.FullID));
                ExceptionHandler.Handle(message);
            }

            OnPropertyChanged(nameof(UIModules));
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"{nameof(ModuleManager)} has experienced an exception");
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
            if (Modules.All<ObservableKeyValuePair<ModulePackage, List<Module>>>(pair => pair.Key.Assembly != localModule.GetType().Assembly))
            {
                Modules[new ModulePackage(localModule.GetType().Assembly, false)] = new List<Module>();
            }

            Modules.First<ObservableKeyValuePair<ModulePackage, List<Module>>>(pair => pair.Key.Assembly == localModule.GetType().Assembly).Value.Add(localModule);
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
            var packageId = moduleDirectory.Split('\\').Last();
            remoteModulesContexts.Add(packageId, loadContextFromPath(moduleDirectory));
        });
        Logger.Log($"Found {remoteModulesContexts.Values.Sum(remoteModuleContext => remoteModuleContext.Assemblies.Count())} assemblies");

        if (!remoteModulesContexts.Any()) return;

        var remoteModules = new List<Module>();
        remoteModulesContexts.ForEach(pair => remoteModules.AddRange(retrieveModuleInstances(pair.Key, pair.Value)));

        remoteModules.ForEach(remoteModule =>
        {
            if (Modules.All<ObservableKeyValuePair<ModulePackage, List<Module>>>(pair => pair.Key.Assembly != remoteModule.GetType().Assembly))
            {
                Modules[new ModulePackage(remoteModule.GetType().Assembly, true)] = new List<Module>();
            }

            Modules.First<ObservableKeyValuePair<ModulePackage, List<Module>>>(pair => pair.Key.Assembly == remoteModule.GetType().Assembly).Value.Add(remoteModule);
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
                module.PackageID = packageId;
                return module;
            })));
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"Error in {nameof(ModuleManager)} when attempting to load a module package with ID '{packageId}'");
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
            ExceptionHandler.Handle(e, $"{nameof(ModuleManager)} experienced an exception when attempting to load the assembly from path '{path}'");
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
            ExceptionHandler.Handle(e, $"{nameof(ModuleManager)} experienced an exception");
        }
    }

    #endregion

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
