// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.Parameters;

namespace VRCOSC.App.Nodes.Types.Sources;

[Node("Parameter Source", "Sources")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public class ParameterSourceNode<T> : Node, IParameterHandler where T : unmanaged
{
    private readonly ParameterType parameterType = ParameterTypeFactory.CreateFrom<T>();

    public string Name { get; set; } = null!;

    public ValueOutput<T> Output = new();

    protected override void Process(PulseContext c)
    {
        var value = string.IsNullOrEmpty(Name) ? default : c.GetParameter<T>(Name);
        Output.Write(value, c);
    }

    public bool HandlesParameter(ReceivedParameter parameter)
    {
        return string.IsNullOrEmpty(Name) && parameter.Name == Name || parameter.Type == parameterType;
    }
}