// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Maths;

[Node("Int Add", "Maths/Add")]
[NodeValueInput("", "")]
[NodeValueOutput("")]
public class IntAddNode : Node
{
    [NodeProcess]
    private float process(float a, float b) => a + b;
}

[Node("Float Add", "Maths/Add")]
[NodeValueInput("", "")]
[NodeValueOutput("")]
public class FloatAddNode : Node
{
    [NodeProcess]
    private float process(float a, float b) => a + b;
}