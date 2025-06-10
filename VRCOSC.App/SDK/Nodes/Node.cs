// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VRCOSC.App.Modules;
using VRCOSC.App.Nodes;
using VRCOSC.App.SDK.VRChat;
using VRCOSC.App.Utils;
using Module = VRCOSC.App.SDK.Modules.Module;

namespace VRCOSC.App.SDK.Nodes;

public abstract class Node
{
    internal NodeField NodeField { get; set; } = null!;
    internal NodeVariableSize VariableSize => NodeField.VariableSizes[Id];

    public Guid Id { get; internal set; } = Guid.NewGuid();
    public ObservableVector2 Position { get; internal set; } = new(5000, 5000);
    public Observable<int> ZIndex { get; } = new();

    public NodeMetadata Metadata => NodeField.GetMetadata(this);
    protected Player Player => AppManager.GetInstance().VRChatClient.Player;

    protected Node()
    {
        var type = GetType();
        var allFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

        var defs = new List<Type>
        {
            typeof(IFlow),
            typeof(IValueInput),
            typeof(IValueOutput)
        };

        foreach (var def in defs)
        {
            var group = allFields
                        .Where(f => def.IsGenericTypeDefinition
                            ? f.FieldType.IsGenericType
                              && f.FieldType.GetGenericTypeDefinition().IsAssignableTo(def)
                            : f.FieldType.IsAssignableTo(def))
                        .ToList();

            for (int i = 0; i < group.Count; i++)
            {
                var field = group[i];
                var instance = (INodeAttribute)field.GetValue(this)!;
                instance.Index = i;
            }
        }
    }

    internal void InternalProcess(PulseContext c)
    {
        try
        {
            Process(c);
        }
        catch (TaskCanceledException)
        {
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e);
        }
    }

    protected abstract void Process(PulseContext c);

    internal bool InternalShouldProcess(PulseContext c)
    {
        try
        {
            return ShouldProcess(c);
        }
        catch (TaskCanceledException)
        {
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e);
        }

        return false;
    }

    protected virtual bool ShouldProcess(PulseContext c) => true;
}

public abstract class ModuleNode<T> : Node where T : Module
{
    public T Module => (T)ModuleManager.GetInstance().GetModuleInstanceFromType(typeof(T));
}