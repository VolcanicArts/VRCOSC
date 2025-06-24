// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Impulse Receive", "Flow/Impulse Receive")]
public class ImpulseReceiveNode : Node, IImpulseReceiver
{
    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public FlowCall OnCall = new("On Call");

    protected override async Task Process(PulseContext c)
    {
        await OnCall.Execute(c);
    }

    public void WriteOutputs(object[] values, PulseContext c)
    {
    }
}

[Node("Impulse Receive With Data", "Flow/Impulse Receive")]
public class ImpulseReceiveNode<T1> : Node, IImpulseReceiver
{
    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public FlowCall OnCall = new("On Call");

    public ValueOutput<T1> First = new(typeof(T1).GetFriendlyName());

    protected override async Task Process(PulseContext c)
    {
        await OnCall.Execute(c);
    }

    public void WriteOutputs(object[] values, PulseContext c)
    {
        First.Write((T1)values[0], c);
    }
}

[Node("Impulse Receive With Data 2", "Flow/Impulse Receive")]
public class ImpulseReceiveNode<T1, T2> : Node, IImpulseReceiver
{
    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public FlowCall OnCall = new("On Call");

    public ValueOutput<T1> First = new(typeof(T1).GetFriendlyName());
    public ValueOutput<T2> Second = new(typeof(T2).GetFriendlyName());

    protected override async Task Process(PulseContext c)
    {
        await OnCall.Execute(c);
    }

    public void WriteOutputs(object[] values, PulseContext c)
    {
        First.Write((T1)values[0], c);
        Second.Write((T2)values[1], c);
    }
}

[Node("Impulse Receive With Data 3", "Flow/Impulse Receive")]
public class ImpulseReceiveNode<T1, T2, T3> : Node, IImpulseReceiver
{
    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public FlowCall OnCall = new("On Call");

    public ValueOutput<T1> First = new(typeof(T1).GetFriendlyName());
    public ValueOutput<T2> Second = new(typeof(T2).GetFriendlyName());
    public ValueOutput<T3> Third = new(typeof(T3).GetFriendlyName());

    protected override async Task Process(PulseContext c)
    {
        await OnCall.Execute(c);
    }

    public void WriteOutputs(object[] values, PulseContext c)
    {
        First.Write((T1)values[0], c);
        Second.Write((T2)values[1], c);
        Third.Write((T3)values[2], c);
    }
}

[Node("Impulse Receive With Data 4", "Flow/Impulse Receive")]
public class ImpulseReceiveNode<T1, T2, T3, T4> : Node, IImpulseReceiver
{
    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public FlowCall OnCall = new("On Call");

    public ValueOutput<T1> First = new(typeof(T1).GetFriendlyName());
    public ValueOutput<T2> Second = new(typeof(T2).GetFriendlyName());
    public ValueOutput<T3> Third = new(typeof(T3).GetFriendlyName());
    public ValueOutput<T4> Fourth = new(typeof(T4).GetFriendlyName());

    protected override async Task Process(PulseContext c)
    {
        await OnCall.Execute(c);
    }

    public void WriteOutputs(object[] values, PulseContext c)
    {
        First.Write((T1)values[0], c);
        Second.Write((T2)values[1], c);
        Third.Write((T3)values[2], c);
        Fourth.Write((T4)values[3], c);
    }
}