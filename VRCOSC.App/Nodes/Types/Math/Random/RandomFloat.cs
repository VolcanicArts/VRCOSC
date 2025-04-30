// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.Nodes.Types.Math.Random;

[Node("Random Float", "Math/Random")]
public sealed class RandomFloatNode : RandomNode<float>
{
    protected override float GetRandom(float min, float max) => App.Utils.Interpolation.Map(System.Random.Shared.NextSingle(), 0, 1, min, max);
}