// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.UI.Views.Nodes;

public class ContextMenuItem : IContextMenuEntry
{
    public string Name { get; }
    public Type Type { get; }

    public ContextMenuItem(string name, Type type)
    {
        Name = name;
        Type = type;
    }
}

public class ContextMenuSubMenu : IContextMenuEntry
{
    public string Name { get; }
    public List<IContextMenuEntry> Items { get; } = [];

    public ContextMenuSubMenu(string name)
    {
        Name = name;
    }
}

public class ContextMenuRoot
{
    public List<IContextMenuEntry> Items { get; } = [];
}

public interface IContextMenuEntry
{
    public string Name { get; }
}

public static class ContextMenuBuilder
{
    public static ContextMenuRoot BuildFromNodes(IEnumerable<Type> nodeTypes)
    {
        var root = new ContextMenuRoot();

        foreach (var type in nodeTypes)
        {
            var attr = type.GetCustomAttribute<NodeAttribute>();
            if (attr == null || string.IsNullOrEmpty(attr.Path)) continue;

            var currentList = root.Items;

            if (!string.IsNullOrWhiteSpace(attr.Path))
            {
                var pathParts = attr.Path.Split('/');

                foreach (var part in pathParts)
                {
                    var existing = currentList
                                   .OfType<ContextMenuSubMenu>()
                                   .FirstOrDefault(sub => sub.Name == part);

                    if (existing == null)
                    {
                        existing = new ContextMenuSubMenu(part);
                        currentList.Add(existing);
                    }

                    currentList = existing.Items;
                }
            }

            currentList.Add(new ContextMenuItem(attr.Title, type));
        }

        sortMenu(root.Items);
        return root;
    }

    private static void sortMenu(List<IContextMenuEntry> entries)
    {
        var sorted = entries
                     .OrderBy(e => e is not ContextMenuSubMenu)
                     .ThenBy(e => e.Name)
                     .ToList();

        entries.Clear();
        entries.AddRange(sorted);

        foreach (var submenu in entries.OfType<ContextMenuSubMenu>())
        {
            sortMenu(submenu.Items);
        }
    }
}


public static class ContextMenuDebugger
{
    public static void PrintMenu(ContextMenuRoot root)
    {
        foreach (var entry in root.Items)
        {
            printEntry(entry, 0);
        }
    }

    private static void printEntry(IContextMenuEntry entry, int indent)
    {
        var indentStr = new string(' ', indent * 4);

        if (entry is ContextMenuSubMenu submenu)
        {
            Console.WriteLine($"{indentStr}F {submenu.Name}");

            foreach (var child in submenu.Items)
            {
                printEntry(child, indent + 1);
            }
        }
        else if (entry is ContextMenuItem item)
        {
            Console.WriteLine($"{indentStr}I {item.Name}");
        }
    }
}