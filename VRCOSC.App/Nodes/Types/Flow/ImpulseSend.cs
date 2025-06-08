// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Impulse Send", "Flow/Impulse Send")]
public class ImpulseSendNode : Node, IImpulseSender, IFlowInput
{
    [NodeProperty("name")]
    public string Name { get; set; } = string.Empty;

    protected override void Process(PulseContext c)
    {
        c.Field.TriggerImpulse(new ImpulseDefinition(Name, []), c);
    }
}

[Node("Impulse Send With Data", "Flow/Impulse Send")]
public class ImpulseSendNode<T1> : Node, IImpulseSender, IFlowInput
{
    [NodeProperty("name")]
    public string Name { get; set; } = string.Empty;

    public ValueInput<T1> First = new();

    protected override void Process(PulseContext c)
    {
        c.Field.TriggerImpulse(new ImpulseDefinition(Name, [First.Read(c)!]), c);
    }
}

[Node("Impulse Send With Data 2", "Flow/Impulse Send")]
public class ImpulseSendNode<T1, T2> : Node, IImpulseSender, IFlowInput
{
    [NodeProperty("name")]
    public string Name { get; set; } = string.Empty;

    public ValueInput<T1> First = new();
    public ValueInput<T2> Second = new();

    protected override void Process(PulseContext c)
    {
        c.Field.TriggerImpulse(new ImpulseDefinition(Name, [First.Read(c)!, Second.Read(c)!]), c);
    }
}

[Node("Impulse Send With Data 3", "Flow/Impulse Send")]
public class ImpulseSendNode<T1, T2, T3> : Node, IImpulseSender, IFlowInput
{
    [NodeProperty("name")]
    public string Name { get; set; } = string.Empty;

    public ValueInput<T1> First = new();
    public ValueInput<T2> Second = new();
    public ValueInput<T3> Third = new();

    protected override void Process(PulseContext c)
    {
        c.Field.TriggerImpulse(new ImpulseDefinition(Name, [First.Read(c)!, Second.Read(c)!, Third.Read(c)!]), c);
    }
}

[Node("Impulse Send With Data 4", "Flow/Impulse Send")]
public class ImpulseSendNode<T1, T2, T3, T4> : Node, IImpulseSender, IFlowInput
{
    [NodeProperty("name")]
    public string Name { get; set; } = string.Empty;

    public ValueInput<T1> First = new();
    public ValueInput<T2> Second = new();
    public ValueInput<T3> Third = new();
    public ValueInput<T4> Fourth = new();

    protected override void Process(PulseContext c)
    {
        c.Field.TriggerImpulse(new ImpulseDefinition(Name, [First.Read(c)!, Second.Read(c)!, Third.Read(c)!, Fourth.Read(c)!]), c);
    }
}