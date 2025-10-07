// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types;

public abstract class Node : IEquatable<Node>
{
    internal NodeGraph NodeGraph { get; set; } = null!;
    internal NodeVariableSize VariableSize => NodeGraph.VariableSizes[Id];

    internal Guid Id { get; set; } = Guid.NewGuid();
    internal Point NodePosition { get; set; } = new(5000, 5000);

    public virtual string DisplayName => Metadata.Title;

    public NodeMetadata Metadata => NodeGraph.GetMetadata(this);

    protected Node()
    {
        var type = GetType();
        var allFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

        var attributeGroups = new List<Type>
        {
            typeof(IFlow),
            typeof(IValueInput),
            typeof(IValueOutput)
        };

        foreach (var attributeGroup in attributeGroups)
        {
            var fieldGroup = allFields.Where(f => f.FieldType.IsAssignableTo(attributeGroup)).ToList();

            for (int i = 0; i < fieldGroup.Count; i++)
            {
                var field = fieldGroup[i];
                var instance = (INodeAttribute)field.GetValue(this)!;
                instance.Index = i;
            }
        }
    }

    internal async Task InternalProcess(PulseContext c)
    {
        try
        {
            await Process(c);
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

    protected abstract Task Process(PulseContext c);

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

    public bool Equals(Node? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((Node)obj);
    }

    public override int GetHashCode() => Id.GetHashCode();
}