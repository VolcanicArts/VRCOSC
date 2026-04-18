// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;

namespace VRCOSC.App.Nodes.Types.Math;

[Node("Is Between", "Math")]
public sealed class IsBetweenNode<T> : ValueComputeNode<bool> where T : IComparisonOperators<T, T, bool>
{
    public ValueInput<T> Value = new();
    public ValueInput<T> Min = new();
    public ValueInput<T> Max = new();

    protected override bool ComputeValue(PulseContext c) => Value.Read(c) >= Min.Read(c) && Value.Read(c) <= Max.Read(c);
}