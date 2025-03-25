// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.SDK.Nodes.Types;

[Node("Branch")]
[NodeFlowInput]
[NodeFlowOutput("True", "False")]
[NodeValueInput("Condition")]
public class BranchNode : Node
{
    [NodeProcess]
    private int process(bool condition) => condition ? 0 : 1;
}

[Node("Is Equal")]
[NodeValueInput("A", "B")]
[NodeValueOutput([typeof(bool)], ["Result"])]
public class IsEqualNode : Node
{
    [NodeProcess]
    private void processBoolBool(bool input0, bool input1)
    {
        SetOutput(0, input0 == input1);
    }

    [NodeProcess]
    private void processIntInt(int input0, int input1)
    {
        SetOutput(0, input0 == input1);
    }

    [NodeProcess]
    private void processLongLong(long input0, long input1)
    {
        SetOutput(0, input0 == input1);
    }

    [NodeProcess]
    private void processFloatFloat(float input0, float input1)
    {
        SetOutput(0, Math.Abs(input0 - input1) < float.Epsilon);
    }

    [NodeProcess]
    private void processDoubleDouble(double input0, double input1)
    {
        SetOutput(0, Math.Abs(input0 - input1) < double.Epsilon);
    }

    [NodeProcess]
    private void processStringString(string input0, string input1)
    {
        SetOutput(0, input0 == input1);
    }
}