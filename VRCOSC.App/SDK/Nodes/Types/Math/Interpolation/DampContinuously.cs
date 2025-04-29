// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Math.Interpolation;

[Node("Damp Continuously", "Math/Interpolation")]
public sealed class DampContinuouslyNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Current")] float current,
        [NodeValue("Target")] float target,
        [NodeValue("Length Milliseconds")] float lengthMilli,
        [NodeValue("Elapsed Time Milliseconds")] float elapsedTimeMilli,
        [NodeValue("Result")] Ref<float> result
    )
    {
        result.Value = App.Utils.Interpolation.DampContinuously(current, target, lengthMilli / 2f, elapsedTimeMilli);
    }
}