// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.SDK.Nodes.Types.Operators;

[Node("Bool Not Equals", "Operators/Equals")]
[NodeValueInput("", "")]
[NodeValueOutput("")]
public class BoolNotEqualsNode : Node
{
    [NodeProcess]
    private bool process(bool a, bool b) => a != b;
}

[Node("Int Not Equals", "Operators/Equals")]
[NodeValueInput("", "")]
[NodeValueOutput("")]
public class IntNotEqualsNode : Node
{
    [NodeProcess]
    private bool process(int a, int b) => a != b;
}

[Node("Float Not Equals", "Operators/Equals")]
[NodeValueInput("", "")]
[NodeValueOutput("")]
public class FloatNotEqualsNode : Node
{
    [NodeProcess]
    private bool process(float a, float b, float? tolerance) => !(Math.Abs(a - b) < (tolerance ?? float.Epsilon));
}

[Node("String Not Equals", "Operators/Equals")]
[NodeValueInput("", "")]
[NodeValueOutput("")]
public class StringNotEqualsNode : Node
{
    [NodeProcess]
    private bool process(string a, string b, StringComparison? comparison) => !a.Equals(b, comparison ?? StringComparison.Ordinal);
}