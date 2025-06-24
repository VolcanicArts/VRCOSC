// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math.Utility;

[Node("Is Between", "Math/Utility")]
public class IsBetweenNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> Value = new();
    public ValueInput<T> Min = new();
    public ValueInput<T> Max = new();
    public ValueOutput<bool> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(Value.Read(c) >= Min.Read(c) && Value.Read(c) <= Max.Read(c), c);
        return Task.CompletedTask;
    }
}