// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using osu.Framework.Lists;

namespace VRCOSC.Game.Modules;

public class ModuleCollection : IEnumerable<Module>
{
    public readonly Assembly Assembly;
    public readonly SortedList<Module> Modules = new();

    public string Title => Assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "UNKNOWN";

    public ModuleCollection(Assembly assembly)
    {
        Assembly = assembly;
    }

    public IEnumerator<Module> GetEnumerator() => Modules.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
