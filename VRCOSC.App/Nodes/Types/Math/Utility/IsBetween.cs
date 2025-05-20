// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Math.Utility;

[Node("Is Between", "Math/Utility")]
public class IsBetweenNode<T> : Node where T : INumber<T>
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Value")] float value,
        [NodeValue("Min")] float min,
        [NodeValue("Max")] float max,
        [NodeValue("Result")] Ref<bool> outResult
    )
    {
        outResult.Value = value >= min && value <= max;
    }
}