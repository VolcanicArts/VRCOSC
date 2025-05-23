// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.Parameters;

namespace VRCOSC.App.Nodes.Types.Sources;

[Node("Parameter Source", "Sources")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public class ParameterSourceNode<T> : Node, IParameterSource where T : unmanaged
{
    public string Name { get; set; } = "VRCOSC/Media/Play";

    private T value;

    public ValueOutput<T> Output = new();

    protected override void Process(PulseContext c)
    {
        Output.Write(value, c);
    }

    public void OnParameterReceived(ReceivedParameter parameter)
    {
        value = parameter.GetValue<T>();
    }
}