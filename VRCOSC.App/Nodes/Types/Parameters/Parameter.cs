// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Text.RegularExpressions;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.Parameters;

namespace VRCOSC.App.Nodes.Types.Parameters;

[Node("Indirect Send Parameter", "Parameters/Send")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public class IndirectSendParameterNode<T> : Node, IFlowInput where T : unmanaged
{
    public FlowContinuation Next = new("Next");

    public ValueInput<string> Name = new();
    public ValueInput<T> Value = new();

    protected override void Process(PulseContext c)
    {
        var name = Name.Read(c);
        if (string.IsNullOrWhiteSpace(name)) return;

        AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOSCConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}{name}", Value.Read(c));
        Next.Execute(c);
    }
}

[Node("Direct Send Parameter", "Parameters/Send")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public class DirectSendParameterNode<T> : Node, IFlowInput, IHasTextProperty where T : unmanaged
{
    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public FlowContinuation Next = new("Next");

    public ValueInput<T> Value = new();

    protected override void Process(PulseContext c)
    {
        if (string.IsNullOrWhiteSpace(Text)) return;

        AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOSCConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}{Text}", Value.Read(c));
        Next.Execute(c);
    }
}

[Node("Drive Parameter", "Parameters/Send")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public class DriveParameterNode<T> : UpdateNode<T>, IHasTextProperty
{
    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public ValueInput<T> Value = new();

    protected override void Process(PulseContext c)
    {
        if (string.IsNullOrWhiteSpace(Text)) return;

        AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOSCConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}{Text}", Value.Read(c));
    }

    protected override T GetValue(PulseContext c) => Value.Read(c);
}

[Node("Parameter Source", "Parameters/Receive")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public class ParameterSourceNode<T> : Node, INodeEventHandler, IHasTextProperty where T : unmanaged
{
    private readonly ParameterType parameterType = ParameterTypeFactory.CreateFrom<T>();

    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public ValueOutput<T> Value = new();

    protected override void Process(PulseContext c)
    {
        var parameter = c.GetParameter<T>(Text);
        if (parameter is null) return;

        Value.Write(parameter.GetValue<T>(), c);
    }

    public bool HandleParameterReceive(PulseContext c, VRChatParameter parameter)
    {
        return !string.IsNullOrWhiteSpace(Text) && parameter.Name == Text && parameter.Type == parameterType;
    }
}

[Node("Read Parameter", "Parameters/Receive")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public class ReadParameterNode<T> : Node, IFlowInput where T : unmanaged
{
    private readonly ParameterType parameterType = ParameterTypeFactory.CreateFrom<T>();

    public FlowContinuation Next = new();

    public ValueInput<string> Name = new();
    public ValueOutput<T> Value = new();

    protected override void Process(PulseContext c)
    {
        var parameter = c.GetParameter<T>(Name.Read(c));
        if (parameter is null || parameter.Type != parameterType) return;

        Value.Write(parameter.GetValue<T>(), c);
        Next.Execute(c);
    }
}

[Node("Wildcard Parameter Source", "Parameters/Receive/Wildcard")]
public sealed class WildcardParameterSourceNode<T, W0> : Node, INodeEventHandler, IHasTextProperty where T : unmanaged
{
    private readonly ParameterType parameterType = ParameterTypeFactory.CreateFrom<T>();

    private string text = string.Empty;

    [NodeProperty("text")]
    public string Text
    {
        get => text;
        set
        {
            text = value;
            textRegex = TemplatedVRChatParameter.TemplateAsRegex(text);
        }
    }

    private Regex textRegex = null!;

    public GlobalStore<VRChatParameter> Parameter = new();

    public ValueOutput<T> Value = new();
    public ValueOutput<W0> Wildcard0 = new("Wildcard 0");

    protected override void Process(PulseContext c)
    {
        var parameter = Parameter.Read(c);
        if (parameter is null) return;

        var templatedParameter = new TemplatedVRChatParameter(Text, textRegex, parameter);
        if (!templatedParameter.IsMatch()) return;
        if (!templatedParameter.IsWildcardType<W0>(0)) return;

        Value.Write(parameter.GetValue<T>(), c);
        Wildcard0.Write(templatedParameter.GetWildcard<W0>(0), c);
    }

    public bool HandleParameterReceive(PulseContext c, VRChatParameter parameter)
    {
        if (string.IsNullOrWhiteSpace(Text) || parameter.Type != parameterType) return false;

        Parameter.Write(parameter, c);
        return true;
    }
}

[Node("Wildcard Parameter Source 2", "Parameters/Receive/Wildcard")]
public sealed class WildcardParameterSourceNode<T, W0, W1> : Node, INodeEventHandler, IHasTextProperty where T : unmanaged
{
    private readonly ParameterType parameterType = ParameterTypeFactory.CreateFrom<T>();

    private string text = string.Empty;

    [NodeProperty("text")]
    public string Text
    {
        get => text;
        set
        {
            text = value;
            textRegex = TemplatedVRChatParameter.TemplateAsRegex(text);
        }
    }

    private Regex textRegex = null!;

    public GlobalStore<VRChatParameter> Parameter = new();

    public ValueOutput<T> Value = new();
    public ValueOutput<W0> Wildcard0 = new("Wildcard 0");
    public ValueOutput<W1> Wildcard1 = new("Wildcard 1");

    protected override void Process(PulseContext c)
    {
        var parameter = Parameter.Read(c);
        if (parameter is null) return;

        var templatedParameter = new TemplatedVRChatParameter(Text, textRegex, parameter);
        if (!templatedParameter.IsMatch()) return;
        if (!templatedParameter.IsWildcardType<W0>(0)) return;
        if (!templatedParameter.IsWildcardType<W1>(1)) return;

        Value.Write(parameter.GetValue<T>(), c);
        Wildcard0.Write(templatedParameter.GetWildcard<W0>(0), c);
        Wildcard1.Write(templatedParameter.GetWildcard<W1>(1), c);
    }

    public bool HandleParameterReceive(PulseContext c, VRChatParameter parameter)
    {
        if (string.IsNullOrWhiteSpace(Text) || parameter.Type != parameterType) return false;

        Parameter.Write(parameter, c);
        return true;
    }
}

[Node("Wildcard Parameter Source 3", "Parameters/Receive/Wildcard")]
public sealed class WildcardParameterSourceNode<T, W0, W1, W2> : Node, INodeEventHandler, IHasTextProperty where T : unmanaged
{
    private readonly ParameterType parameterType = ParameterTypeFactory.CreateFrom<T>();

    private string text = string.Empty;

    [NodeProperty("text")]
    public string Text
    {
        get => text;
        set
        {
            text = value;
            textRegex = TemplatedVRChatParameter.TemplateAsRegex(text);
        }
    }

    private Regex textRegex = null!;

    public GlobalStore<VRChatParameter> Parameter = new();

    public ValueOutput<T> Value = new();
    public ValueOutput<W0> Wildcard0 = new("Wildcard 0");
    public ValueOutput<W1> Wildcard1 = new("Wildcard 1");
    public ValueOutput<W2> Wildcard2 = new("Wildcard 2");

    protected override void Process(PulseContext c)
    {
        var parameter = Parameter.Read(c);
        if (parameter is null) return;

        var templatedParameter = new TemplatedVRChatParameter(Text, textRegex, parameter);
        if (!templatedParameter.IsMatch()) return;
        if (!templatedParameter.IsWildcardType<W0>(0)) return;
        if (!templatedParameter.IsWildcardType<W1>(1)) return;
        if (!templatedParameter.IsWildcardType<W2>(2)) return;

        Value.Write(parameter.GetValue<T>(), c);
        Wildcard0.Write(templatedParameter.GetWildcard<W0>(0), c);
        Wildcard1.Write(templatedParameter.GetWildcard<W1>(1), c);
        Wildcard2.Write(templatedParameter.GetWildcard<W2>(2), c);
    }

    public bool HandleParameterReceive(PulseContext c, VRChatParameter parameter)
    {
        if (string.IsNullOrWhiteSpace(Text) || parameter.Type != parameterType) return false;

        Parameter.Write(parameter, c);
        return true;
    }
}

[Node("Wildcard Parameter Source 4", "Parameters/Receive/Wildcard")]
public sealed class WildcardParameterSourceNode<T, W0, W1, W2, W3> : Node, INodeEventHandler, IHasTextProperty where T : unmanaged
{
    private readonly ParameterType parameterType = ParameterTypeFactory.CreateFrom<T>();

    private string text = string.Empty;

    [NodeProperty("text")]
    public string Text
    {
        get => text;
        set
        {
            text = value;
            textRegex = TemplatedVRChatParameter.TemplateAsRegex(text);
        }
    }

    private Regex textRegex = null!;

    public GlobalStore<VRChatParameter> Parameter = new();

    public ValueOutput<T> Value = new();
    public ValueOutput<W0> Wildcard0 = new("Wildcard 0");
    public ValueOutput<W1> Wildcard1 = new("Wildcard 1");
    public ValueOutput<W2> Wildcard2 = new("Wildcard 2");
    public ValueOutput<W3> Wildcard3 = new("Wildcard 3");

    protected override void Process(PulseContext c)
    {
        var parameter = Parameter.Read(c);
        if (parameter is null) return;

        var templatedParameter = new TemplatedVRChatParameter(Text, textRegex, parameter);
        if (!templatedParameter.IsMatch()) return;
        if (!templatedParameter.IsWildcardType<W0>(0)) return;
        if (!templatedParameter.IsWildcardType<W1>(1)) return;
        if (!templatedParameter.IsWildcardType<W2>(2)) return;
        if (!templatedParameter.IsWildcardType<W3>(3)) return;

        Value.Write(parameter.GetValue<T>(), c);
        Wildcard0.Write(templatedParameter.GetWildcard<W0>(0), c);
        Wildcard1.Write(templatedParameter.GetWildcard<W1>(1), c);
        Wildcard2.Write(templatedParameter.GetWildcard<W2>(2), c);
        Wildcard3.Write(templatedParameter.GetWildcard<W3>(3), c);
    }

    public bool HandleParameterReceive(PulseContext c, VRChatParameter parameter)
    {
        if (string.IsNullOrWhiteSpace(Text) || parameter.Type != parameterType) return false;

        Parameter.Write(parameter, c);
        return true;
    }
}