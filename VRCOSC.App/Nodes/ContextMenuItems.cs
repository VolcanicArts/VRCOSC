// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using VRCOSC.App.Modules;
using VRCOSC.App.Nodes.Types;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.Utils;
using Module = VRCOSC.App.SDK.Modules.Module;

namespace VRCOSC.App.Nodes;

public class ContextMenuNodeTypeItem : IContextMenuEntry
{
    public string Name { get; }
    public Type Type { get; }

    public ContextMenuNodeTypeItem(string name, Type type)
    {
        Name = name;
        Type = type;
    }
}

public class ContextMenuPresetItem : IContextMenuEntry
{
    public string Name { get; }
    public NodePreset Preset { get; }

    public ContextMenuPresetItem(string name, NodePreset preset)
    {
        Name = name;
        Preset = preset;
    }
}

public class ContextMenuSubMenu : IContextMenuEntry
{
    public string Name { get; }
    public ObservableCollection<IContextMenuEntry> Items { get; } = [];

    public ContextMenuSubMenu(string name)
    {
        Name = name;
    }
}

public class ContextMenuRoot
{
    public ObservableCollection<IContextMenuEntry> Items { get; } = [];
}

public interface IContextMenuEntry
{
    public string Name { get; }
}

public static class ContextMenuBuilder
{
    public static ContextMenuSubMenu BuildSpawnPresetMenu()
    {
        var root = new ContextMenuSubMenu("Spawn Preset");

        foreach (var nodePreset in NodeManager.GetInstance().Presets)
        {
            root.Items.Add(new ContextMenuPresetItem(nodePreset.Name.Value, nodePreset));
        }

        return root;
    }

    public static ContextMenuSubMenu BuildCreateNodesMenu()
    {
        var root = new ContextMenuSubMenu("Create Node");
        var nodeTypes = getAllNodeTypes().ToList();
        var executingAsm = Assembly.GetExecutingAssembly();

        foreach (var type in nodeTypes.Where(t => t.Assembly == executingAsm))
        {
            var attr = type.GetCustomAttribute<NodeAttribute>();

            if (attr == null || string.IsNullOrWhiteSpace(attr.Path))
                continue;

            var currentList = root.Items;
            var pathParts = attr.Path.Split('/');

            foreach (var part in pathParts)
            {
                var list = currentList;

                var submenu = currentList
                              .OfType<ContextMenuSubMenu>()
                              .FirstOrDefault(sm => sm.Name == part)
                              ?? new ContextMenuSubMenu(part).Also(sm => list.Add(sm));

                currentList = submenu.Items;
            }

            addNodeEntry(currentList, type, attr);
        }

        sortMenu(root.Items);

        var modulesMenu = new ContextMenuSubMenu("Modules");
        var external = nodeTypes.Where(t => t.Assembly != executingAsm);

        var byModule = external
                       .Select(nodeType => new
                       {
                           NodeType = nodeType,
                           ModuleType = nodeType.GetConstructedGenericBase(typeof(ModuleNode<>))?.GenericTypeArguments[0]
                       })
                       .Where(x =>
                           x.ModuleType != null
                           && typeof(Module).IsAssignableFrom(x.ModuleType)
                           && !x.ModuleType!.IsAbstract
                       )
                       .GroupBy(
                           x => ModuleManager
                                .GetInstance()
                                .GetModuleInstanceFromType(x.ModuleType!)
                                .Title,
                           x => x.NodeType
                       );

        foreach (var moduleGroup in byModule)
        {
            var moduleTitle = moduleGroup.Key;
            var moduleMenu = new ContextMenuSubMenu(moduleTitle);
            modulesMenu.Items.Add(moduleMenu);

            foreach (var nodeType in moduleGroup)
            {
                var attr = nodeType.GetCustomAttribute<NodeAttribute>()!;
                addNodeEntry(moduleMenu.Items, nodeType, attr);
            }

            sortMenu(moduleMenu.Items);
        }

        if (modulesMenu.Items.Count > 0)
            root.Items.Insert(0, modulesMenu);

        return root;
    }

    private static void addNodeEntry(
        ObservableCollection<IContextMenuEntry> list,
        Type nodeType,
        NodeAttribute attr)
    {
        var genAttr = nodeType.GetCustomAttribute<NodeGenericTypeFilterAttribute>();

        if (genAttr is { Types.Length: > 0 })
        {
            var genMenu = new ContextMenuSubMenu(attr.Title);

            foreach (var gt in genAttr.Types)
            {
                var constructed = nodeType.MakeGenericType(gt);
                var friendly = $"{attr.Title} ({gt.GetFriendlyName()})";
                genMenu.Items.Add(new ContextMenuNodeTypeItem(friendly, constructed));
            }

            list.Add(genMenu);
        }
        else
        {
            list.Add(new ContextMenuNodeTypeItem(attr.Title, nodeType));
        }
    }

    private static IEnumerable<Type> getAllNodeTypes()
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;

            try
            {
                types = asm.GetExportedTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray()!;
            }

            foreach (var t in types)
            {
                if (t.IsAbstract) continue;
                if (!typeof(Node).IsAssignableFrom(t)) continue;
                if (t.GetCustomAttribute<NodeAttribute>() is null) continue;

                yield return t;
            }
        }
    }

    private static void sortMenu(ObservableCollection<IContextMenuEntry> entries)
    {
        var sorted = entries
                     .OrderBy(e => e is not ContextMenuSubMenu)
                     .ThenBy(e => e.Name)
                     .ToList();

        entries.Clear();
        entries.AddRange(sorted);

        foreach (var sm in entries.OfType<ContextMenuSubMenu>())
            sortMenu(sm.Items);
    }
}

// Extension to let you fluently add to a list when you create an object
public static class FluentExtensions
{
    public static T Also<T>(this T obj, Action<T> action)
    {
        action(obj);
        return obj;
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
        else if (entry is ContextMenuNodeTypeItem item)
        {
            Console.WriteLine($"{indentStr}I {item.Name}");
        }
    }
}