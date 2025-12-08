// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Operators;

[Node("Flag Create", "Operators/Enum")]
public sealed class EnumFlagCreateNode<T> : Node where T : struct, Enum
{
    public ValueInputList<T> Flags = new();
    public ValueOutput<T> Result = new();

    protected override Task Process(PulseContext c)
    {
        var flags = Flags.Read(c);
        var result = flags.Aggregate(0ul, (current, flag) => current | Convert.ToUInt64(flag));
        Result.Write((T)Enum.ToObject(typeof(T), result), c);
        return Task.CompletedTask;
    }
}

[Node("Has Flag", "Operators/Enum")]
public sealed class EnumHasFlagNode<T> : Node where T : struct, Enum
{
    public ValueInput<T> Flags = new();
    public ValueInput<T> Flag = new();
    public ValueOutput<bool> HasFlag = new("Has Flag");

    protected override Task Process(PulseContext c)
    {
        var flags = Convert.ToUInt64(Flags.Read(c));
        var flag = Convert.ToUInt64(Flag.Read(c));
        HasFlag.Write((flags & flag) == flag, c);
        return Task.CompletedTask;
    }
}

[Node("Flag Add", "Operators/Enum")]
public sealed class EnumFlagAddNode<T> : Node, IFlowInput where T : struct, Enum
{
    public FlowContinuation Next = new("Next");

    public ValueInput<T> Flags = new();
    public ValueInput<T> NewFlag = new("New Flag");

    public ValueOutput<T> CreatedFlag = new("Created");

    protected override async Task Process(PulseContext c)
    {
        var flags = Convert.ToUInt64(Flags.Read(c));
        var newFlag = Convert.ToUInt64(NewFlag.Read(c));
        var createdFlag = (T)Enum.ToObject(typeof(T), flags | newFlag);
        CreatedFlag.Write(createdFlag, c);

        await Next.Execute(c);
    }
}

[Node("Flag Remove", "Operators/Enum")]
public sealed class EnumFlagRemoveNode<T> : Node, IFlowInput where T : struct, Enum
{
    public FlowContinuation Next = new("Next");

    public ValueInput<T> Flags = new();
    public ValueInput<T> OldFlag = new("Old Flag");

    public ValueOutput<T> CreatedFlag = new("Created");

    protected override async Task Process(PulseContext c)
    {
        var flags = Convert.ToUInt64(Flags.Read(c));
        var oldFlag = Convert.ToUInt64(OldFlag.Read(c));
        var createdFlag = (T)Enum.ToObject(typeof(T), flags & ~oldFlag);
        CreatedFlag.Write(createdFlag, c);

        await Next.Execute(c);
    }
}

[Node("Flag Toggle", "Operators/Enum")]
public sealed class EnumFlagToggleNode<T> : Node, IFlowInput where T : struct, Enum
{
    public FlowContinuation Next = new("Next");

    public ValueInput<T> Flags = new();
    public ValueInput<T> Flag = new();

    public ValueOutput<T> Result = new();

    protected override async Task Process(PulseContext c)
    {
        var flags = Convert.ToUInt64(Flags.Read(c));
        var flag = Convert.ToUInt64(Flag.Read(c));

        var result = flags;

        if ((flags & flag) == flag)
            result &= ~flag;
        else
            result |= flag;

        Result.Write((T)Enum.ToObject(typeof(T), result), c);

        await Next.Execute(c);
    }
}