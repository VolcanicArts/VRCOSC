// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.Nodes.Types;

[NodeCollapsed]
public abstract class ConstantNode<T>(T value) : ValueComputeNode<T>
{
    protected override T ComputeValue(IPulseContext c) => value;
}