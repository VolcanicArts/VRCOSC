// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using FontAwesome6;

namespace VRCOSC.App.Nodes.Types.Math.Transform;

[Node("Unpack Transform", "Math/Transform")]
[NodeCollapsed]
public sealed class TransformUnpackNode() : ValueConsumeNode<Utils.Transform>(nameof(Utils.Transform))
{
    public ValueOutput<Vector3> Position = new();
    public ValueOutput<System.Numerics.Quaternion> Rotation = new();

    protected override void ConsumeValue(Utils.Transform transform, IPulseContext c)
    {
        Position.Write(transform.Position, c);
        Rotation.Write(transform.Rotation, c);
    }
}

[Node("Pack Transform", "Math/Transform")]
[NodeCollapsed]
public sealed class TransformPackNode() : ValueComputeNode<Utils.Transform>(nameof(Utils.Transform))
{
    public ValueInput<Vector3> Position = new();
    public ValueInput<System.Numerics.Quaternion> Rotation = new(defaultValue: System.Numerics.Quaternion.Identity);

    protected override Utils.Transform ComputeValue(IPulseContext c) => new(Position.Read(c), Rotation.Read(c));
}

[Node("Transform Relative To", "Math/Transform")]
public sealed class TransformRelativeToNode : ValueComputeNode<Utils.Transform>
{
    public ValueInput<Utils.Transform> Source = new(defaultValue: Utils.Transform.Identity);
    public ValueInput<Utils.Transform> Parent = new(defaultValue: Utils.Transform.Identity);

    protected override Utils.Transform ComputeValue(IPulseContext c) => Source.Read(c).RelativeTo(Parent.Read(c));
}

[Node("Transform Apply", "Math/Transform")]
public sealed class TransformApplyNode : ValueComputeNode<Utils.Transform>
{
    public ValueInput<Utils.Transform> Source = new(defaultValue: Utils.Transform.Identity);
    public ValueInput<Utils.Transform> Parent = new(defaultValue: Utils.Transform.Identity);

    protected override Utils.Transform ComputeValue(IPulseContext c) => Source.Read(c).Apply(Parent.Read(c));
}

[Node("Add Position", "Math/Transform")]
[NodeCollapsed(EFontAwesomeIcon.Solid_Plus)]
public sealed class TransformPositionAddNode() : SimpleResultComputeNode<Utils.Transform, Vector3, Utils.Transform>((transform, position) => transform.Apply(new Utils.Transform(position, System.Numerics.Quaternion.Identity)));

[Node("Add Rotation", "Math/Transform")]
[NodeCollapsed(EFontAwesomeIcon.Solid_Plus)]
public sealed class TransformRotationAddNode() : SimpleResultComputeNode<Utils.Transform, System.Numerics.Quaternion, Utils.Transform>((transform, rotation) => transform.Apply(new Utils.Transform(Vector3.Zero, rotation)));

[Node("Forward", "Math/Transform")]
[NodeCollapsed]
public sealed class TransformForwardNode() : SimpleValueTransformNode<Utils.Transform, Vector3>(t => t.Forward);

[Node("Backward", "Math/Transform")]
[NodeCollapsed]
public sealed class TransformBackwardNode() : SimpleValueTransformNode<Utils.Transform, Vector3>(t => t.Backward);

[Node("Right", "Math/Transform")]
[NodeCollapsed]
public sealed class TransformRightNode() : SimpleValueTransformNode<Utils.Transform, Vector3>(t => t.Right);

[Node("Left", "Math/Transform")]
[NodeCollapsed]
public sealed class TransformLeftNode() : SimpleValueTransformNode<Utils.Transform, Vector3>(t => t.Left);

[Node("Up", "Math/Transform")]
[NodeCollapsed]
public sealed class TransformUpNode() : SimpleValueTransformNode<Utils.Transform, Vector3>(t => t.Up);

[Node("Down", "Math/Transform")]
[NodeCollapsed]
public sealed class TransformDownNode() : SimpleValueTransformNode<Utils.Transform, Vector3>(t => t.Down);