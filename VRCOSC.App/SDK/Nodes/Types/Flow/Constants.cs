// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes.Types.Base;

namespace VRCOSC.App.SDK.Nodes.Types.Flow;

[Node("Update Count", "Flow")]
public sealed class UpdateCountNode : ConstantNode<int>
{
    protected override int GetValue() => NodeScape.UpdateCount;
}