// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Impulse", "Flow/Impulse")]
public class ImpulseNode : Node, IFlowInput
{
    public FlowContinuation OnComplete = new("On Complete");

    public ValueInput<string> Name = new();

    protected override void Process(PulseContext c)
    {
    }
}

[Node("Impulse With Value", "Flow/Impulse")]
public class ImpulseNode<T1> : ImpulseNode
{
    public ValueInput<T1> First = new();

    protected override void Process(PulseContext c)
    {
        base.Process(c);
    }
}

[Node("Impulse With Value 2", "Flow/Impulse")]
public class ImpulseNode<T1, T2> : ImpulseNode<T1>
{
    public ValueInput<T2> Second = new();

    protected override void Process(PulseContext c)
    {
        base.Process(c);
    }
}

[Node("Impulse With Value 3", "Flow/Impulse")]
public class ImpulseNode<T1, T2, T3> : ImpulseNode<T1, T2>
{
    public ValueInput<T3> Third = new();

    protected override void Process(PulseContext c)
    {
        base.Process(c);
    }
}

[Node("Impulse With Value 4", "Flow/Impulse")]
public class ImpulseNode<T1, T2, T3, T4> : ImpulseNode<T1, T2, T3>
{
    public ValueInput<T4> Fourth = new();

    protected override void Process(PulseContext c)
    {
        base.Process(c);
    }
}

[Node("Impulse With Value 5", "Flow/Impulse")]
public class ImpulseNode<T1, T2, T3, T4, T5> : ImpulseNode<T1, T2, T3, T4>
{
    public ValueInput<T5> Fifth = new();

    protected override void Process(PulseContext c)
    {
        base.Process(c);
    }
}

[Node("Impulse With Value 6", "Flow/Impulse")]
public class ImpulseNode<T1, T2, T3, T4, T5, T6> : ImpulseNode<T1, T2, T3, T4, T5>
{
    public ValueInput<T6> Sixth = new();

    protected override void Process(PulseContext c)
    {
        base.Process(c);
    }
}

[Node("Impulse With Value 7", "Flow/Impulse")]
public class ImpulseNode<T1, T2, T3, T4, T5, T6, T7> : ImpulseNode<T1, T2, T3, T4, T5, T6>
{
    public ValueInput<T7> Seventh = new();

    protected override void Process(PulseContext c)
    {
        base.Process(c);
    }
}

[Node("Impulse With Value 8", "Flow/Impulse")]
public class ImpulseNode<T1, T2, T3, T4, T5, T6, T7, T8> : ImpulseNode<T1, T2, T3, T4, T5, T6, T7>
{
    public ValueInput<T8> Eighth = new();

    protected override void Process(PulseContext c)
    {
        base.Process(c);
    }
}