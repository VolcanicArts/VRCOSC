// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.SDK.Parameters;

namespace VRCOSC.App.Nodes.Types.Parameters;

[Node("Indirect Send Parameter", "Parameters/Send")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public sealed class IndirectSendParameterNode<T> : Node, IFlowInput where T : struct
{
    public FlowContinuation Next = new("Next");

    public ValueInput<string> Name = new();
    public ValueInput<T> Value = new();

    protected override async Task Process(PulseContext c)
    {
        var name = Name.Read(c);

        if (!string.IsNullOrWhiteSpace(name))
            AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOSCConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}{name}", Value.Read(c));

        await Next.Execute(c);
    }
}

[Node("Direct Send Parameter", "Parameters/Send")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public sealed class DirectSendParameterNode<T> : Node, IFlowInput, IHasTextProperty where T : struct
{
    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public FlowContinuation Next = new("Next");

    public ValueInput<T> Value = new();

    protected override async Task Process(PulseContext c)
    {
        if (!string.IsNullOrWhiteSpace(Text))
            AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOSCConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}{Text}", Value.Read(c));

        await Next.Execute(c);
    }
}

[Node("Drive Parameter", "Parameters/Send")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public sealed class DriveParameterNode<T> : Node, IUpdateNode, IHasTextProperty where T : struct
{
    public int UpdateOffset => 2;
    public GlobalStore<T> CurrValue = new();

    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

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

[Node("Toggle Parameter", "Parameters/Send")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public sealed class ToggleParameterNode<T> : Node, IFlowInput, IHasTextProperty where T : struct
{
    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public FlowContinuation Next = new("Next");

    public ValueInput<T> ValueOn;
    public ValueInput<T> ValueOff = new("Off");

    public ToggleParameterNode()
    {
        ValueOn = new ValueInput<T>("On", defaultValue: getOnValue());
    }

    protected override async Task Process(PulseContext c)
    {
        var valueOn = ValueOn.Read(c);
        var valueOff = ValueOff.Read(c);

        if (!string.IsNullOrWhiteSpace(Text))
        {
            var parameter = await c.GetParameter<T>(Text);

            T sendValue = default!;

            if (parameter is not null)
            {
                var currentValue = parameter.GetValue<T>();

                if (EqualityComparer<T>.Default.Equals(currentValue, valueOn))
                {
                    sendValue = valueOff;
                }
                else if (EqualityComparer<T>.Default.Equals(currentValue, valueOff))
                {
                    sendValue = valueOn;
                }
                else
                {
                    sendValue = valueOn;
                }
            }

            AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOSCConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}{Text}", sendValue);
        }

        await Next.Execute(c);
    }

    private T getOnValue()
    {
        if (typeof(T) == typeof(bool))
            return (T)(object)true;

        if (typeof(T) == typeof(int))
            return (T)(object)255;

        if (typeof(T) == typeof(float))
            return (T)(object)1f;

        return default!;
    }
}

[Node("Parameter Source", "Parameters/Receive")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public sealed class ParameterSourceNode<T> : UpdateNode<T>, IHasTextProperty where T : struct
{
    public override int UpdateOffset => -2;
    public GlobalStore<T> ValueStore = new();

    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public ValueOutput<T> Value = new();

    protected override Task Process(PulseContext c)
    {
        Value.Write(ValueStore.Read(c), c);
        return Task.CompletedTask;
    }

    protected override async Task<T> GetValue(PulseContext c)
    {
        if (string.IsNullOrWhiteSpace(Text)) return default;

        var parameter = await c.GetParameter<T>(Text);
        var value = parameter?.GetValue<T>() ?? default;
        ValueStore.Write(value, c);
        return value;
    }
}

[Node("Read Parameter", "Parameters/Receive")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public sealed class ReadParameterNode<T> : Node, IFlowInput where T : struct
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
public sealed class PhysboneParameterSourceNode : UpdateNode<bool, bool, float, float, float>, IHasTextProperty
{
    public override int UpdateOffset => -2;

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

    protected override async Task<(bool, bool, float, float, float)> GetValues(PulseContext c)
    {
        var isGrabbed = false;
        var isPosed = false;
        var angle = 0f;
        var stretch = 0f;
        var squish = 0f;

        var isGrabbedParameter = await c.GetParameter<bool>($"{Text}_IsGrabbed");
        if (isGrabbedParameter is not null) isGrabbed = isGrabbedParameter.GetValue<bool>();

        var isPosedParameter = await c.GetParameter<bool>($"{Text}_IsPosed");
        if (isPosedParameter is not null) isPosed = isPosedParameter.GetValue<bool>();

        var angleParameter = await c.GetParameter<float>($"{Text}_Angle");
        if (angleParameter is not null) angle = angleParameter.GetValue<float>();

        var stretchParameter = await c.GetParameter<float>($"{Text}_Stretch");
        if (stretchParameter is not null) stretch = stretchParameter.GetValue<float>();

        var squishParameter = await c.GetParameter<float>($"{Text}_Squish");
        if (squishParameter is not null) squish = squishParameter.GetValue<float>();

        GrabbedStore.Write(isGrabbed, c);
        PosedStore.Write(isPosed, c);
        AngleStore.Write(angle, c);
        StretchStore.Write(stretch, c);
        SquishStore.Write(squish, c);

        return (isGrabbed, isPosed, angle, stretch, squish);
    }
}

[Node("Wildcard Parameter Source", "Parameters/Receive/Wildcard")]
public sealed class WildcardParameterSourceNode<T, W0> : UpdateNode<T>, IHasTextProperty where T : struct
{
    public override int UpdateOffset => -2;

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

    public GlobalStore<VRChatParameter> ParameterStore = new();

    public ValueOutput<T> Value = new();
    public ValueOutput<W0> Wildcard0 = new("Wildcard 0");

    protected override Task Process(PulseContext c)
    {
        var parameter = ParameterStore.Read(c);
        if (parameter is null) return Task.CompletedTask;

        var templatedParameter = new TemplatedVRChatParameter(textRegex, parameter);
        if (!templatedParameter.IsMatch()) return Task.CompletedTask;
        if (!templatedParameter.IsWildcardType<W0>(0)) return Task.CompletedTask;

        Value.Write(parameter.GetValue<T>(), c);
        Wildcard0.Write(templatedParameter.GetWildcard<W0>(0), c);
        return Task.CompletedTask;
    }

    protected override async Task<T> GetValue(PulseContext c)
    {
        if (string.IsNullOrWhiteSpace(Text)) return default!;

        var parameter = await c.GetParameter<T>(Text);
        if (parameter is null) return default!;

        ParameterStore.Write(parameter, c);
        return parameter.GetValue<T>();
    }
}

[Node("Wildcard Parameter Source 2", "Parameters/Receive/Wildcard")]
public sealed class WildcardParameterSourceNode<T, W0, W1> : UpdateNode<T>, IHasTextProperty where T : struct
{
    public override int UpdateOffset => -2;

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

    public GlobalStore<VRChatParameter> ParameterStore = new();

    public ValueOutput<T> Value = new();
    public ValueOutput<W0> Wildcard0 = new("Wildcard 0");
    public ValueOutput<W1> Wildcard1 = new("Wildcard 1");

    protected override Task Process(PulseContext c)
    {
        var parameter = ParameterStore.Read(c);
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

    protected override async Task<T> GetValue(PulseContext c)
    {
        if (string.IsNullOrWhiteSpace(Text)) return default!;

        var parameter = await c.GetParameter<T>(Text);
        if (parameter is null) return default!;

        ParameterStore.Write(parameter, c);
        return parameter.GetValue<T>();
    }
}

[Node("Wildcard Parameter Source 3", "Parameters/Receive/Wildcard")]
public sealed class WildcardParameterSourceNode<T, W0, W1, W2> : UpdateNode<T>, IHasTextProperty where T : struct
{
    public override int UpdateOffset => -2;

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

    public GlobalStore<VRChatParameter> ParameterStore = new();

    public ValueOutput<T> Value = new();
    public ValueOutput<W0> Wildcard0 = new("Wildcard 0");
    public ValueOutput<W1> Wildcard1 = new("Wildcard 1");
    public ValueOutput<W2> Wildcard2 = new("Wildcard 2");

    protected override Task Process(PulseContext c)
    {
        var parameter = ParameterStore.Read(c);
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

    protected override async Task<T> GetValue(PulseContext c)
    {
        if (string.IsNullOrWhiteSpace(Text)) return default!;

        var parameter = await c.GetParameter<T>(Text);
        if (parameter is null) return default!;

        ParameterStore.Write(parameter, c);
        return parameter.GetValue<T>();
    }
}

[Node("Wildcard Parameter Source 4", "Parameters/Receive/Wildcard")]
public sealed class WildcardParameterSourceNode<T, W0, W1, W2, W3> : UpdateNode<T>, IHasTextProperty where T : struct
{
    public override int UpdateOffset => -2;

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

    public GlobalStore<VRChatParameter> ParameterStore = new();

    public ValueOutput<T> Value = new();
    public ValueOutput<W0> Wildcard0 = new("Wildcard 0");
    public ValueOutput<W1> Wildcard1 = new("Wildcard 1");
    public ValueOutput<W2> Wildcard2 = new("Wildcard 2");
    public ValueOutput<W3> Wildcard3 = new("Wildcard 3");

    protected override Task Process(PulseContext c)
    {
        var parameter = ParameterStore.Read(c);
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

    protected override async Task<T> GetValue(PulseContext c)
    {
        if (string.IsNullOrWhiteSpace(Text)) return default!;

        var parameter = await c.GetParameter<T>(Text);
        if (parameter is null) return default!;

        ParameterStore.Write(parameter, c);
        return parameter.GetValue<T>();
    }
}