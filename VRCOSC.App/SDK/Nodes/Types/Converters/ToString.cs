// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;

namespace VRCOSC.App.SDK.Nodes.Types.Converters;

[Node("Int To String", "Converter/ToString")]
[NodeValueInput("")]
[NodeValueOutput("")]
public class IntToStringNode : Node
{
    [NodeProcess]
    private string process(int value) => value.ToString();
}

[Node("Float To String", "Converter/ToString")]
[NodeValueInput("")]
[NodeValueOutput("")]
public class FloatToStringNode : Node
{
    [NodeProcess]
    private string process(float value, CultureInfo? cultureInfo) => value.ToString(cultureInfo ?? CultureInfo.CurrentCulture);
}