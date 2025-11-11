// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.SDK.Parameters;

namespace VRCOSC.App.Nodes.Types.Parameters;

[Node("Indirect Send Parameter", "Parameters/Send")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public sealed class IndirectSendParameterNode<T> : Node, IFlowInput where T : unmanaged
{
    public FlowContinuation Next = new("Next");

    public ValueInput<string> Name = new();
    public ValueInput<T> Value = new();

    protected override async Task Process(PulseContext c)
    {
        var name = Name.Read(c);
        if (string.IsNullOrWhiteSpace(name)) return;

        AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOSCConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}{name}", Value.Read(c));
        await Next.Execute(c);
    }
}

[Node("Direct Send Parameter", "Parameters/Send")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public sealed class DirectSendParameterNode<T> : Node, IFlowInput, IHasTextProperty
{
    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public FlowContinuation Next = new("Next");

    public ValueInput<T> Value = new();

    protected override async Task Process(PulseContext c)
    {
        if (string.IsNullOrWhiteSpace(Text)) return;

        AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOSCConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}{Text}", Value.Read(c));
        await Next.Execute(c);
    }
}

[Node("Drive Parameter", "Parameters/Send")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public sealed class DriveParameterNode<T> : Node, IUpdateNode, IHasTextProperty
{
    public GlobalStore<T> CurrValue = new();

    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    [NodeReactive]
    public ValueInput<T> Value = new();

    protected override Task Process(PulseContext c)
    {
        CurrValue.Write(Value.Read(c), c);
        return Task.CompletedTask;
    }

    public void OnUpdate(PulseContext c)
    {
        if (!string.IsNullOrWhiteSpace(Text))
            AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOSCConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}{Text}", CurrValue.Read(c));
    }
}

[Node("Parameter Source", "Parameters/Receive")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public sealed class ParameterSourceNode<T> : Node, INodeEventHandler, IHasTextProperty
{
    private readonly ParameterType parameterType = ParameterTypeFactory.CreateFrom<T>();

    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public ValueOutput<T> Value = new();

    protected override async Task Process(PulseContext c)
    {
        var parameter = await c.GetParameter<T>(Text);
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
public sealed class ReadParameterNode<T> : Node, IFlowInput
{
    private readonly ParameterType parameterType = ParameterTypeFactory.CreateFrom<T>();

    public FlowContinuation Next = new();

    public ValueInput<string> Name = new();
    public ValueOutput<T> Value = new();

    protected override async Task Process(PulseContext c)
    {
        var parameter = await c.GetParameter<T>(Name.Read(c));
        if (parameter is null || parameter.Type != parameterType) return;

        Value.Write(parameter.GetValue<T>(), c);
        await Next.Execute(c);
    }
}

[Node("Physbone Parameter Source", "Parameters/Receive")]
public sealed class PhysboneParameterSourceNode : Node, INodeEventHandler, IHasTextProperty
{
    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public GlobalStore<bool> GrabbedStore = new();
    public GlobalStore<bool> PosedStore = new();
    public GlobalStore<float> AngleStore = new();
    public GlobalStore<float> StretchStore = new();
    public GlobalStore<float> SquishStore = new();

    public ValueOutput<bool> Grabbed = new();
    public ValueOutput<bool> Posed = new();
    public ValueOutput<float> Angle = new();
    public ValueOutput<float> Stretch = new();
    public ValueOutput<float> Squish = new();

    protected override Task Process(PulseContext c)
    {
        Grabbed.Write(GrabbedStore.Read(c), c);
        Posed.Write(PosedStore.Read(c), c);
        Angle.Write(AngleStore.Read(c), c);
        Stretch.Write(StretchStore.Read(c), c);
        Squish.Write(SquishStore.Read(c), c);
        return Task.CompletedTask;
    }

    public bool HandleParameterReceive(PulseContext c, VRChatParameter parameter)
    {
        if (string.IsNullOrWhiteSpace(Text)) return false;

        if (parameter.Name == $"{Text}_IsGrabbed" && parameter.Type == ParameterType.Bool)
        {
            GrabbedStore.Write(parameter.GetValue<bool>(), c);
            return true;
        }

        if (parameter.Name == $"{Text}_IsPosed" && parameter.Type == ParameterType.Bool)
        {
            PosedStore.Write(parameter.GetValue<bool>(), c);
            return true;
        }

        if (parameter.Name == $"{Text}_Angle" && parameter.Type == ParameterType.Float)
        {
            AngleStore.Write(parameter.GetValue<float>(), c);
            return true;
        }

        if (parameter.Name == $"{Text}_Stretch" && parameter.Type == ParameterType.Float)
        {
            StretchStore.Write(parameter.GetValue<float>(), c);
            return true;
        }

        if (parameter.Name == $"{Text}_Squish" && parameter.Type == ParameterType.Float)
        {
            SquishStore.Write(parameter.GetValue<float>(), c);
            return true;
        }

        return false;
    }
}

[Node("Wildcard Parameter Source", "Parameters/Receive/Wildcard")]
public sealed class WildcardParameterSourceNode<T, W0> : Node, INodeEventHandler, IHasTextProperty
{
    private readonly ParameterType parameterType = ParameterTypeFactory.CreateFrom<T>();

    [NodeProperty("text")]
    public string Text
    {
        get;
        set
        {
            field = value;
            textRegex = TemplatedVRChatParameter.TemplateAsRegex(field);
        }
    } = string.Empty;

    private Regex textRegex = null!;

    public GlobalStore<VRChatParameter> Parameter = new();

    public ValueOutput<T> Value = new();
    public ValueOutput<W0> Wildcard0 = new("Wildcard 0");

    protected override Task Process(PulseContext c)
    {
        var parameter = Parameter.Read(c);
        if (parameter is null) return Task.CompletedTask;

        var templatedParameter = new TemplatedVRChatParameter(textRegex, parameter);
        if (!templatedParameter.IsMatch()) return Task.CompletedTask;
        if (!templatedParameter.IsWildcardType<W0>(0)) return Task.CompletedTask;

        Value.Write(parameter.GetValue<T>(), c);
        Wildcard0.Write(templatedParameter.GetWildcard<W0>(0), c);
        return Task.CompletedTask;
    }

    public bool HandleParameterReceive(PulseContext c, VRChatParameter parameter)
    {
        if (string.IsNullOrWhiteSpace(Text) || parameter.Type != parameterType) return false;

        Parameter.Write(parameter, c);
        return true;
    }
}

[Node("Wildcard Parameter Source 2", "Parameters/Receive/Wildcard")]
public sealed class WildcardParameterSourceNode<T, W0, W1> : Node, INodeEventHandler, IHasTextProperty
{
    private readonly ParameterType parameterType = ParameterTypeFactory.CreateFrom<T>();

    [NodeProperty("text")]
    public string Text
    {
        get;
        set
        {
            field = value;
            textRegex = TemplatedVRChatParameter.TemplateAsRegex(field);
        }
    } = string.Empty;

    private Regex textRegex = null!;

    public GlobalStore<VRChatParameter> Parameter = new();

    public ValueOutput<T> Value = new();
    public ValueOutput<W0> Wildcard0 = new("Wildcard 0");
    public ValueOutput<W1> Wildcard1 = new("Wildcard 1");

    protected override Task Process(PulseContext c)
    {
        var parameter = Parameter.Read(c);
        if (parameter is null) return Task.CompletedTask;

        var templatedParameter = new TemplatedVRChatParameter(textRegex, parameter);
        if (!templatedParameter.IsMatch()) return Task.CompletedTask;
        if (!templatedParameter.IsWildcardType<W0>(0)) return Task.CompletedTask;
        if (!templatedParameter.IsWildcardType<W1>(1)) return Task.CompletedTask;

        Value.Write(parameter.GetValue<T>(), c);
        Wildcard0.Write(templatedParameter.GetWildcard<W0>(0), c);
        Wildcard1.Write(templatedParameter.GetWildcard<W1>(1), c);
        return Task.CompletedTask;
    }

    public bool HandleParameterReceive(PulseContext c, VRChatParameter parameter)
    {
        if (string.IsNullOrWhiteSpace(Text) || parameter.Type != parameterType) return false;

        Parameter.Write(parameter, c);
        return true;
    }
}

[Node("Wildcard Parameter Source 3", "Parameters/Receive/Wildcard")]
public sealed class WildcardParameterSourceNode<T, W0, W1, W2> : Node, INodeEventHandler, IHasTextProperty
{
    private readonly ParameterType parameterType = ParameterTypeFactory.CreateFrom<T>();

    [NodeProperty("text")]
    public string Text
    {
        get;
        set
        {
            field = value;
            textRegex = TemplatedVRChatParameter.TemplateAsRegex(field);
        }
    } = string.Empty;

    private Regex textRegex = null!;

    public GlobalStore<VRChatParameter> Parameter = new();

    public ValueOutput<T> Value = new();
    public ValueOutput<W0> Wildcard0 = new("Wildcard 0");
    public ValueOutput<W1> Wildcard1 = new("Wildcard 1");
    public ValueOutput<W2> Wildcard2 = new("Wildcard 2");

    protected override Task Process(PulseContext c)
    {
        var parameter = Parameter.Read(c);
        if (parameter is null) return Task.CompletedTask;

        var templatedParameter = new TemplatedVRChatParameter(textRegex, parameter);
        if (!templatedParameter.IsMatch()) return Task.CompletedTask;
        if (!templatedParameter.IsWildcardType<W0>(0)) return Task.CompletedTask;
        if (!templatedParameter.IsWildcardType<W1>(1)) return Task.CompletedTask;
        if (!templatedParameter.IsWildcardType<W2>(2)) return Task.CompletedTask;

        Value.Write(parameter.GetValue<T>(), c);
        Wildcard0.Write(templatedParameter.GetWildcard<W0>(0), c);
        Wildcard1.Write(templatedParameter.GetWildcard<W1>(1), c);
        Wildcard2.Write(templatedParameter.GetWildcard<W2>(2), c);
        return Task.CompletedTask;
    }

    public bool HandleParameterReceive(PulseContext c, VRChatParameter parameter)
    {
        if (string.IsNullOrWhiteSpace(Text) || parameter.Type != parameterType) return false;

        Parameter.Write(parameter, c);
        return true;
    }
}

[Node("Wildcard Parameter Source 4", "Parameters/Receive/Wildcard")]
public sealed class WildcardParameterSourceNode<T, W0, W1, W2, W3> : Node, INodeEventHandler, IHasTextProperty
{
    private readonly ParameterType parameterType = ParameterTypeFactory.CreateFrom<T>();

    [NodeProperty("text")]
    public string Text
    {
        get;
        set
        {
            field = value;
            textRegex = TemplatedVRChatParameter.TemplateAsRegex(field);
        }
    } = string.Empty;

    private Regex textRegex = null!;

    public GlobalStore<VRChatParameter> Parameter = new();

    public ValueOutput<T> Value = new();
    public ValueOutput<W0> Wildcard0 = new("Wildcard 0");
    public ValueOutput<W1> Wildcard1 = new("Wildcard 1");
    public ValueOutput<W2> Wildcard2 = new("Wildcard 2");
    public ValueOutput<W3> Wildcard3 = new("Wildcard 3");

    protected override Task Process(PulseContext c)
    {
        var parameter = Parameter.Read(c);
        if (parameter is null) return Task.CompletedTask;

        var templatedParameter = new TemplatedVRChatParameter(textRegex, parameter);
        if (!templatedParameter.IsMatch()) return Task.CompletedTask;
        if (!templatedParameter.IsWildcardType<W0>(0)) return Task.CompletedTask;
        if (!templatedParameter.IsWildcardType<W1>(1)) return Task.CompletedTask;
        if (!templatedParameter.IsWildcardType<W2>(2)) return Task.CompletedTask;
        if (!templatedParameter.IsWildcardType<W3>(3)) return Task.CompletedTask;

        Value.Write(parameter.GetValue<T>(), c);
        Wildcard0.Write(templatedParameter.GetWildcard<W0>(0), c);
        Wildcard1.Write(templatedParameter.GetWildcard<W1>(1), c);
        Wildcard2.Write(templatedParameter.GetWildcard<W2>(2), c);
        Wildcard3.Write(templatedParameter.GetWildcard<W3>(3), c);
        return Task.CompletedTask;
    }

    public bool HandleParameterReceive(PulseContext c, VRChatParameter parameter)
    {
        if (string.IsNullOrWhiteSpace(Text) || parameter.Type != parameterType) return false;

        Parameter.Write(parameter, c);
        return true;
    }
}