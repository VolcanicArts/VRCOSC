// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Collections;

[Node("Enumerable Count", "Collections")]
public class EnumerableCountNode<T> : Node
{
    public ValueInput<IEnumerable<T>> Enumerable = new();
    public ValueOutput<int> Count = new();

    protected override Task Process(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);
        if (enumerable is null) return Task.CompletedTask;

        Count.Write(enumerable.Count(), c);
        return Task.CompletedTask;
    }
}

[Node("Enumerable Element At", "Collections")]
public sealed class EnumerableElementAtNode<T> : Node
{
    public ValueInput<IEnumerable<T>> Enumerable = new();
    public ValueInput<int> Index = new();
    public ValueOutput<T> Element = new();

    protected override Task Process(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);
        if (enumerable is null) return Task.CompletedTask;

        var index = Index.Read(c);
        if (index < 0 || index >= enumerable.Count()) return Task.CompletedTask;

        Element.Write(enumerable.ElementAt(index), c);
        return Task.CompletedTask;
    }
}

[Node("Enumerable Insert Element", "Collections")]
public class EnumerableElementInsertNode<T> : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<IEnumerable<T>> Enumerable = new();
    public ValueInput<int> Index = new();
    public ValueInput<T> Element = new();
    public ValueOutput<IEnumerable<T>> Result = new();

    protected override Task Process(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);
        if (enumerable is null) return Task.CompletedTask;

        var index = Index.Read(c);
        var element = Element.Read(c);

        var list = enumerable.ToList();
        list.Insert(index, element);
        Result.Write(list, c);

        Next.Execute(c);
        return Task.CompletedTask;
    }
}

[Node("Enumerable Add Element", "Collections")]
public sealed class EnumerableElementAddNode<T> : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<IEnumerable<T>> Enumerable = new();
    public ValueInput<T> Element = new();
    public ValueOutput<IEnumerable<T>> Result = new();

    protected override Task Process(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);
        if (enumerable is null) return Task.CompletedTask;

        var element = Element.Read(c);

        var list = enumerable.ToList();
        list.Add(element);
        Result.Write(list, c);

        Next.Execute(c);
        return Task.CompletedTask;
    }
}

[Node("Enumerable Remove Element", "Collections")]
public sealed class EnumerableElementRemoveNode<T> : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<IEnumerable<T>> Enumerable = new();
    public ValueInput<T> Element = new();
    public ValueOutput<IEnumerable<T>> Result = new();

    protected override Task Process(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);
        if (enumerable is null) return Task.CompletedTask;

        var element = Element.Read(c);

        var list = enumerable.ToList();
        list.Remove(element);
        Result.Write(list, c);

        Next.Execute(c);
        return Task.CompletedTask;
    }
}

[Node("Enumerable Remove Index", "Collections")]
public sealed class EnumerableIndexRemoveNode<T> : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<IEnumerable<T>> Enumerable = new();
    public ValueInput<int> Index = new();
    public ValueOutput<IEnumerable<T>> Result = new();

    protected override Task Process(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);
        if (enumerable is null) return Task.CompletedTask;

        var index = Index.Read(c);

        var list = enumerable.ToList();
        list.RemoveAt(index);
        Result.Write(list, c);

        Next.Execute(c);
        return Task.CompletedTask;
    }
}