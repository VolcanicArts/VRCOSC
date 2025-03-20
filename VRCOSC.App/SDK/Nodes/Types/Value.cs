// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Nodes.Types;

[NodeValue([typeof(string)])]
public class StringTextNode : Node
{
    public Observable<string> Text { get; } = new(string.Empty);

    public StringTextNode(NodeField nodeField)
        : base(nodeField)
    {
    }

    [NodeProcess]
    private void setOutputs()
    {
        SetOutputValue(0, Text.Value);
    }
}

[NodeValue([typeof(int)])]
public class IntTextNode : Node
{
    public Observable<int> Int { get; } = new();

    public IntTextNode(NodeField nodeField)
        : base(nodeField)
    {
    }

    [NodeProcess]
    private void setOutputs()
    {
        SetOutputValue(0, Int.Value);
    }
}

[NodeValue([typeof(float)])]
public class FloatTextNode : Node
{
    public Observable<float> Float { get; } = new();

    public FloatTextNode(NodeField nodeField)
        : base(nodeField)
    {
    }

    [NodeProcess]
    private void setOutputs()
    {
        SetOutputValue(0, Float.Value);
    }
}