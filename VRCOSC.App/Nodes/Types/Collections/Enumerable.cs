// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;

namespace VRCOSC.App.Nodes.Types.Collections;

[Node("Enumerable Count", "Collections/Enumerable")]
public sealed class EnumerableCountNode<T>() : ValueComputeNode<int>("Count")
{
    public ValueInput<IEnumerable<T>?> Enumerable = new();

    protected override int ComputeValue(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);
        return enumerable?.Count() ?? 0;
    }
}

[Node("Enumerable Contains", "Collections/Enumerable")]
public sealed class EnumerableContainsNode<T>() : ValueComputeNode<bool>("Contains")
{
    public ValueInput<IEnumerable<T>?> Enumerable = new();
    public ValueInput<T> Item = new();

    protected override bool ComputeValue(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);
        return enumerable?.Contains(Item.Read(c)) ?? false;
    }
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
public sealed class EnumerableAnyNode<T>() : ValueComputeNode<bool>("Any")
{
    public ValueInput<IEnumerable<T>?> Enumerable = new();

    protected override bool ComputeValue(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);
        return enumerable?.Any() ?? false;
    }
}

[Node("Enumerable Is Empty", "Collections/Enumerable")]
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
public sealed class EnumerableReverseNode<T>() : ValueComputeNode<IEnumerable<T>?>("Reversed")
{
    public ValueInput<IEnumerable<T>?> Enumerable = new();

    protected override IEnumerable<T>? ComputeValue(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);
        return enumerable?.Reverse();
    }
}

[Node("Enumerable Distinct", "Collections/Enumerable")]
public sealed class EnumerableDistinctNode<T>() : ValueComputeNode<IEnumerable<T>?>("Distinct")
{
    public ValueInput<IEnumerable<T>?> Enumerable = new();

    protected override IEnumerable<T>? ComputeValue(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);
        return enumerable?.Distinct();
    }
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

    protected override IEnumerable<T>? ComputeValue(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);
        return enumerable?.Skip(Count.Read(c));
    }
}

[Node("Enumerable Take", "Collections/Enumerable")]
public sealed class EnumerableTakeNode<T> : ValueComputeNode<IEnumerable<T>?>
{
    public ValueInput<IEnumerable<T>?> Enumerable = new();
    public ValueInput<int> Count = new();

    protected override IEnumerable<T>? ComputeValue(PulseContext c)
    {
        var enumerable = Enumerable.Read(c);
        return enumerable?.Take(Count.Read(c));
    }
}

[Node("Enumerable Insert Element", "Collections/Enumerable/Modifiers")]
public sealed class EnumerableElementInsertNode<T> : TryValueComputeNode<IEnumerable<T>?>
{
    public ValueInput<IEnumerable<T>?> Enumerable = new();
    public ValueInput<int> Index = new();
    public ValueInput<T> Element = new();

    protected override bool TryComputeValue(out IEnumerable<T>? value, PulseContext c)
    {
        var enumerable = Enumerable.Read(c);

        if (enumerable is null)
        {
            value = null;
            return false;
        }

        var index = Index.Read(c);
        var element = Element.Read(c);

        if (element is null)
        {
            value = null;
            return false;
        }

        var list = enumerable.ToList();

        try
        {
            list.Insert(index, element);
        }
        catch
        {
            value = null;
            return false;
        }

        value = list;
        return true;
    }
}

[Node("Enumerable Add Element", "Collections/Enumerable/Modifiers")]
public sealed class EnumerableElementAddNode<T> : TryValueComputeNode<IEnumerable<T>?>
{
    public ValueInput<IEnumerable<T>?> Enumerable = new();
    public ValueInput<T> Element = new();

    protected override bool TryComputeValue(out IEnumerable<T>? value, PulseContext c)
    {
        var enumerable = Enumerable.Read(c);

        if (enumerable is null)
        {
            value = null;
            return false;
        }

        var element = Element.Read(c);

        var list = enumerable.ToList();
        list.Add(element);

        value = list;
        return true;
    }
}

[Node("Enumerable Remove Element", "Collections/Enumerable/Modifiers")]
public sealed class EnumerableElementRemoveNode<T> : TryValueComputeNode<IEnumerable<T>?>
{
    public ValueInput<IEnumerable<T>?> Enumerable = new();
    public ValueInput<T> Element = new();

    protected override bool TryComputeValue(out IEnumerable<T>? value, PulseContext c)
    {
        var enumerable = Enumerable.Read(c);

        if (enumerable is null)
        {
            value = null;
            return false;
        }

        var element = Element.Read(c);

        var list = enumerable.ToList();
        var result = list.Remove(element);

        value = result ? list : enumerable;
        return result;
    }
}

[Node("Enumerable Remove Index", "Collections/Enumerable/Modifiers")]
public sealed class EnumerableIndexRemoveNode<T> : TryValueComputeNode<IEnumerable<T>?>
{
    public ValueInput<IEnumerable<T>?> Enumerable = new();
    public ValueInput<int> Index = new();

    protected override bool TryComputeValue(out IEnumerable<T>? value, PulseContext c)
    {
        var enumerable = Enumerable.Read(c);

        if (enumerable is null)
        {
            value = null;
            return false;
        }

        var index = Index.Read(c);

        var list = enumerable.ToList();

        try
        {
            list.RemoveAt(index);
        }
        catch
        {
            value = null;
            return false;
        }

        value = list;
        return true;
    }
}