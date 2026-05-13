// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;

namespace VRCOSC.App.Nodes.Types.Math.Transform;

[Node("Unpack Transform", "Math/Transform")]
public sealed class TransformUnpackNode() : ValueConsumeNode<Utils.Transform>(nameof(Utils.Transform))
{
    public ValueOutput<Vector3> Position = new();
    public ValueOutput<System.Numerics.Quaternion> Rotation = new();

    protected override void ConsumeValue(Utils.Transform transform, PulseContext c)
    {
        Position.Write(transform.Position, c);
        Rotation.Write(transform.Rotation, c);
    }
}

[Node("Pack Transform", "Math/Transform")]
public sealed class TransformPackNode() : ValueComputeNode<Utils.Transform>(nameof(Utils.Transform))
{
    public ValueInput<Vector3> Position = new();
    public ValueInput<System.Numerics.Quaternion> Rotation = new(defaultValue: System.Numerics.Quaternion.Identity);

    protected override Utils.Transform ComputeValue(PulseContext c) => new(Position.Read(c), Rotation.Read(c));
}

[Node("Transform Relative To", "Math/Transform")]
public sealed class TransformRelativeToNode() : SimpleResultComputeNode<Utils.Transform>((s, p) => s.RelativeTo(p), "Source", "Parent");