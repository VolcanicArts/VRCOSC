// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Nodes;


[Flags]
public enum NodeFlowFlag
{
    Loop = 1 << 0
}

public sealed record NodeFlowRef(string Name, ConnectionSide Side, NodeFlowFlag Flags);

public abstract class Node
{
    public NodeScape NodeScape { get; set; } = null!;

    public Guid Id { get; } = Guid.NewGuid();
    public ObservableVector2 Position { get; } = new(5000, 5000);
    public int ZIndex { get; set; }
    internal List<NodeFlowRef> InputFlows { get; } = [];
    internal List<NodeFlowRef> OutputFlows { get; } = [];
    internal int NextFlowSlot = -1;

    public string Title { get; }

    protected Node()
    {
        Title = GetType().GetCustomAttribute<NodeAttribute>()!.Title;
    }

    protected NodeFlowRef AddFlow(string name, ConnectionSide side, NodeFlowFlag flags = 0)
    {
        var flowRef = new NodeFlowRef(name, side, flags);
        if (side == ConnectionSide.Input) InputFlows.Add(flowRef);
        if (side == ConnectionSide.Output) OutputFlows.Add(flowRef);
        return flowRef;
    }

    protected void SetFlow(NodeFlowRef flowRef) => NextFlowSlot = OutputFlows.IndexOf(flowRef);

    protected NodeFlowRef GetFlowAt(int position, ConnectionSide side) => side == ConnectionSide.Input ? InputFlows[position] : OutputFlows[position];

    internal bool IsFlowNode(ConnectionSide side)
    {
        return (side.HasFlag(ConnectionSide.Input) && InputFlows.Count > 0) || (side.HasFlag(ConnectionSide.Output) && OutputFlows.Count > 0);
    }

    internal bool IsValueNode(ConnectionSide side)
    {
        return (side.HasFlag(ConnectionSide.Input) && NodeScape.GetMetadata(this).Process.InputCount > 0)
               || (side.HasFlag(ConnectionSide.Output) && NodeScape.GetMetadata(this).Process.OutputCount > 0);
    }
}

// constant values
[Node("Value", "Values")]
public sealed class ValueNode<T> : Node where T : notnull
{
    public Observable<T> Value { get; }

    public ValueNode(T defaultValue)
    {
        Value = new Observable<T>(defaultValue);
    }

    [NodeProcess([], [""])]
    private T process() => Value.Value;
}

// for nodes that are user inputs like buttons
public abstract class InputNode : Node
{
}

[Node("Call", "Inputs")]
public sealed class ButtonInputNode : InputNode
{
    private readonly NodeFlowRef outFlow;

    public ButtonInputNode()
    {
        outFlow = AddFlow("On Click", ConnectionSide.Output);
    }

    [NodeTrigger]
    private bool wasButtonClicked() => true;

    [NodeProcess([], [])]
    private void process() => SetFlow(outFlow);
}

public sealed class ObservableVector2 : INotifyPropertyChanged
{
    private double x;
    private double y;

    public double X
    {
        get => x;
        set
        {
            x = value;
            OnPropertyChanged();
        }
    }

    public double Y
    {
        get => y;
        set
        {
            y = value;
            OnPropertyChanged();
        }
    }

    public ObservableVector2(double x = 0, double y = 0)
    {
        this.x = x;
        this.y = y;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}