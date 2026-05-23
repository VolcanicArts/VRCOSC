// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using VRCOSC.App.Modules;
using VRCOSC.App.Nodes.Metadata;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes;

public interface IContextMenuEntry
{
    public string Name { get; }
}

public class ContextMenuNodeEntry : IContextMenuEntry
{
    public string Name { get; }
    public string TypeLookup { get; }
    public Type? FilteredGeneric { get; }

    public ContextMenuNodeEntry(string name, string typeLookup, Type? filteredGeneric = null)
    {
        Name = name;
        TypeLookup = typeLookup;
        FilteredGeneric = filteredGeneric;
    }
}

public class ContextMenuSubMenuEntry : IContextMenuEntry
{
    public string Name { get; }
    public List<IContextMenuEntry> Items { get; } = [];

    public ContextMenuSubMenuEntry(string name)
    {
        Name = name;
    }
}

public static class GraphContextMenuBuilder
{
    public static Lazy<IContextMenuEntry> Items = new(build);

    public static void Refresh()
    {
        Items = new Lazy<IContextMenuEntry>(build);
    }

    private static ContextMenuSubMenuEntry build()
    {
        var createNodeSubMenu = new ContextMenuSubMenuEntry("Add Node");

        foreach (var (key, typeMetadata) in NodeTypeManager.Data)
        {
            var currentList = createNodeSubMenu.Items;
            var path = typeMetadata.Shared.Path;

            var moduleNode = typeMetadata.Shared.Type.GetConstructedGenericBase(typeof(ModuleNode<>));

            if (moduleNode is not null)
            {
                var moduleType = moduleNode.GenericTypeArguments[0];
                var module = ModuleManager.GetInstance().GetModuleInstanceFromType(moduleType);
                path = $"Modules/{module.Title}" + (!string.IsNullOrWhiteSpace(path) ? $"/{path}" : string.Empty);
            }

            if (string.IsNullOrEmpty(path) && moduleNode is null) continue;

            var pathParts = path.Split('/');

            foreach (var part in pathParts)
            {
                var list = currentList;

                var submenu = currentList
                              .OfType<ContextMenuSubMenuEntry>()
                              .FirstOrDefault(sm => sm.Name == part)
                              ?? new ContextMenuSubMenuEntry(part).Also(list.Add);

                currentList = submenu.Items;
            }

            addNodeEntry(currentList, key, typeMetadata.Shared);
        }

        sortMenu(createNodeSubMenu.Items);

        return createNodeSubMenu;
    }

    private static void addNodeEntry(List<IContextMenuEntry> list, string typeLookup, INodeSharedMetadata metadata)
    {
        if (metadata.GenericsFilter is { Length: > 0 })
        {
            var genMenu = new ContextMenuSubMenuEntry(metadata.Name);

            foreach (var gt in metadata.GenericsFilter)
            {
                var friendly = $"{metadata.Name} ({gt.GetFriendlyName()})";
                genMenu.Items.Add(new ContextMenuNodeEntry(friendly, typeLookup, gt));
            }

            list.Add(genMenu);
        }
        else
        {
            list.Add(new ContextMenuNodeEntry(metadata.Name, typeLookup));
        }
    }

    private static void sortMenu(List<IContextMenuEntry> entries)
    {
        var sorted = entries
                     .OrderBy(e => e is not ContextMenuSubMenuEntry)
                     .ThenByDescending(e => e.Name == "Modules")
                     .ThenBy(e => e.Name)
                     .ToList();

        entries.Clear();
        entries.AddRange(sorted);

        foreach (var sm in entries.OfType<ContextMenuSubMenuEntry>())
            sortMenu(sm.Items);
    }
}

public static class FluentExtensions
{
    public static T Also<T>(this T obj, Action<T> action)
    {
        action(obj);
        return obj;
    }
}