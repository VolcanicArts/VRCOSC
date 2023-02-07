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
using osu.Framework.Lists;
using osu.Framework.Logging;
using osu.Framework.Platform;
using VRCOSC.Game.OSC.VRChat;

namespace VRCOSC.Game.Modules;

public sealed partial class ModuleManager : CompositeComponent, IEnumerable<Module>
{
    private readonly TerminalLogger terminal = new(nameof(ModuleManager));

    private readonly SortedList<Module> tempModuleList = new();

    [Resolved]
    private Storage storage { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        loadInternalModules();
        loadExternalModules();
        AddRangeInternal(tempModuleList);
    }

    private void loadInternalModules()
    {
        var assemblyPath = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.AllDirectories)
                                    .FirstOrDefault(fileName => fileName.Contains("VRCOSC.Modules"));

        if (string.IsNullOrEmpty(assemblyPath))
        {
            Logger.Log("Could not find internal module assembly");
            return;
        }

        loadModuleAssembly(assemblyPath);
    }

    private void loadExternalModules()
    {
        var moduleDirectoryPath = storage.GetStorageForDirectory("custom").GetFullPath(string.Empty, true);
        Directory.GetFiles(moduleDirectoryPath, "*.dll", SearchOption.AllDirectories)
                 .ForEach(loadModuleAssembly);
    }

    private void loadModuleAssembly(string assemblyPath)
    {
        Assembly.LoadFile(assemblyPath).GetTypes()
                .Where(type => type.IsSubclassOf(typeof(Module)) && !type.IsAbstract)
                .Select(type => (Module)Activator.CreateInstance(type)!)
                .ForEach(module => tempModuleList.Add(module));
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
            module.Start();
        }
    }

    public void Stop()
    {
        foreach (var module in this)
        {
            module.Stop();
        }
    }

    public void OnParameterReceived(VRChatOscData data)
    {
        foreach (var module in this)
        {
            module.OnParameterReceived(data);
        }
    }

    public IEnumerator<Module> GetEnumerator() => InternalChildren.Select(child => (Module)child).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
