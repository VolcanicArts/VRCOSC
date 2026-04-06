// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;
using FontAwesome6;

namespace VRCOSC.App.Nodes.Types.Math;

[Node("Square Root", "Math", EFontAwesomeIcon.Solid_SquareRootVariable)]
public sealed class SquareRootNode<T> : Node where T : IRootFunctions<T>
{
    public ValueInput<T> A = new();
    public ValueOutput<T> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(T.Sqrt(A.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Power", "Math", EFontAwesomeIcon.Solid_Superscript)]
public sealed class PowerNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> A = new();
    public ValueInput<T> B = new();
    public ValueOutput<T> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(T.Pow(A.Read(c), B.Read(c)), c);
        return Task.CompletedTask;
    }
}