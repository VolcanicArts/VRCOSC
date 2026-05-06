// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math.Transform;

[Node("Unpack Transform", "Math/Transform")]
public sealed class TransformUnpackNode : Node
{
    public ValueInput<Utils.Transform> Transform = new();

    public ValueOutput<Vector3> Position = new();
    public ValueOutput<System.Numerics.Quaternion> Rotation = new();

    protected override Task Process(PulseContext c)
    {
        Position.Write(Transform.Read(c).Position, c);
        Rotation.Write(Transform.Read(c).Rotation, c);
        return Task.CompletedTask;
    }
}

[Node("Pack Transform", "Math/Transform")]
public sealed class TransformPackNode() : ValueComputeNode<Utils.Transform>("Transform")
{
    public ValueInput<Vector3> Position = new();
    public ValueInput<System.Numerics.Quaternion> Rotation = new(defaultValue: System.Numerics.Quaternion.Identity);

    protected override Utils.Transform ComputeValue(PulseContext c) => new(Position.Read(c), Rotation.Read(c));
}

[Node("Transform Relative To", "Math/Transform")]
public sealed class TransformRelativeToNode() : SimpleResultComputeNode<Utils.Transform>((s, p) => s.RelativeTo(p), "Source", "Parent");