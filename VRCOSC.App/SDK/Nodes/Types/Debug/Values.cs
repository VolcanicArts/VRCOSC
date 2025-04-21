// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;

namespace VRCOSC.App.SDK.Nodes.Types.Debug;

[Node("List Output Test", "Debug")]
[NodeValueOutput("String List")]
public class ListOutputTestNode : Node
{
    [NodeProcess]
    private List<string> process() => ["Test1", "Test2", "Test3"];
}

[Node("Passthrough List Test", "Debug")]
[NodeValueInput("String List")]
[NodeValueOutput("String List")]
public class PassthroughListTestNode : Node
{
    [NodeProcess]
    private IEnumerable<T> process<T>(IEnumerable<T> enumerable) => enumerable;
}