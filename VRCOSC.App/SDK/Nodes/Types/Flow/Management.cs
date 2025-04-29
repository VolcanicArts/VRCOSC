// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Flow;

[Node("Value Relay", "Utility")]
public sealed class ValueRelayNode<T> : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] T value,
        [NodeValue] Ref<T> outValue
    )
    {
        outValue.Value = value;
    }
}