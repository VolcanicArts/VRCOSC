// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Fire On Burst", "Flow")]
public sealed class FireOnBurstNode : Node
{
    public FlowContinuation Next = new("Next");

    public GlobalStore<int> BecameTrue = new();
    public GlobalStore<DateTime> TrueTime = new();
    public GlobalStore<int> BecameFalse = new();

    public ValueInput<int> Milliseconds = new();
    public ValueInput<int> Count = new(defaultValue: 2);
    public ValueInput<bool> Condition = new();

    protected override Task Process(PulseContext c) => Next.Execute(c);

    protected override bool ShouldProcess(PulseContext c)
    {
        var becameTrue = BecameTrue.Read(c);
        var trueTime = TrueTime.Read(c);
        var becameFalse = BecameFalse.Read(c);
        var milliseconds = Milliseconds.Read(c);
        var count = Count.Read(c);
        var condition = Condition.Read(c);

        if (becameTrue != 0 && (DateTime.Now - trueTime).TotalMilliseconds > milliseconds)
        {
            BecameTrue.Write(condition ? 1 : 0, c);
            BecameFalse.Write(0, c);
            if (condition) TrueTime.Write(DateTime.Now, c);
            return false;
        }

        if (becameTrue == becameFalse && becameTrue + 1 != count && condition)
        {
            BecameTrue.Write(++becameTrue, c);
            if (becameTrue == 1) TrueTime.Write(DateTime.Now, c);
            return false;
        }

        if (becameTrue == becameFalse + 1 && becameTrue != count && !condition)
        {
            BecameFalse.Write(++becameFalse, c);
            return false;
        }

        if (becameTrue == becameFalse && becameTrue + 1 == count && condition)
        {
            BecameTrue.Write(0, c);
            BecameFalse.Write(0, c);
            return true;
        }

        return false;
    }
}