// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.Utils;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.SDK.Nodes.Types.Math;

[Node("Remap", "Math/Map")]
public class RemapNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Value")] float value,
        [NodeValue("Value Min")] float vMin,
        [NodeValue("Value Max")] float vMax,
        [NodeValue("To Min")] float dMin,
        [NodeValue("To Max")] float dMax,
        [NodeValue("Value")] ref float outValue
    )
    {
        outValue = Interpolation.Map(value, vMin, vMax, dMin, dMax);
    }
}

[Node("Remap -1,1 To 0,1", "Math/Map")]
public class Remap_11_01 : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Value")] float value,
        [NodeValue("Value")] ref float outValue
    )
    {
        outValue = Interpolation.Map(value, -1, 1, 0, 1);
    }
}

[Node("Remap 0,1 To -1,1", "Math/Map")]
public class Remap_01_11 : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Value")] float value,
        [NodeValue("Value")] ref float outValue
    )
    {
        outValue = Interpolation.Map(value, 0, 1, -1, 1);
    }
}