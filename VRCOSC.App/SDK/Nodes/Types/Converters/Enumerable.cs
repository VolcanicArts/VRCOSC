// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;

namespace VRCOSC.App.SDK.Nodes.Types.Converters;

[Node("Element At", "Enumerable")]
public sealed class ElementAtNode<T> : Node
{
    [NodeProcess(["Enumerable", "Index"], ["Element"])]
    private T process(IEnumerable<T> enumerable, int index) => enumerable.ElementAt(index);
}

[Node("Count", "Enumerable")]
public class EnumerableCountNode<T> : Node
{
    [NodeProcess(["Enumerable"], ["Count"])]
    private int process(IEnumerable<T> enumerable) => enumerable.Count();
}