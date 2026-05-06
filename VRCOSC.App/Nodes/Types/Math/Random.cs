// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Numerics;

namespace VRCOSC.App.Nodes.Types.Math;

[Node("Random", "Math")]
public sealed class RandomNode<T>() : SimpleResultComputeNode<T>((min, max) => Utils.Interpolation.Map(Random.Shared.NextDouble(), 0d, 1d, min, max), "Min", "Max") where T : INumberBase<T>;