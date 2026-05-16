// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;

namespace VRCOSC.App.Nodes.Types.Math;

[Node("Factorial", "Math")]
[NodeCollapsed]
public sealed class FactorialNode<T> : ValueTransformNode<int, T> where T : INumberBase<T>
{
    protected override T TransformValue(int value, IPulseContext c)
    {
        var result = T.One;

        for (var i = 1; i <= value; i++)
        {
            result *= T.CreateSaturating(i);
        }

        return result;
    }
}