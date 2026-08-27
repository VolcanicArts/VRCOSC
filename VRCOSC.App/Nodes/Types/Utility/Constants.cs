// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Utility;

[Node("Null Constant", "Utility")]
public sealed class NullConstantNode<T>() : ConstantNode<T>(null!) where T : class
{
    public override string DisplayName => $"Null {typeof(T).GetFriendlyName()}";
}