// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;
using VRCOSC.App.SDK.Nodes.Types.Base;

namespace VRCOSC.App.SDK.Nodes.Types.Strings;

[Node("Current Culture", "Strings")]
public sealed class CurrentCultureConstantNode : ConstantNode<CultureInfo>
{
    protected override CultureInfo GetValue() => CultureInfo.CurrentCulture;
}

[Node("Invariant Culture", "Strings")]
public sealed class InvariantCultureConstantNode : ConstantNode<CultureInfo>
{
    protected override CultureInfo GetValue() => CultureInfo.InvariantCulture;
}