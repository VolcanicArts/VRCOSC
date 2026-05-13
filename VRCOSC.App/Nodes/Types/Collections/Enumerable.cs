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
public sealed class EnumerableContainsNode<T>() : SimpleResultComputeNode<IEnumerable<T>?, T, bool>((enumerable, item) => enumerable?.Contains(item) ?? false, "Enumerable", "Item", "Contains");

[Node("Enumerable Element At", "Collections/Enumerable")]
public sealed class EnumerableElementAtNode<T>() : ResultComputeNode<IEnumerable<T>?, int, T>("Enumerable", "Index", "Element")
{
    protected override T ComputeResult(IEnumerable<T>? enumerable, int index)
    {
        if (enumerable is null || index < 0 || index >= enumerable.Count()) return default!;

        return enumerable.ElementAt(index);
    }
}

[Node("Enumerable First", "Collections/Enumerable")]
[NodeCollapsed]
public sealed class EnumerableFirstNode<T> : ValueTransformNode<IEnumerable<T>?, T>
{
    protected override T TransformValue(IEnumerable<T>? enumerable, PulseContext c)
        => enumerable is not null ? enumerable.First() : default!;
}

[Node("Enumerable Last", "Collections/Enumerable")]
[NodeCollapsed]
public sealed class EnumerableLastNode<T> : ValueTransformNode<IEnumerable<T>?, T>
{
    protected override T TransformValue(IEnumerable<T>? enumerable, PulseContext c)
        => enumerable is not null ? enumerable.Last() : default!;
}

[Node("Enumerable Any", "Collections/Enumerable")]
[NodeCollapsed]
public sealed class EnumerableAnyNode<T>() : SimpleValueTransformNode<IEnumerable<T>?, bool>(enumerable => enumerable?.Any() ?? false);

[Node("Enumerable Is Empty", "Collections/Enumerable")]
[NodeCollapsed]
public sealed class EnumerableIsEmptyNode<T>() : SimpleValueTransformNode<IEnumerable<T>?, bool>(enumerable => !enumerable?.Any() ?? false);

[Node("Enumerable Reverse", "Collections/Enumerable")]
[NodeCollapsed]
public sealed class EnumerableReverseNode<T>() : SimpleValueTransformNode<IEnumerable<T>?>(enumerable => enumerable?.Reverse() ?? null);

[Node("Enumerable Distinct", "Collections/Enumerable")]
[NodeCollapsed]
public sealed class EnumerableDistinctNode<T>() : SimpleValueTransformNode<IEnumerable<T>?>(enumerable => enumerable?.Distinct() ?? null);

[Node("Enumerable Concat", "Collections/Enumerable")]
public sealed class EnumerableConcatNode<T>() : ResultComputeNode<IEnumerable<T>?>("First", "Second", "Enumerable")
{
    protected override IEnumerable<T>? ComputeResult(IEnumerable<T>? first, IEnumerable<T>? second)
    {
        first ??= Enumerable.Empty<T>();
        second ??= Enumerable.Empty<T>();
        return first.Concat(second);
    }
}

[Node("Enumerable Skip", "Collections/Enumerable")]
public sealed class EnumerableSkipNode<T>() : SimpleResultComputeNode<IEnumerable<T>?, int, IEnumerable<T>?>((enumerable, count) => enumerable?.Skip(count) ?? null, "Enumerable", "Count", "Enumerable");

[Node("Enumerable Take", "Collections/Enumerable")]
public sealed class EnumerableTakeNode<T>() : SimpleResultComputeNode<IEnumerable<T>?, int, IEnumerable<T>?>((enumerable, count) => enumerable?.Take(count) ?? null, "Enumerable", "Count", "Enumerable");

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
public sealed class EnumerableElementAddNode<T>() : TryResultComputeNode<IEnumerable<T>?, T, IEnumerable<T>?>("Enumerable", "Element", "Enumerable")
{
    protected override Result<IEnumerable<T>?> TryComputeResult(IEnumerable<T>? enumerable, T element, PulseContext c)
    {
        if (enumerable is null)
            return Result<IEnumerable<T>?>.Fail();

        var list = enumerable.ToList();
        list.Add(element);
        return list;
    }
}

[Node("Enumerable Remove Element", "Collections/Enumerable/Modifiers")]
public sealed class EnumerableElementRemoveNode<T>() : TryResultComputeNode<IEnumerable<T>?, T, IEnumerable<T>?>("Enumerable", "Element", "Enumerable")
{
    protected override Result<IEnumerable<T>?> TryComputeResult(IEnumerable<T>? enumerable, T element, PulseContext c)
    {
        if (enumerable is null)
            return Result<IEnumerable<T>?>.Fail();

        var list = enumerable.ToList();
        var result = list.Remove(element);

        return Result<IEnumerable<T>?>.Success(result ? list : enumerable);
    }
}

[Node("Enumerable Remove Index", "Collections/Enumerable/Modifiers")]
public sealed class EnumerableIndexRemoveNode<T>() : TryResultComputeNode<IEnumerable<T>?, int, IEnumerable<T>?>("Enumerable", "Index", "Enumerable")
{
    protected override Result<IEnumerable<T>?> TryComputeResult(IEnumerable<T>? enumerable, int index, PulseContext c)
    {
        if (enumerable is null)
            return Result<IEnumerable<T>?>.Fail();

        var list = enumerable.ToList();

        try
        {
            list.RemoveAt(index);
        }
        catch
        {
            return Result<IEnumerable<T>?>.Fail();
        }

        return list;
    }
}