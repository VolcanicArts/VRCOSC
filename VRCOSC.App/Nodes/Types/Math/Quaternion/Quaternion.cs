// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Numerics;
using FontAwesome6;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Math.Quaternion;

[Node("Quaternion To Euler", "Math/Quaternion")]
[NodeCollapsed]
public sealed class QuaternionToEulerNode : ValueComputeNode<Vector3>
{
    public ValueInput<System.Numerics.Quaternion> Quaternion = new(defaultValue: System.Numerics.Quaternion.Identity);

    protected override Vector3 ComputeValue(IPulseContext c) => Quaternion.Read(c).ToEulerDegrees();
}

[Node("Euler To Quaternion", "Math/Quaternion")]
[NodeCollapsed]
public sealed class EulerToQuaternionNode() : SimpleValueTransformNode<Vector3, System.Numerics.Quaternion>(v => v.ToQuaternion());

[Node("Multiply Quaternion", "Math/Quaternion")]
[NodeCollapsed(EFontAwesomeIcon.Solid_Asterisk)]
public sealed class QuaternionMultiplyNode : ValueComputeNode<System.Numerics.Quaternion>
{
    public ValueInput<System.Numerics.Quaternion> A = new(defaultValue: System.Numerics.Quaternion.Identity);
    public ValueInput<System.Numerics.Quaternion> B = new(defaultValue: System.Numerics.Quaternion.Identity);

    protected override System.Numerics.Quaternion ComputeValue(IPulseContext c) => A.Read(c) * B.Read(c);
}

[Node("Quaternion Inverse", "Math/Quaternion")]
[NodeCollapsed]
public sealed class QuaternionInverseNode : ValueComputeNode<System.Numerics.Quaternion>
{
    public ValueInput<System.Numerics.Quaternion> Quaternion = new(defaultValue: System.Numerics.Quaternion.Identity);

    protected override System.Numerics.Quaternion ComputeValue(IPulseContext c)
    {
        var q = Quaternion.Read(c);
        return System.Numerics.Quaternion.Inverse(q);
    }
}

[Node("Quaternion Conjugate", "Math/Quaternion")]
[NodeCollapsed]
public sealed class QuaternionConjugateNode : ValueComputeNode<System.Numerics.Quaternion>
{
    public ValueInput<System.Numerics.Quaternion> Quaternion = new(defaultValue: System.Numerics.Quaternion.Identity);

    protected override System.Numerics.Quaternion ComputeValue(IPulseContext c)
    {
        var q = Quaternion.Read(c);
        return new System.Numerics.Quaternion(-q.X, -q.Y, -q.Z, q.W);
    }
}

[Node("Quaternion Dot", "Math/Quaternion")]
[NodeCollapsed]
public sealed class QuaternionDotNode : ValueComputeNode<float>
{
    public ValueInput<System.Numerics.Quaternion> A = new(defaultValue: System.Numerics.Quaternion.Identity);
    public ValueInput<System.Numerics.Quaternion> B = new(defaultValue: System.Numerics.Quaternion.Identity);

    protected override float ComputeValue(IPulseContext c) => System.Numerics.Quaternion.Dot(A.Read(c), B.Read(c));
}

[Node("Quaternion Dot (Absolute)", "Math/Quaternion")]
[NodeCollapsed]
public sealed class QuaternionDotAbsNode : ValueComputeNode<float>
{
    public ValueInput<System.Numerics.Quaternion> A = new(defaultValue: System.Numerics.Quaternion.Identity);
    public ValueInput<System.Numerics.Quaternion> B = new(defaultValue: System.Numerics.Quaternion.Identity);

    protected override float ComputeValue(IPulseContext c) => MathF.Abs(System.Numerics.Quaternion.Dot(A.Read(c), B.Read(c)));
}

[Node("Quaternion Angular Distance", "Math/Quaternion")]
[NodeCollapsed]
public sealed class QuaternionAngularDistanceNode : ValueComputeNode<float>
{
    public ValueInput<System.Numerics.Quaternion> A = new(defaultValue: System.Numerics.Quaternion.Identity);
    public ValueInput<System.Numerics.Quaternion> B = new(defaultValue: System.Numerics.Quaternion.Identity);

    protected override float ComputeValue(IPulseContext c)
    {
        var dot = System.Numerics.Quaternion.Dot(A.Read(c), B.Read(c));
        dot = float.Clamp(MathF.Abs(dot), -1f, 1f);
        return 2f * MathF.Acos(dot);
    }
}

[Node("Quaternion Angular Distance (Degrees)", "Math/Quaternion")]
[NodeCollapsed]
public sealed class QuaternionAngularDistanceDegreesNode : ValueComputeNode<float>
{
    public ValueInput<System.Numerics.Quaternion> A = new(defaultValue: System.Numerics.Quaternion.Identity);
    public ValueInput<System.Numerics.Quaternion> B = new(defaultValue: System.Numerics.Quaternion.Identity);

    protected override float ComputeValue(IPulseContext c)
    {
        var dot = System.Numerics.Quaternion.Dot(A.Read(c), B.Read(c));
        dot = float.Clamp(MathF.Abs(dot), -1f, 1f);
        return float.RadiansToDegrees(2f * MathF.Acos(dot));
    }
}

[Node("Quaternion Normalize", "Math/Quaternion")]
[NodeCollapsed]
public sealed class QuaternionNormalizeNode : ValueComputeNode<System.Numerics.Quaternion>
{
    public ValueInput<System.Numerics.Quaternion> Quaternion = new(defaultValue: System.Numerics.Quaternion.Identity);

    protected override System.Numerics.Quaternion ComputeValue(IPulseContext c) => System.Numerics.Quaternion.Normalize(Quaternion.Read(c));
}

[Node("Transform Vector by Quaternion", "Math/Quaternion")]
[NodeCollapsed]
public sealed class QuaternionTransformVectorNode : ValueComputeNode<Vector3>
{
    public ValueInput<System.Numerics.Quaternion> Quaternion = new(defaultValue: System.Numerics.Quaternion.Identity);
    public ValueInput<Vector3> Vector = new(defaultValue: Vector3.Zero);

    protected override Vector3 ComputeValue(IPulseContext c)
    {
        var q = Quaternion.Read(c);
        var v = Vector.Read(c);
        var qv = new System.Numerics.Quaternion(v.X, v.Y, v.Z, 0);
        var result = q * qv * System.Numerics.Quaternion.Conjugate(q);
        return new Vector3(result.X, result.Y, result.Z);
    }
}

[Node("Quaternion Slerp", "Math/Quaternion")]
[NodeCollapsed]
public sealed class QuaternionSlerpNode : ValueComputeNode<System.Numerics.Quaternion>
{
    public ValueInput<System.Numerics.Quaternion> A = new(defaultValue: System.Numerics.Quaternion.Identity);
    public ValueInput<System.Numerics.Quaternion> B = new(defaultValue: System.Numerics.Quaternion.Identity);
    public ValueInput<float> T = new(defaultValue: 0.5f);

    protected override System.Numerics.Quaternion ComputeValue(IPulseContext c)
    {
        var a = A.Read(c);
        var b = B.Read(c);
        var t = float.Clamp(T.Read(c), 0f, 1f);
        return System.Numerics.Quaternion.Slerp(a, b, t);
    }
}