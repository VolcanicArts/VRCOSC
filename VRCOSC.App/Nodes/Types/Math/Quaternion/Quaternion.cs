// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

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