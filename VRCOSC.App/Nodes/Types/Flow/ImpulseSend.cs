// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Impulse Send", "Flow")]
public class ImpulseSendNode : AsyncActionNode, IImpulseSender
{
    public ValueInput<string> Name = new();

    protected override async Task DoActionAsync(IPulseContext c)
    {
        var name = Name.Read(c);
        if (string.IsNullOrEmpty(name)) return;

        var data = GetImpulseData(c);
        await ContainingGraph.TriggerImpulse(new ImpulseDefinition(name, data), c);
        await Next.Execute(c);
    }

    protected virtual object[] GetImpulseData(IPulseContext c) => [];
}

public class ImpulseSendNode<T1> : ImpulseSendNode
{
    public ValueInput<T1> First = new(typeof(T1).GetFriendlyName());

    protected override object[] GetImpulseData(IPulseContext c) => [First.Read(c)!];
}

public class ImpulseSendNode<T1, T2> : ImpulseSendNode<T1>
{
    public ValueInput<T2> Second = new(typeof(T2).GetFriendlyName());

    protected override object[] GetImpulseData(IPulseContext c) => [First.Read(c)!, Second.Read(c)!];
}

public class ImpulseSendNode<T1, T2, T3> : ImpulseSendNode<T1, T2>
{
    public ValueInput<T3> Third = new(typeof(T3).GetFriendlyName());

    protected override object[] GetImpulseData(IPulseContext c) => [First.Read(c)!, Second.Read(c)!, Third.Read(c)!];
}

public sealed class ImpulseSendNode<T1, T2, T3, T4> : ImpulseSendNode<T1, T2, T3>
{
    public ValueInput<T4> Fourth = new(typeof(T4).GetFriendlyName());

    protected override object[] GetImpulseData(IPulseContext c) => [First.Read(c)!, Second.Read(c)!, Third.Read(c)!, Fourth.Read(c)!];
}