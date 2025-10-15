// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using VRCOSC.App.Nodes.Types;

namespace VRCOSC.App.Nodes;

/// <summary>
/// A passively updating node that can only read and write from stores in <see cref="OnUpdate"/>
/// </summary>
public interface IUpdateNode
{
    public void OnUpdate(PulseContext c);
}

/// <summary>
/// An actively updating node that can read inputs/stores and write outputs/stores in <see cref="OnUpdate"/>.
/// If <see cref="OnUpdate"/> returns true it will process and notify nodes down flow of the <see cref="ValueOutput{T}"/> changes, otherwise it will not process
/// </summary>
public interface IActiveUpdateNode
{
    public bool OnUpdate(PulseContext c);
}

public abstract class UpdateNode<T> : Node, IActiveUpdateNode
{
    private readonly GlobalStore<T> prevValue = new();

    public bool OnUpdate(PulseContext c)
    {
        var value = GetValue(c);

        if (EqualityComparer<T>.Default.Equals(value, prevValue.Read(c))) return false;

        prevValue.Write(value, c);
        return true;
    }

    protected abstract T GetValue(PulseContext c);
}

public abstract class UpdateNode<T1, T2> : Node, IActiveUpdateNode
{
    private readonly GlobalStore<T1> prevValue1 = new();
    private readonly GlobalStore<T2> prevValue2 = new();

    public bool OnUpdate(PulseContext c)
    {
        var value = GetValues(c);

        if (EqualityComparer<T1>.Default.Equals(value.Item1, prevValue1.Read(c))
            && EqualityComparer<T2>.Default.Equals(value.Item2, prevValue2.Read(c)))
        {
            return false;
        }

        prevValue1.Write(value.Item1, c);
        prevValue2.Write(value.Item2, c);
        return true;
    }

    protected abstract (T1, T2) GetValues(PulseContext c);
}

public abstract class UpdateNode<T1, T2, T3> : Node, IActiveUpdateNode
{
    private readonly GlobalStore<T1> prevValue1 = new();
    private readonly GlobalStore<T2> prevValue2 = new();
    private readonly GlobalStore<T3> prevValue3 = new();

    public bool OnUpdate(PulseContext c)
    {
        var value = GetValues(c);

        if (EqualityComparer<T1>.Default.Equals(value.Item1, prevValue1.Read(c))
            && EqualityComparer<T2>.Default.Equals(value.Item2, prevValue2.Read(c))
            && EqualityComparer<T3>.Default.Equals(value.Item3, prevValue3.Read(c)))
        {
            return false;
        }

        prevValue1.Write(value.Item1, c);
        prevValue2.Write(value.Item2, c);
        prevValue3.Write(value.Item3, c);
        return true;
    }

    protected abstract (T1, T2, T3) GetValues(PulseContext c);
}

public abstract class UpdateNode<T1, T2, T3, T4> : Node, IActiveUpdateNode
{
    private readonly GlobalStore<T1> prevValue1 = new();
    private readonly GlobalStore<T2> prevValue2 = new();
    private readonly GlobalStore<T3> prevValue3 = new();
    private readonly GlobalStore<T4> prevValue4 = new();

    public bool OnUpdate(PulseContext c)
    {
        var value = GetValues(c);

        if (EqualityComparer<T1>.Default.Equals(value.Item1, prevValue1.Read(c))
            && EqualityComparer<T2>.Default.Equals(value.Item2, prevValue2.Read(c))
            && EqualityComparer<T3>.Default.Equals(value.Item3, prevValue3.Read(c))
            && EqualityComparer<T4>.Default.Equals(value.Item4, prevValue4.Read(c)))
        {
            return false;
        }

        prevValue1.Write(value.Item1, c);
        prevValue2.Write(value.Item2, c);
        prevValue3.Write(value.Item3, c);
        prevValue4.Write(value.Item4, c);
        return true;
    }

    protected abstract (T1, T2, T3, T4) GetValues(PulseContext c);
}

public abstract class UpdateNode<T1, T2, T3, T4, T5> : Node, IActiveUpdateNode
{
    private readonly GlobalStore<T1> prevValue1 = new();
    private readonly GlobalStore<T2> prevValue2 = new();
    private readonly GlobalStore<T3> prevValue3 = new();
    private readonly GlobalStore<T4> prevValue4 = new();
    private readonly GlobalStore<T5> prevValue5 = new();

    public bool OnUpdate(PulseContext c)
    {
        var value = GetValues(c);

        if (EqualityComparer<T1>.Default.Equals(value.Item1, prevValue1.Read(c))
            && EqualityComparer<T2>.Default.Equals(value.Item2, prevValue2.Read(c))
            && EqualityComparer<T3>.Default.Equals(value.Item3, prevValue3.Read(c))
            && EqualityComparer<T4>.Default.Equals(value.Item4, prevValue4.Read(c))
            && EqualityComparer<T5>.Default.Equals(value.Item5, prevValue5.Read(c)))
        {
            return false;
        }

        prevValue1.Write(value.Item1, c);
        prevValue2.Write(value.Item2, c);
        prevValue3.Write(value.Item3, c);
        prevValue4.Write(value.Item4, c);
        prevValue5.Write(value.Item5, c);
        return true;
    }

    protected abstract (T1, T2, T3, T4, T5) GetValues(PulseContext c);
}

public abstract class UpdateNode<T1, T2, T3, T4, T5, T6> : Node, IActiveUpdateNode
{
    private readonly GlobalStore<T1> prevValue1 = new();
    private readonly GlobalStore<T2> prevValue2 = new();
    private readonly GlobalStore<T3> prevValue3 = new();
    private readonly GlobalStore<T4> prevValue4 = new();
    private readonly GlobalStore<T5> prevValue5 = new();
    private readonly GlobalStore<T6> prevValue6 = new();

    public bool OnUpdate(PulseContext c)
    {
        var value = GetValues(c);

        if (EqualityComparer<T1>.Default.Equals(value.Item1, prevValue1.Read(c))
            && EqualityComparer<T2>.Default.Equals(value.Item2, prevValue2.Read(c))
            && EqualityComparer<T3>.Default.Equals(value.Item3, prevValue3.Read(c))
            && EqualityComparer<T4>.Default.Equals(value.Item4, prevValue4.Read(c))
            && EqualityComparer<T5>.Default.Equals(value.Item5, prevValue5.Read(c))
            && EqualityComparer<T6>.Default.Equals(value.Item6, prevValue6.Read(c)))
        {
            return false;
        }

        prevValue1.Write(value.Item1, c);
        prevValue2.Write(value.Item2, c);
        prevValue3.Write(value.Item3, c);
        prevValue4.Write(value.Item4, c);
        prevValue5.Write(value.Item5, c);
        prevValue6.Write(value.Item6, c);
        return true;
    }

    protected abstract (T1, T2, T3, T4, T5, T6) GetValues(PulseContext c);
}

public abstract class UpdateNode<T1, T2, T3, T4, T5, T6, T7> : Node, IActiveUpdateNode
{
    private readonly GlobalStore<T1> prevValue1 = new();
    private readonly GlobalStore<T2> prevValue2 = new();
    private readonly GlobalStore<T3> prevValue3 = new();
    private readonly GlobalStore<T4> prevValue4 = new();
    private readonly GlobalStore<T5> prevValue5 = new();
    private readonly GlobalStore<T6> prevValue6 = new();
    private readonly GlobalStore<T7> prevValue7 = new();

    public bool OnUpdate(PulseContext c)
    {
        var value = GetValues(c);

        if (EqualityComparer<T1>.Default.Equals(value.Item1, prevValue1.Read(c))
            && EqualityComparer<T2>.Default.Equals(value.Item2, prevValue2.Read(c))
            && EqualityComparer<T3>.Default.Equals(value.Item3, prevValue3.Read(c))
            && EqualityComparer<T4>.Default.Equals(value.Item4, prevValue4.Read(c))
            && EqualityComparer<T5>.Default.Equals(value.Item5, prevValue5.Read(c))
            && EqualityComparer<T6>.Default.Equals(value.Item6, prevValue6.Read(c))
            && EqualityComparer<T7>.Default.Equals(value.Item7, prevValue7.Read(c)))
        {
            return false;
        }

        prevValue1.Write(value.Item1, c);
        prevValue2.Write(value.Item2, c);
        prevValue3.Write(value.Item3, c);
        prevValue4.Write(value.Item4, c);
        prevValue5.Write(value.Item5, c);
        prevValue6.Write(value.Item6, c);
        prevValue7.Write(value.Item7, c);
        return true;
    }

    protected abstract (T1, T2, T3, T4, T5, T6, T7) GetValues(PulseContext c);
}

public abstract class UpdateNode<T1, T2, T3, T4, T5, T6, T7, T8> : Node, IActiveUpdateNode
{
    private readonly GlobalStore<T1> prevValue1 = new();
    private readonly GlobalStore<T2> prevValue2 = new();
    private readonly GlobalStore<T3> prevValue3 = new();
    private readonly GlobalStore<T4> prevValue4 = new();
    private readonly GlobalStore<T5> prevValue5 = new();
    private readonly GlobalStore<T6> prevValue6 = new();
    private readonly GlobalStore<T7> prevValue7 = new();
    private readonly GlobalStore<T8> prevValue8 = new();

    public bool OnUpdate(PulseContext c)
    {
        var value = GetValues(c);

        if (EqualityComparer<T1>.Default.Equals(value.Item1, prevValue1.Read(c))
            && EqualityComparer<T2>.Default.Equals(value.Item2, prevValue2.Read(c))
            && EqualityComparer<T3>.Default.Equals(value.Item3, prevValue3.Read(c))
            && EqualityComparer<T4>.Default.Equals(value.Item4, prevValue4.Read(c))
            && EqualityComparer<T5>.Default.Equals(value.Item5, prevValue5.Read(c))
            && EqualityComparer<T6>.Default.Equals(value.Item6, prevValue6.Read(c))
            && EqualityComparer<T7>.Default.Equals(value.Item7, prevValue7.Read(c))
            && EqualityComparer<T8>.Default.Equals(value.Item8, prevValue8.Read(c)))
        {
            return false;
        }

        prevValue1.Write(value.Item1, c);
        prevValue2.Write(value.Item2, c);
        prevValue3.Write(value.Item3, c);
        prevValue4.Write(value.Item4, c);
        prevValue5.Write(value.Item5, c);
        prevValue6.Write(value.Item6, c);
        prevValue7.Write(value.Item7, c);
        prevValue8.Write(value.Item8, c);
        return true;
    }

    protected abstract (T1, T2, T3, T4, T5, T6, T7, T8) GetValues(PulseContext c);
}