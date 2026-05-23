// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;

namespace VRCOSC.App.Nodes.Types.Operators;

[Node("Flag Create", "Operators/Enum")]
public sealed class EnumFlagCreateNode<T> : ValueComputeNode<T> where T : struct, Enum
{
    public ValueInputList<T> Flags = new();

    protected override T ComputeValue(IPulseContext c)
    {
        var flags = Flags.Read(c);
        var result = flags.Aggregate(0ul, (current, flag) => current | Convert.ToUInt64(flag));
        return (T)Enum.ToObject(typeof(T), result);
    }
}

[Node("Has Flag", "Operators/Enum")]
public sealed class EnumHasFlagNode<T>() : ValueComputeNode<bool>("Has Flag") where T : struct, Enum
{
    [InputMode(InputModes.Connection)]
    public ValueInput<T> Flags = new();

    public ValueInput<T> Flag = new();

    protected override bool ComputeValue(IPulseContext c)
    {
        var flags = Convert.ToUInt64(Flags.Read(c));
        var flag = Convert.ToUInt64(Flag.Read(c));
        return (flags & flag) == flag;
    }
}

[Node("Flag Add", "Operators/Enum")]
public sealed class EnumFlagAddNode<T>() : ActionValueComputeNode<T>("Flags") where T : struct, Enum
{
    [InputMode(InputModes.Connection)]
    public ValueInput<T> Flags = new();

    public ValueInput<T> NewFlag = new();

    protected override T ComputeValue(IPulseContext c)
    {
        var flags = Convert.ToUInt64(Flags.Read(c));
        var newFlag = Convert.ToUInt64(NewFlag.Read(c));
        return (T)Enum.ToObject(typeof(T), flags | newFlag);
    }
}

[Node("Flag Remove", "Operators/Enum")]
public sealed class EnumFlagRemoveNode<T>() : ActionValueComputeNode<T>("Flags") where T : struct, Enum
{
    [InputMode(InputModes.Connection)]
    public ValueInput<T> Flags = new();

    public ValueInput<T> OldFlag = new();

    protected override T ComputeValue(IPulseContext c)
    {
        var flags = Convert.ToUInt64(Flags.Read(c));
        var oldFlag = Convert.ToUInt64(OldFlag.Read(c));
        return (T)Enum.ToObject(typeof(T), flags & ~oldFlag);
    }
}

[Node("Flag Toggle", "Operators/Enum")]
public sealed class EnumFlagToggleNode<T>() : ActionValueComputeNode<T>("Flags") where T : struct, Enum
{
    [InputMode(InputModes.Connection)]
    public ValueInput<T> Flags = new();

    public ValueInput<T> Flag = new();

    protected override T ComputeValue(IPulseContext c)
    {
        var flags = Convert.ToUInt64(Flags.Read(c));
        var flag = Convert.ToUInt64(Flag.Read(c));

        var result = flags;

        if ((flags & flag) == flag)
            result &= ~flag;
        else
            result |= flag;

        return (T)Enum.ToObject(typeof(T), result);
    }
}