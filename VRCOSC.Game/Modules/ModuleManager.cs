// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;

namespace VRCOSC.Game.Modules;

public sealed partial class ModuleManager : CompositeComponent, IEnumerable<Module>
{
    private readonly TerminalLogger terminal = new(nameof(ModuleManager));

    private IReadOnlyList<Type> moduleTypes = null!;

    [Resolved]
    private Storage storage { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        loadInternalModules();
        loadExternalModules();
    }

    private void loadInternalModules()
    {
        moduleTypes.ForEach(instanciateModule);
    }

    public void RegisterInternalModules(IReadOnlyList<Type> moduleTypes)
    {
        this.moduleTypes = moduleTypes;
    }

    private void loadExternalModules()
    {
        var moduleDirectoryPath = storage.GetStorageForDirectory("custom").GetFullPath(string.Empty, true);
        Directory.GetFiles(moduleDirectoryPath, "*.dll").ForEach(createModule);
    }

    private void createModule(string dllPath)
    {
        var moduleAssemblyTypes = Assembly.LoadFile(dllPath).GetTypes();
        var moduleType = moduleAssemblyTypes.First(type => type.IsSubclassOf(typeof(Module)) && !type.IsAbstract);
        instanciateModule(moduleType);
    }

    private void instanciateModule(Type type)
    {
        var instance = (Module)Activator.CreateInstance(type)!;
        AddInternal(instance);
    }

    public void Start()
    {
        if (this.All(module => !module.Enabled.Value))
        {
            terminal.Log("You have no modules selected!\nSelect some modules to begin using VRCOSC");
            return;
        }

        foreach (var module in this)
        {
            module.start();
        }
    }

    public void Stop()
    {
        foreach (var module in this)
        {
            module.stop();
        }
    }

    public IEnumerator<Module> GetEnumerator() => InternalChildren.Select(child => (Module)child).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
