// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.Nodes.Types.Base;

namespace VRCOSC.App.Nodes.Types.Math;

[Node("Pi", "Math/Constants")]
[NodeCollapsed]
public sealed class PiConstantNode : ConstantNode<float>
{
    protected override float GetValue() => MathF.PI;
}