// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.Parameters;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("On Registered Parameter Received", "")]
public sealed class RegisteredParameterReceivedNode<T> : Node
{
    private readonly RegisteredParameter registeredParameter;

    public FlowContinuation OnRegisteredParameterReceived;

    public LocalStore<ReceivedParameter?> Parameter = new();

    public ValueOutput<T> Value = new();

    public RegisteredParameterReceivedNode(RegisteredParameter registeredParameter)
    {
        this.registeredParameter = registeredParameter;

        OnRegisteredParameterReceived = new($"On {registeredParameter.Name} Received");
    }

    protected override void Process(PulseContext c)
    {
        Value.Write(Parameter.Read(c)!.GetValue<T>(), c);
        OnRegisteredParameterReceived.Execute(c);
    }

    protected override bool ShouldProcess(PulseContext c)
    {
        return Parameter.Read(c) is not null;
    }

    public void OnParameterReceived(PulseContext c, ReceivedParameter parameter)
    {
        // TODO: Validation

        Parameter.Write(parameter, c);
    }
}

[Node("On Parameter Received", "Flow")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public sealed class OnParameterReceivedNode<T> : Node where T : struct
{
    private readonly ParameterType parameterType = ParameterTypeFactory.CreateFrom<T>();

    public FlowContinuation OnReceived = new("On Received");

    public LocalStore<ReceivedParameter> Parameter = new();

    public ValueInput<string> Name = new(string.Empty);
    public ValueOutput<T> Value = new();

    protected override void Process(PulseContext c)
    {
        Value.Write(Parameter.Read(c).GetValue<T>(), c);
        OnReceived.Execute(c);
    }

    protected override bool ShouldProcess(PulseContext c)
    {
        return Parameter.Read(c) is not null;
    }

    public void OnParameterReceived(PulseContext c, ReceivedParameter parameter)
    {
        var name = Name.Read(c);
        if (string.IsNullOrEmpty(name) || parameter.Name != name || parameter.Type != parameterType) return;

        Parameter.Write(parameter, c);
    }
}