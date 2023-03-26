// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Containers;
using osu.Framework.Lists;
using osu.Framework.Logging;
using VRCOSC.Game.Modules.Serialisation;
using VRCOSC.Game.Modules.Sources;

namespace VRCOSC.Game.Modules.Manager;

public sealed partial class ModuleManager : CompositeComponent, IModuleManager
{
    private static TerminalLogger terminal => new("ModuleManager");

    private readonly List<IModuleSource> sources = new();
    private readonly SortedList<Module> modules = new();
    private IModuleSerialiser? serialiser;

    public void AddSource(IModuleSource source) => sources.Add(source);
    public bool RemoveSource(IModuleSource source) => sources.Remove(source);
    public void SetSerialiser(IModuleSerialiser serialiser) => this.serialiser = serialiser;

    public void Load()
    {
        modules.Clear();

        sources.ForEach(source =>
        {
            foreach (var type in source.Load())
            {
                var module = (Module)Activator.CreateInstance(type)!;
                modules.Add(module);
            }
        });

        // TODO - Remove after decoupling
        AddRangeInternal(modules);

        if (serialiser is null)
        {
            Logger.Log("Warning. No serialiser has been set. Aborting module load");
            return;
        }

        foreach (var module in this)
        {
            module.Load();
            serialiser?.Deserialise(module);
        }
    }

    public void Start()
    {
        if (modules.All(module => !module.Enabled.Value))
            terminal.Log("You have no modules selected!\nSelect some modules to begin using VRCOSC");

        foreach (var module in modules)
        {
            module.Start();
        }
    }

    public void Stop()
    {
        foreach (var module in modules)
        {
            module.Stop();
        }
    }

    public IEnumerator<Module> GetEnumerator() => modules.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
