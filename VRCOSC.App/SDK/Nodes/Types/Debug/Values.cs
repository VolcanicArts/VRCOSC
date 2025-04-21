// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;

namespace VRCOSC.App.SDK.Nodes.Types.Debug;

[Node("List Output Test", "Debug")]
public class ListOutputTestNode : Node
{
    [NodeProcess([""], ["String List"])]
    private List<string> process() => ["Test1", "Test2", "Test3"];
}

[Node("Passthrough List Test", "Debug")]
public class PassthroughListTestNode<T> : Node
{
    [NodeProcess([""], [""])]
    private IEnumerable<T> process(IEnumerable<T> enumerable) => enumerable;
}