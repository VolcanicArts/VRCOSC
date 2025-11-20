// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.Nodes.Types.Math.Constants;

[Node("E", "Math/Constants")]
public sealed class EConstantNode : ConstantNode<float>
{
    protected override float GetValue() => MathF.E;
}

[Node("Pi", "Math/Constants")]
public sealed class PiConstantNode : ConstantNode<float>
{
    protected override float GetValue() => MathF.PI;
}

[Node("Tau", "Math/Constants")]
public sealed class TauConstantNode : ConstantNode<float>
{
    protected override float GetValue() => MathF.Tau;
}