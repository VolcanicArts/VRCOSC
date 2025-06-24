// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;

namespace VRCOSC.App.Nodes.Types.Strings;

[Node("Current Culture", "Strings")]
public sealed class CurrentCultureValueOutputNode : ValueOutputNode<CultureInfo>
{
    protected override CultureInfo GetValue() => CultureInfo.CurrentCulture;
}

[Node("Invariant Culture", "Strings")]
public sealed class InvariantCultureValueOutputNode : ValueOutputNode<CultureInfo>
{
    protected override CultureInfo GetValue() => CultureInfo.InvariantCulture;
}