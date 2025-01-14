// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using VRCOSC.App.ChatBox;
using VRCOSC.App.Modules.Serialisation;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.SDK.VRChat;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Modules;

internal class ModuleManager : INotifyPropertyChanged
{
    private static ModuleManager? instance;
    internal static ModuleManager GetInstance() => instance ??= new ModuleManager();

    private readonly Storage storage = AppManager.GetInstance().Storage;

    private AssemblyLoadContext? localModulesContext;
    private Dictionary<string, AssemblyLoadContext>? remoteModulesContexts;

    public ObservableDictionary<ModulePackage, List<Module>> Modules { get; } = new();

    public Dictionary<ModulePackage, List<Module>> UIModules
    {
        get
        {
            var orderedModules = new Dictionary<ModulePackage, List<Module>>();
            foreach (var pair in Modules) orderedModules.Add(pair.Key, pair.Value.OrderBy(module => module.Type).ThenBy(module => module.Title).ToList());
            orderedModules = orderedModules.OrderBy(pair => pair.Key.Remote).ThenBy(pair => pair.Key.DisplayName).ToDictionary();
            return orderedModules;
        }
    }

    private IEnumerable<Module> modules => Modules.Values.SelectMany(moduleList => moduleList);
    public IEnumerable<Module> RunningModules => modules.Where(module => module.State.Value == ModuleState.Started);

    private readonly List<string> failedPackageImports = new();
    private readonly List<string> failedModuleImports = new();
    private readonly List<string> failedModuleLoads = new();

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
        RunningModules.ForEach(module => module.InvokePlayerUpdate());
    }

    public void ParameterReceived(VRChatOscMessage vrChatOscMessage)
    {
        RunningModules.ForEach(module => module.OnParameterReceived(vrChatOscMessage));
    }

    public void ChatBoxUpdate()
    {
        RunningModules.ForEach(module => module.InvokeChatBoxUpdate());
    }

    public void AvatarChange(AvatarConfig? avatarConfig)
    {
        RunningModules.ForEach(module => module.InvokeAvatarChange(avatarConfig));
    }

    #endregion

    #region Management

    public IEnumerable<T> GetModulesOfType<T>() => modules.Where(module => module.GetType().IsAssignableTo(typeof(T))).Cast<T>();
    public IEnumerable<T> GetRunningModulesOfType<T>() => RunningModules.Where(module => module.GetType().IsAssignableTo(typeof(T))).Cast<T>();
    public IEnumerable<T> GetEnabledModulesOfType<T>() => modules.Where(module => module.GetType().IsAssignableTo(typeof(T)) && module.Enabled.Value).Cast<T>();

    public Module GetModuleOfID(string moduleID) => modules.First(module => module.FullID == moduleID);

    public IEnumerable<string> GetEnabledModuleIDs() => modules.Where(module => module.Enabled.Value).Select(module => module.FullID);

    public bool IsModuleRunning(string moduleID) => RunningModules.Any(module => module.FullID == moduleID);
    public bool IsModuleLoaded(string moduleID) => modules.Any(module => module.FullID == moduleID);

    /// <summary>
    /// Reloads all local and remote modules by unloading their assembly contexts and calling <see cref="LoadAllModules"/>
    /// Ensures the ChatBox is unloaded before unloading the modules, and is loaded after loading the modules
    /// </summary>
    public async Task ReloadAllModules(Dictionary<string, string>? filePathOverrides = null)
    {
        Logger.Log("Module reload requested");

        await AppManager.GetInstance().StopAsync();

        ChatBoxManager.GetInstance().Unload();
        UnloadAllModules();
        LoadAllModules(filePathOverrides);
        ChatBoxManager.GetInstance().Load();
    }

    public void UnloadAllModules()
    {
        Logger.Log("Unloading all modules");

        Modules.Clear();
        OnPropertyChanged(nameof(UIModules));

        localModulesContext?.Unload();
        remoteModulesContexts?.ForEach(remoteModuleContextPair => remoteModuleContextPair.Value.Unload());

        localModulesContext = null;
        remoteModulesContexts = null;

        Logger.Log("Unloading successful");
    }

    /// <summary>
    /// Loads all local and remote modules from file
    /// </summary>
    public void LoadAllModules(Dictionary<string, string>? filePathOverrides = null)
    {
        failedPackageImports.Clear();
        failedModuleImports.Clear();
        failedModuleLoads.Clear();

        loadLocalModules();
        loadRemoteModules();

        foreach (var module in modules)
        {
            try
            {
                var moduleSerialisationManager = new SerialisationManager();
                moduleSerialisationManager.RegisterSerialiser(1, new ModuleSerialiser(storage, module));

                var modulePersistenceSerialisationManager = new SerialisationManager();
                modulePersistenceSerialisationManager.RegisterSerialiser(1, new ModulePersistenceSerialiser(storage, module));

                module.InjectDependencies(moduleSerialisationManager, modulePersistenceSerialisationManager);

                if (filePathOverrides is not null && filePathOverrides.TryGetValue(module.FullID, out var filePathOverride))
                {
                    module.Load(filePathOverride);
                }
                else
                {
                    module.Load();
                }
            }
            catch (Exception e)
            {
                failedModuleLoads.Add(module.FullID);
                Logger.Error(e, $"Module '{module.FullID}' failed to load");
            }
        }

        // remove failed modules from the loaded list
        foreach (var pair in Modules)
        {
            pair.Value.RemoveIf(module => failedModuleLoads.Contains(module.FullID));
        }

        OnPropertyChanged(nameof(UIModules));

        buildErrorMessageBox();
    }

    private void buildErrorMessageBox()
    {
        if (failedPackageImports.Count == 0 && failedModuleImports.Count == 0 && failedModuleLoads.Count == 0) return;

        var errorBuilder = new StringBuilder();

        if (failedPackageImports.Count != 0)
        {
            errorBuilder.AppendLine("The following packages failed to import:");
            failedPackageImports.ForEach(packageId => errorBuilder.AppendLine(packageId));
            errorBuilder.AppendLine();
        }

        if (failedModuleImports.Count != 0)
        {
            errorBuilder.AppendLine("The following modules failed to import:");
            failedModuleImports.ForEach(moduleId => errorBuilder.AppendLine(moduleId));
            errorBuilder.AppendLine();
        }

        if (failedModuleLoads.Count != 0)
        {
            errorBuilder.AppendLine("The following modules failed to load:");
            failedModuleLoads.ForEach(moduleId => errorBuilder.AppendLine(moduleId));
            errorBuilder.AppendLine();
        }

        errorBuilder.Append("This is usually a sign that module packages need to be updated");

        ExceptionHandler.Handle(errorBuilder.ToString());
    }

    private void loadLocalModules()
    {
        Logger.Log("Loading local modules");

        if (localModulesContext is not null)
            throw new InvalidOperationException("Cannot load local modules while local modules are already loaded");

        var localModulesPath = storage.GetStorageForDirectory("packages/local").GetFullPath(string.Empty, true);

        try
        {
            localModulesContext = loadContextFromPath(localModulesPath);
        }
        catch (Exception e)
        {
            localModulesContext = null;
            failedPackageImports.Add("local");
            Logger.Error(e, "Package 'local' failed to import");
            return;
        }

        Logger.Log($"Found {localModulesContext.Assemblies.Count()} assemblies");

        if (!localModulesContext.Assemblies.Any()) return;

        List<Module> localModules;

        try
        {
            localModules = retrieveModuleInstances("local", localModulesContext);
        }
        catch (Exception e)
        {
            localModulesContext = null;
            failedPackageImports.Add("local");
            Logger.Error(e, "Package 'local' failed to import");
            return;
        }

        foreach (var localModule in localModules)
        {
            if (Modules.All<ObservableKeyValuePair<ModulePackage, List<Module>>>(pair => pair.Key.Assembly != localModule.GetType().Assembly))
            {
                Modules[new ModulePackage(localModule.GetType().Assembly, false)] = new List<Module>();
            }

            Modules.First<ObservableKeyValuePair<ModulePackage, List<Module>>>(pair => pair.Key.Assembly == localModule.GetType().Assembly).Value.Add(localModule);
        }

        Logger.Log($"Final local module count: {localModules.Count}");
    }

    private void loadRemoteModules()
    {
        Logger.Log("Loading remote modules");

        if (remoteModulesContexts is not null)
            throw new InvalidOperationException("Cannot load remote modules while remote modules are already loaded");

        remoteModulesContexts = new Dictionary<string, AssemblyLoadContext>();

        var remoteModulesDirectory = storage.GetStorageForDirectory("packages/remote").GetFullPath(string.Empty, true);

        foreach (var moduleDirectory in Directory.GetDirectories(remoteModulesDirectory))
        {
            var packageId = moduleDirectory.Split('\\').Last();

            AssemblyLoadContext assemblyLoadContext;

            try
            {
                assemblyLoadContext = loadContextFromPath(moduleDirectory);
            }
            catch (Exception e)
            {
                failedPackageImports.Add(packageId);
                Logger.Error(e, $"Package '{packageId}' failed to import");
                continue;
            }

            remoteModulesContexts.Add(packageId, assemblyLoadContext);
        }

        Logger.Log($"Found {remoteModulesContexts.Values.Sum(remoteModuleContext => remoteModuleContext.Assemblies.Count())} assemblies");

        if (remoteModulesContexts.Count == 0) return;

        var remoteModules = new List<Module>();

        foreach (var pair in remoteModulesContexts)
        {
            try
            {
                var moduleInstances = retrieveModuleInstances(pair.Key, pair.Value);
                remoteModules.AddRange(moduleInstances);
            }
            catch (Exception e)
            {
                failedPackageImports.Add(pair.Key);
                Logger.Error(e, $"Package '{pair.Key}' failed to import");
            }
        }

        // remove packages if they've failed to import
        remoteModulesContexts.RemoveIf(pair => failedPackageImports.Contains(pair.Key));

        foreach (var remoteModule in remoteModules)
        {
            if (Modules.All<ObservableKeyValuePair<ModulePackage, List<Module>>>(pair => pair.Key.Assembly != remoteModule.GetType().Assembly))
            {
                Modules[new ModulePackage(remoteModule.GetType().Assembly, true)] = new List<Module>();
            }

            Modules.First<ObservableKeyValuePair<ModulePackage, List<Module>>>(pair => pair.Key.Assembly == remoteModule.GetType().Assembly).Value.Add(remoteModule);
        }

        Logger.Log($"Final remote module count: {remoteModules.Count}");
    }

    private List<Module> retrieveModuleInstances(string packageId, AssemblyLoadContext assemblyLoadContext)
    {
        var moduleInstanceList = new List<Module>();

        foreach (var assembly in assemblyLoadContext.Assemblies)
        {
            var moduleTypes = assembly.ExportedTypes.Where(type => type.IsSubclassOf(typeof(Module)) && !type.IsAbstract);

            foreach (var moduleType in moduleTypes)
            {
                try
                {
                    var module = (Module)Activator.CreateInstance(moduleType)!;
                    module.PackageID = packageId;
                    moduleInstanceList.Add(module);
                }
                catch (Exception e)
                {
                    failedModuleImports.Add($"{packageId}.{moduleType.Name.ToLowerInvariant()}");
                    Logger.Error(e, $"Module '{packageId}.{moduleType.Name.ToLowerInvariant()}' failed to import");
                }
            }
        }

        return moduleInstanceList;
    }

    private AssemblyLoadContext loadContextFromPath(string path)
    {
        var assemblyLoadContext = new AssemblyLoadContext(null, true);
        foreach (var dllPath in Directory.GetFiles(path, "*.dll")) loadAssemblyFromPath(assemblyLoadContext, dllPath);
        return assemblyLoadContext;
    }

    private static void loadAssemblyFromPath(AssemblyLoadContext context, string path)
    {
        using var assemblyStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        context.LoadFromStream(assemblyStream);
    }

    #endregion

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}