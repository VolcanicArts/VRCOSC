// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.Nodes;

public interface IConnection
{
    Guid Id { get; }
    Guid OutputId { get; }
    int OutputSlot { get; }
    int OutputSlotIndex { get; }
    Guid InputId { get; }
    int InputSlot { get; }
    int InputSlotIndex { get; }
}

public interface IFlowConnection : IConnection;

public interface IValueConnection : IConnection
{
    public Type OutputType { get; }
    public Type InputType { get; }
}

public abstract record Connection : IConnection
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid OutputId { get; }
    public int OutputSlot { get; }
    public int OutputSlotIndex { get; }
    public Guid InputId { get; }
    public int InputSlot { get; set; }
    public int InputSlotIndex { get; }

    protected Connection(Guid outputId, int outputSlot, int outputSlotIndex, Guid inputId, int inputSlot, int inputSlotIndex)
    {
        OutputId = outputId;
        OutputSlot = outputSlot;
        OutputSlotIndex = outputSlotIndex;
        InputId = inputId;
        InputSlot = inputSlot;
        InputSlotIndex = inputSlotIndex;
    }

    public override int GetHashCode() => Id.GetHashCode();
}

public record FlowConnection : Connection, IFlowConnection
{
    public FlowConnection(Guid outputId, int outputSlot, int outputSlotIndex, Guid inputId, int inputSlot, int inputSlotIndex)
        : base(outputId, outputSlot, outputSlotIndex, inputId, inputSlot, inputSlotIndex)
    {
    }
}

public record ValueConnection : Connection, IValueConnection
{
    public Type OutputType { get; }
    public Type InputType { get; }

    public ValueConnection(Guid outputId, int outputSlot, int outputSlotIndex, Type outputType, Guid inputId, int inputSlot, int inputSlotIndex, Type inputType)
        : base(outputId, outputSlot, outputSlotIndex, inputId, inputSlot, inputSlotIndex)
    {
        OutputType = outputType;
        InputType = inputType;
    }
}