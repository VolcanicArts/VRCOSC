// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;

namespace VRCOSC.App.Nodes.Types.Strings;

[Node("Current Culture", "Strings/Constants")]
public sealed class CurrentCultureConstantNode : ConstantNode<CultureInfo>
{
    protected override CultureInfo GetValue() => CultureInfo.CurrentCulture;
}

[Node("Invariant Culture", "Strings/Constants")]
public sealed class InvariantCultureConstantNode : ConstantNode<CultureInfo>
{
    protected override CultureInfo GetValue() => CultureInfo.InvariantCulture;
}

[Node("String Empty", "Strings/Constants")]
public sealed class StringEmptyConstantNode : ConstantNode<string>
{
    protected override string GetValue() => string.Empty;
}

[Node("New Line", "Strings/Constants")]
public sealed class NewLineConstantNode : ConstantNode<string>
{
    protected override string GetValue() => Environment.NewLine;
}