// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Impulse Receive", "Flow")]
public class ImpulseReceiveNode : Node, IImpulseReceiver
{
    public FlowOutput OnCall = new();

    [InputMode(InputModes.Inline)]
    public ValueInput<string> Name = new();

    protected override Task Process(IPulseContext c) => OnCall.Execute(c);

    public bool CanReceive(string checkName, IPulseContext c)
    {
        var name = Name.Read(c);
        if (string.IsNullOrEmpty(name)) return false;

        return string.Equals(name, checkName, StringComparison.CurrentCulture);
    }

    public virtual void WriteOutputs(object[] values, IPulseContext c)
    {
    }
}

public class ImpulseReceiveNode<T1> : ImpulseReceiveNode
{
    public ValueOutput<T1> First = new(typeof(T1).GetFriendlyName());

    public override void WriteOutputs(object[] values, IPulseContext c)
    {
        First.Write((T1)values[0], c);
    }
}

public class ImpulseReceiveNode<T1, T2> : ImpulseReceiveNode<T1>
{
    public ValueOutput<T2> Second = new(typeof(T2).GetFriendlyName());

    public override void WriteOutputs(object[] values, IPulseContext c)
    {
        base.WriteOutputs(values, c);
        Second.Write((T2)values[1], c);
    }
}

public class ImpulseReceiveNode<T1, T2, T3> : ImpulseReceiveNode<T1, T2>
{
    public ValueOutput<T3> Third = new(typeof(T3).GetFriendlyName());

    public override void WriteOutputs(object[] values, IPulseContext c)
    {
        base.WriteOutputs(values, c);
        Third.Write((T3)values[2], c);
    }
}

public sealed class ImpulseReceiveNode<T1, T2, T3, T4> : ImpulseReceiveNode<T1, T2, T3>
{
    public ValueOutput<T4> Fourth = new(typeof(T4).GetFriendlyName());

    public override void WriteOutputs(object[] values, IPulseContext c)
    {
        base.WriteOutputs(values, c);
        Fourth.Write((T4)values[3], c);
    }
}