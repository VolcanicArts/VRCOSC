// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;

namespace VRCOSC.App.SDK.Nodes.Types.Converters;

[Node("Element At", "Enumerable")]
[NodeValueInput("Enumerable", "Index")]
[NodeValueOutput("Element")]
public class ElementAtNode : Node
{
    [NodeProcess]
    private T process<T>(IEnumerable<T> enumerable, int index) => enumerable.ElementAt(index);
}

[Node("Length", "Enumerable")]
[NodeValueInput("Enumerable")]
[NodeValueOutput("Length")]
public class EnumerableLengthNode : Node
{
    [NodeProcess]
    private int process<T>(IEnumerable<T> enumerable) => enumerable.Count();
}