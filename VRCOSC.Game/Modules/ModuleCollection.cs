// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using osu.Framework.Lists;

namespace VRCOSC.Game.Modules;

/// <summary>
/// A collection of instanciated <see cref="Module"/>s that are present inside a given <see cref="Assembly"/>
/// </summary>
/// <remarks>Modules are sorted automatically based on <see cref="Module.CompareTo"/></remarks>
public class ModuleCollection : IEnumerable<Module>
{
    public readonly Assembly Assembly;
    public readonly SortedList<Module> Modules = new();

    /// <summary>
    /// The title of this module collection, currently denoted by the product name of the <see cref="ModuleCollection.Assembly"/>
    /// </summary>
    public string Title => Assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "UNKNOWN";

    public ModuleCollection(Assembly assembly)
    {
        Assembly = assembly;
    }

    public IEnumerator<Module> GetEnumerator() => Modules.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
