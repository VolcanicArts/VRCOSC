// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;

namespace VRCOSC.App.SDK.Nodes.Types;

[Node("ToString")]
[NodeValue([typeof(string)])]
[NodeInputs("")]
public class ToStringNode : Node
{
    [NodeProcess]
    private void process(int value)
    {
        SetOutput(0, value.ToString());
    }

    [NodeProcess]
    private void process(float value)
    {
        SetOutput(0, value.ToString(CultureInfo.InvariantCulture));
    }
}