// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Math.Random;

using System;

[Node("Random Float", "Math/Random")]
public sealed class RandomFloatNode : RandomNode<float>
{
    protected override float GetRandom(float min, float max) => App.Utils.Interpolation.Map(Random.Shared.NextSingle(), 0, 1, min, max);
}