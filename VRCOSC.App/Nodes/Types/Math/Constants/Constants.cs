// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.Nodes.Types.Math.Constants;

[Node("E", "Math/Constants")]
[NodeCollapsed]
public sealed class EValueOutputNode : ValueOutputNode<float>
{
    protected override float GetValue() => MathF.E;
}

[Node("Pi", "Math/Constants")]
[NodeCollapsed]
public sealed class PiValueOutputNode : ValueOutputNode<float>
{
    protected override float GetValue() => MathF.PI;
}

[Node("Tau", "Math/Constants")]
[NodeCollapsed]
public sealed class TauValueOutputNode : ValueOutputNode<float>
{
    protected override float GetValue() => MathF.Tau;
}