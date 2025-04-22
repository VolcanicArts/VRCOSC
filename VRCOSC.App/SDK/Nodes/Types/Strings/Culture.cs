// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;

namespace VRCOSC.App.SDK.Nodes.Types.Strings;

[Node("Current Culture", "Strings/Culture")]
public sealed class CurrentCultureConstantNode : ConstantNode<CultureInfo>
{
    public CurrentCultureConstantNode()
    {
        Value = CultureInfo.CurrentCulture;
    }
}

[Node("Invariant Culture", "Strings/Culture")]
public sealed class InvariantCultureConstantNode : ConstantNode<CultureInfo>
{
    public InvariantCultureConstantNode()
    {
        Value = CultureInfo.InvariantCulture;
    }
}