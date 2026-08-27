// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using System.Threading.Tasks;
using VRCOSC.App.Modules;
using VRCOSC.App.Nodes.Metadata;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types;

public interface IGraphElement
{
    public Guid Id { get; }
}

public abstract class GraphElement : IGraphElement, IEquatable<GraphElement>
{
    public Guid Id { get; internal set; } = Guid.NewGuid();

    public bool Equals(GraphElement? other)
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

        return Equals((GraphElement)obj);
    }

    public override int GetHashCode() => Id.GetHashCode();
}

public interface INode : IGraphElement
{
    INodeMetadata Metadata { get; }
    string DisplayName { get; }

    internal Task IProcess(IPulseContext c);
    internal bool IShouldProcess(IPulseContext c);
}

public abstract class Node : GraphElement, INode
{
    public INodeMetadata Metadata => field ??= NodeMetadataManager.GetFor(this).Value;
    public virtual string DisplayName => Metadata.Shared.Name;
    internal NodeGraph ContainingGraph { get; private set; } = null!;

    internal void Init(NodeGraph containingGraph)
    {
        ContainingGraph = containingGraph;

        var fields = GetType().GetFieldsByType(typeof(INodeElement));

        foreach (var field in fields)
        {
            var instance = (INodeElement)field.GetValue(this)!;
            instance.Owner = this;
        }
    }

    public async Task IProcess(IPulseContext c)
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
            Console.WriteLine(e.StackTrace);
        }
    }

    public bool IShouldProcess(IPulseContext c) => ShouldProcess(c);

    /// <summary>
    /// Processes this node from inputs to outputs
    /// </summary>
    /// <param name="c">The context a flow is running in</param>
    protected abstract Task Process(IPulseContext c);

    /// <summary>
    /// Whether this <see cref="Node"/> should process or not
    /// </summary>
    /// <param name="c">The context a flow is running in</param>
    /// <returns>True if this node should process. False otherwise</returns>
    /// <remarks>In the case this this <see cref="Node"/> is a trigger node, returning false means an existing flow isn't cancelled</remarks>
    protected virtual bool ShouldProcess(IPulseContext c)
    {
        var moduleType = GetType().GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IModuleNode<>)).Select(i => i.GetGenericArguments()[0]).SingleOrDefault();
        if (moduleType is null) return true;

        var moduleInstance = ModuleManager.GetInstance().GetModuleInstanceFromType(moduleType);
        return ModuleManager.GetInstance().IsModuleRunning(moduleInstance.FullID);
    }
}