// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Collections;

[Node("Enumerable Count", "Collections/Enumerable")]
[NodeCollapsed]
public sealed class EnumerableCountNode<T>() : SimpleValueTransformNode<IEnumerable<T>?, int>(e => e?.Count() ?? 0);

[Node("Enumerable Contains", "Collections/Enumerable")]
public sealed class EnumerableContainsNode<T>() : ValueComputeNode<bool>("Contains")
{
    public ValueInput<IEnumerable<T>?> Enumerable = new();
    public ValueInput<T> Item = new();

    protected override bool ComputeValue(PulseContext c) => Enumerable.Read(c)?.Contains(Item.Read(c)) ?? false;
}

[Node("Enumerable Element At", "Collections/Enumerable")]
public sealed class EnumerableElementAtNode<T>() : ValueComputeNode<T>("Element")
{
    public ValueInput<IEnumerable<T>> Enumerable = new();
    public ValueInput<int> Index = new();

    protected override T ComputeValue(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);
        if (enumerable is null) return default!;

        var index = Index.Read(c);
        if (index < 0 || index >= enumerable.Count()) return default!;

        return enumerable.ElementAt(index);
    }
}

[Node("Enumerable First", "Collections/Enumerable")]
[NodeCollapsed]
public sealed class EnumerableFirstNode<T>() : ValueComputeNode<T>("Element")
{
    public ValueInput<IEnumerable<T>?> Enumerable = new();

    protected override T ComputeValue(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);
        return enumerable is null ? default! : enumerable.FirstOrDefault()!;
    }
}

[Node("Enumerable Last", "Collections/Enumerable")]
[NodeCollapsed]
public sealed class EnumerableLastNode<T>() : ValueComputeNode<T>("Element")
{
    public ValueInput<IEnumerable<T>?> Enumerable = new();

    protected override T ComputeValue(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);
        return enumerable is null ? default! : enumerable.LastOrDefault()!;
    }
}

[Node("Enumerable Any", "Collections/Enumerable")]
[NodeCollapsed]
public sealed class EnumerableAnyNode<T>() : ValueComputeNode<bool>("Any")
{
    public ValueInput<IEnumerable<T>?> Enumerable = new();

    protected override bool ComputeValue(PulseContext c) => Enumerable.Read(c)?.Any() ?? false;
}

[Node("Enumerable Is Empty", "Collections/Enumerable")]
[NodeCollapsed]
public sealed class EnumerableIsEmptyNode<T>() : ValueComputeNode<bool>("IsEmpty")
{
    public ValueInput<IEnumerable<T>?> Enumerable = new();

    protected override bool ComputeValue(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);
        return enumerable is null || !enumerable.Any();
    }
}

[Node("Enumerable Reverse", "Collections/Enumerable")]
[NodeCollapsed]
public sealed class EnumerableReverseNode<T>() : ValueComputeNode<IEnumerable<T>?>("Reversed")
{
    public ValueInput<IEnumerable<T>?> Enumerable = new();

    protected override IEnumerable<T>? ComputeValue(PulseContext c) => Enumerable.Read(c)?.Reverse();
}

[Node("Enumerable Distinct", "Collections/Enumerable")]
[NodeCollapsed]
public sealed class EnumerableDistinctNode<T>() : ValueComputeNode<IEnumerable<T>?>("Distinct")
{
    public ValueInput<IEnumerable<T>?> Enumerable = new();

    protected override IEnumerable<T>? ComputeValue(PulseContext c) => Enumerable.Read(c)?.Distinct();
}

[Node("Enumerable Concat", "Collections/Enumerable")]
public sealed class EnumerableConcatNode<T> : ValueComputeNode<IEnumerable<T>>
{
    public ValueInput<IEnumerable<T>?> First = new();
    public ValueInput<IEnumerable<T>?> Second = new();

    protected override IEnumerable<T> ComputeValue(PulseContext c)
    {
        var first = First.Read(c) ?? Enumerable.Empty<T>();
        var second = Second.Read(c) ?? Enumerable.Empty<T>();
        return first.Concat(second);
    }
}

[Node("Enumerable Skip", "Collections/Enumerable")]
public sealed class EnumerableSkipNode<T> : ValueComputeNode<IEnumerable<T>?>
{
    public ValueInput<IEnumerable<T>?> Enumerable = new();
    public ValueInput<int> Count = new();

    protected override IEnumerable<T>? ComputeValue(PulseContext c) => Enumerable.Read(c)?.Skip(Count.Read(c));
}

[Node("Enumerable Take", "Collections/Enumerable")]
public sealed class EnumerableTakeNode<T> : ValueComputeNode<IEnumerable<T>?>
{
    public ValueInput<IEnumerable<T>?> Enumerable = new();
    public ValueInput<int> Count = new();

    protected override IEnumerable<T>? ComputeValue(PulseContext c) => Enumerable.Read(c)?.Take(Count.Read(c));
}

[Node("Enumerable Insert Element", "Collections/Enumerable/Modifiers")]
public sealed class EnumerableElementInsertNode<T> : TryValueComputeNode<IEnumerable<T>>
{
    public ValueInput<IEnumerable<T>?> Enumerable = new();
    public ValueInput<int> Index = new();
    public ValueInput<T> Element = new();

    protected override Result<IEnumerable<T>> TryComputeValue(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);

        if (enumerable is null)
            return Result<IEnumerable<T>>.Fail();

        var index = Index.Read(c);
        var element = Element.Read(c);

        if (element is null)
            return Result<IEnumerable<T>>.Fail();

        var list = enumerable.ToList();

        try
        {
            list.Insert(index, element);
        }
        catch
        {
            return Result<IEnumerable<T>>.Fail();
        }

        return list;
    }
}

[Node("Enumerable Add Element", "Collections/Enumerable/Modifiers")]
public sealed class EnumerableElementAddNode<T> : TryValueComputeNode<IEnumerable<T>>
{
    public ValueInput<IEnumerable<T>?> Enumerable = new();
    public ValueInput<T> Element = new();

    protected override Result<IEnumerable<T>> TryComputeValue(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);

        if (enumerable is null)
            return Result<IEnumerable<T>>.Fail();

        var element = Element.Read(c);

        var list = enumerable.ToList();
        list.Add(element);

        return list;
    }
}

[Node("Enumerable Remove Element", "Collections/Enumerable/Modifiers")]
public sealed class EnumerableElementRemoveNode<T> : TryValueComputeNode<IEnumerable<T>>
{
    public ValueInput<IEnumerable<T>?> Enumerable = new();
    public ValueInput<T> Element = new();

    protected override Result<IEnumerable<T>> TryComputeValue(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);

        if (enumerable is null)
            return Result<IEnumerable<T>>.Fail();

        var element = Element.Read(c);

        var list = enumerable.ToList();
        var result = list.Remove(element);

        return Result<IEnumerable<T>>.Success(result ? list : enumerable);
    }
}

[Node("Enumerable Remove Index", "Collections/Enumerable/Modifiers")]
public sealed class EnumerableIndexRemoveNode<T> : TryValueComputeNode<IEnumerable<T>>
{
    public ValueInput<IEnumerable<T>?> Enumerable = new();
    public ValueInput<int> Index = new();

    protected override Result<IEnumerable<T>> TryComputeValue(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);

        if (enumerable is null)
            return Result<IEnumerable<T>>.Fail();

        var index = Index.Read(c);

        var list = enumerable.ToList();

        try
        {
            list.RemoveAt(index);
        }
        catch
        {
            return Result<IEnumerable<T>>.Fail();
        }

        return list;
    }
}