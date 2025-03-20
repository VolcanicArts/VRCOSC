// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;

namespace VRCOSC.App.SDK.Nodes.Types;

[NodeValue([typeof(string)])]
public class ToStringNode : Node
{
    public ToStringNode(NodeField nodeField)
        : base(nodeField)
    {
    }

    [NodeProcess]
    private void process(int value)
    {
        SetOutputValue(0, value.ToString());
    }

    [NodeProcess]
    private void process(float value)
    {
        SetOutputValue(0, value.ToString(CultureInfo.InvariantCulture));
    }
}