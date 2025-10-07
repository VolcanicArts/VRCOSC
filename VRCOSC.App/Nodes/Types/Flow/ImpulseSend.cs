// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Impulse Send", "Flow/Impulse Send")]
public class ImpulseSendNode : Node, IImpulseSender, IFlowInput, IHasTextProperty
{
    public FlowContinuation Next = new("Next");

    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    protected override async Task Process(PulseContext c)
    {
        await c.Graph.TriggerImpulse(new ImpulseDefinition(Text, []), c);
        await Next.Execute(c);
    }
}

[Node("Impulse Send With Data", "Flow/Impulse Send")]
public class ImpulseSendNode<T1> : Node, IImpulseSender, IFlowInput, IHasTextProperty
{
    public FlowContinuation Next = new("Next");

    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public ValueInput<T1> First = new(typeof(T1).GetFriendlyName());

    protected override async Task Process(PulseContext c)
    {
        await c.Graph.TriggerImpulse(new ImpulseDefinition(Text, [First.Read(c)!]), c);
        await Next.Execute(c);
    }
}

[Node("Impulse Send With Data 2", "Flow/Impulse Send")]
public class ImpulseSendNode<T1, T2> : Node, IImpulseSender, IFlowInput, IHasTextProperty
{
    public FlowContinuation Next = new("Next");

    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public ValueInput<T1> First = new(typeof(T1).GetFriendlyName());
    public ValueInput<T2> Second = new(typeof(T2).GetFriendlyName());

    protected override async Task Process(PulseContext c)
    {
        await c.Graph.TriggerImpulse(new ImpulseDefinition(Text, [First.Read(c)!, Second.Read(c)!]), c);
        await Next.Execute(c);
    }
}

[Node("Impulse Send With Data 3", "Flow/Impulse Send")]
public class ImpulseSendNode<T1, T2, T3> : Node, IImpulseSender, IFlowInput, IHasTextProperty
{
    public FlowContinuation Next = new("Next");

    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public ValueInput<T1> First = new(typeof(T1).GetFriendlyName());
    public ValueInput<T2> Second = new(typeof(T2).GetFriendlyName());
    public ValueInput<T3> Third = new(typeof(T3).GetFriendlyName());

    protected override async Task Process(PulseContext c)
    {
        await c.Graph.TriggerImpulse(new ImpulseDefinition(Text, [First.Read(c)!, Second.Read(c)!, Third.Read(c)!]), c);
        await Next.Execute(c);
    }
}

[Node("Impulse Send With Data 4", "Flow/Impulse Send")]
public class ImpulseSendNode<T1, T2, T3, T4> : Node, IImpulseSender, IFlowInput, IHasTextProperty
{
    public FlowContinuation Next = new("Next");

    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public ValueInput<T1> First = new(typeof(T1).GetFriendlyName());
    public ValueInput<T2> Second = new(typeof(T2).GetFriendlyName());
    public ValueInput<T3> Third = new(typeof(T3).GetFriendlyName());
    public ValueInput<T4> Fourth = new(typeof(T4).GetFriendlyName());

    protected override async Task Process(PulseContext c)
    {
        await c.Graph.TriggerImpulse(new ImpulseDefinition(Text, [First.Read(c)!, Second.Read(c)!, Third.Read(c)!, Fourth.Read(c)!]), c);
        await Next.Execute(c);
    }
}