// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Nodes.Types.Math;

[Node("Damp Continuously", "Math/Interpolation")]
public class DampContinuouslyNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Current")] float current,
        [NodeValue("Target")] float target,
        [NodeValue("Half Time Milliseconds")] float halfTimeMilli,
        [NodeValue("Elapsed Time Milliseconds")] float elapsedTimeMilli,
        [NodeValue("Result")] ref float result
    )
    {
        result = Interpolation.DampContinuously(current, target, halfTimeMilli, elapsedTimeMilli);
    }
}