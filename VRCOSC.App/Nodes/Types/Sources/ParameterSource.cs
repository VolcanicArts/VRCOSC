// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.Parameters;

namespace VRCOSC.App.Nodes.Types.Sources;

[Node("Direct Parameter Source", "Sources")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public class DirectParameterSourceNode<T> : Node, INodeEventHandler, IHasTextProperty where T : unmanaged
{
    private readonly ParameterType parameterType = ParameterTypeFactory.CreateFrom<T>();

    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public ValueOutput<T> Output = new();

    protected override void Process(PulseContext c)
    {
        var value = string.IsNullOrEmpty(Text) ? default : c.FindParameter<T>(Text);
        Output.Write(value, c);
    }

    public bool HandleParameterReceive(PulseContext c, VRChatParameter parameter)
    {
        return string.IsNullOrEmpty(Text) && parameter.Name == Text || parameter.Type == parameterType;
    }
}

[Node("Indirect Parameter Source", "Sources")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public class IndirectParameterSourceNode<T> : Node, INodeEventHandler where T : unmanaged
{
    private readonly ParameterType parameterType = ParameterTypeFactory.CreateFrom<T>();

    public ValueInput<string> Name = new();
    public ValueOutput<T> Output = new();

    protected override void Process(PulseContext c)
    {
        var value = string.IsNullOrEmpty(Name.Read(c)) ? default : c.FindParameter<T>(Name.Read(c));
        Output.Write(value, c);
    }

    public bool HandleParameterReceive(PulseContext c, VRChatParameter parameter)
    {
        return string.IsNullOrEmpty(Name.Read(c)) && parameter.Name == Name.Read(c) || parameter.Type == parameterType;
    }
}