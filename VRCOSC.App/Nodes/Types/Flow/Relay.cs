// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Relay", "Utility/Relay")]
public sealed class RelayNode<T>() : SimpleValueTransformNode<T>(v => v);

[Node("Update Relay", "Utility/Relay")]
[NodeCollapsed]
public sealed class UpdateRelayNode<T>() : SimpleValueTransformNode<T>(v => v), IContinuousNode
{
    public int UpdateOffset => 0;
}