// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Nodes.Types;

[Node("String Value")]
[NodeValue([typeof(string)])]
public class StringTextNode : Node
{
    public Observable<string> Text { get; } = new(string.Empty);

    [NodeProcess]
    private void setOutputs()
    {
        SetOutput(0, Text.Value);
    }
}

[Node("Int Value")]
[NodeValue([typeof(int)])]
public class IntTextNode : Node
{
    public Observable<int> Int { get; } = new();

    [NodeProcess]
    private void setOutputs()
    {
        SetOutput(0, Int.Value);
    }
}

[Node("Float Value")]
[NodeValue([typeof(float)])]
public class FloatTextNode : Node
{
    public Observable<float> Float { get; } = new();

    [NodeProcess]
    private void setOutputs()
    {
        SetOutput(0, Float.Value);
    }
}