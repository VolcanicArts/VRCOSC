// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Math.Random;

using System;

[Node("Random Int", "Math/Random")]
public sealed class RandomIntNode : RandomNode<int>
{
    protected override int GetRandom(int min, int max) => Random.Shared.Next(min, max + 1);
}