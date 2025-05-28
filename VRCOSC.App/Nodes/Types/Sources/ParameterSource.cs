// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.Parameters;

namespace VRCOSC.App.Nodes.Types.Sources;

[Node("Direct Parameter Source", "Sources")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public class DirectParameterSourceNode<T> : Node, IParameterHandler where T : unmanaged
{
    private readonly ParameterType parameterType = ParameterTypeFactory.CreateFrom<T>();

    [NodeProperty("name")]
    public string Name { get; set; } = null!;

    public ValueOutput<T> Output = new();

    protected override void Process(PulseContext c)
    {
        var value = string.IsNullOrEmpty(Name) ? default : c.FindParameter<T>(Name);
        Output.Write(value, c);
    }

    public bool HandlesParameter(PulseContext c, VRChatParameter parameter)
    {
        return string.IsNullOrEmpty(Name) && parameter.Name == Name || parameter.Type == parameterType;
    }
}

[Node("Indirect Parameter Source", "Sources")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public class IndirectParameterSourceNode<T> : Node, IParameterHandler where T : unmanaged
{
    private readonly ParameterType parameterType = ParameterTypeFactory.CreateFrom<T>();

    public ValueInput<string> Name = new();
    public ValueOutput<T> Output = new();

    protected override void Process(PulseContext c)
    {
        var value = string.IsNullOrEmpty(Name.Read(c)) ? default : c.FindParameter<T>(Name.Read(c));
        Output.Write(value, c);
    }

    public bool HandlesParameter(PulseContext c, VRChatParameter parameter)
    {
        return string.IsNullOrEmpty(Name.Read(c)) && parameter.Name == Name.Read(c) || parameter.Type == parameterType;
    }
}