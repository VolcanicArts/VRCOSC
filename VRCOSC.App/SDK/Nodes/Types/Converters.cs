// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace VRCOSC.App.SDK.Nodes.Types;

[Node("ToString")]
[NodeValueInput("")]
[NodeValueOutput([typeof(string)], ["String"])]
public class ToStringNode : Node
{
    [NodeProcess]
    private void processInt(int value)
    {
        SetOutput(0, value.ToString());
    }

    [NodeProcess]
    private void processFloat(float value)
    {
        SetOutput(0, value.ToString(CultureInfo.InvariantCulture));
    }
}

[Node("Element At")]
[NodeValueInput("Enumerable", "Index")]
[NodeValueOutput([typeof(object)], ["Element"])]
public class ElementAtNode : Node
{
    [NodeProcess]
    private void process(IEnumerable<object> enumerable, int index)
    {
        SetOutput(0, enumerable.ElementAt(index));
    }
}