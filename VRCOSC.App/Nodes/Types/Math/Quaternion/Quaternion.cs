// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Math.Quaternion;

[Node("Quaternion To Euler", "Math/Quaternion")]
[NodeCollapsed]
public sealed class QuaternionToEulerNode() : SimpleValueTransformNode<System.Numerics.Quaternion, Vector3>(v => v.ToEulerDegrees());

[Node("Euler To Quaternion", "Math/Quaternion")]
[NodeCollapsed]
public sealed class EulerToQuaternionNode() : SimpleValueTransformNode<Vector3, System.Numerics.Quaternion>(v => v.ToQuaternion());

[Node("Multiply Quaternion", "Math/Quaternion")]
[NodeCollapsed]
public sealed class QuaternionMultiplyNode() : SimpleResultComputeNode<System.Numerics.Quaternion>((a, b) => a * b);