// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.SDK.Parameters;

namespace VRCOSC.App.Nodes.Types.Parameters;

[Node("Indirect Send Parameter", "Parameters/Send")]
[NodeGenericTypeFilter(typeof(bool), typeof(int), typeof(float))]
public sealed class IndirectSendParameterNode<T> : ActionNode where T : struct
{
    public ValueInput<string> Name = new();
    public ValueInput<T> Value = new();

    protected override void DoAction(PulseContext c)
    {
        var name = Name.Read(c);

        if (!string.IsNullOrWhiteSpace(name))
            AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOSCConstants.ADDRESS_AVATAR_PARAMETERS}/{name}", Value.Read(c));
    }
}

[Node("Direct Send Parameter", "Parameters/Send")]
[NodeGenericTypeFilter(typeof(bool), typeof(int), typeof(float))]
public sealed class DirectSendParameterNode<T> : ActionValueConsumeNode<T>, IHasTextProperty where T : struct
{
    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    protected override void ConsumeValue(T value, PulseContext c)
    {
        if (!string.IsNullOrWhiteSpace(Text))
            AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOSCConstants.ADDRESS_AVATAR_PARAMETERS}/{Text}", value);
    }
}

[Node("Drive Parameter", "Parameters/Send")]
[NodeGenericTypeFilter(typeof(bool), typeof(int), typeof(float))]
public sealed class DriveParameterNode<T> : ValueConsumeNode<T>, IUpdateNode, IHasTextProperty where T : struct
{
    public int UpdateOffset => 2;
    public GlobalStore<T> CurrValue = new();

    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    protected override void ConsumeValue(T value, PulseContext c) => CurrValue.Write(value, c);

    public void OnUpdate(PulseContext c)
    {
        if (!string.IsNullOrWhiteSpace(Text))
            AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOSCConstants.ADDRESS_AVATAR_PARAMETERS}/{Text}", CurrValue.Read(c));
    }
}

[Node("Toggle Parameter", "Parameters/Send")]
[NodeGenericTypeFilter(typeof(bool), typeof(int), typeof(float))]
public sealed class ToggleParameterNode<T> : ActionNode, IHasTextProperty where T : struct
{
    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public ValueInput<T> ValueOn;
    public ValueInput<T> ValueOff = new("Off");

    public ToggleParameterNode()
    {
        ValueOn = new ValueInput<T>("On", defaultValue: getOnValue());
    }

    protected override void DoAction(PulseContext c)
    {
        var valueOn = ValueOn.Read(c);
        var valueOff = ValueOff.Read(c);

        if (string.IsNullOrWhiteSpace(Text)) return;

        var parameter = c.GetParameter<T>(Text);

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

        AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOSCConstants.ADDRESS_AVATAR_PARAMETERS}/{Text}", sendValue);
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
[NodeGenericTypeFilter(typeof(bool), typeof(int), typeof(float))]
public sealed class ParameterSourceNode<T>() : ValueSourceNode<T>("Value"), IHasTextProperty where T : struct
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

    protected override T ComputeValue(PulseContext c)
    {
        if (string.IsNullOrWhiteSpace(Text)) return default;

        return c.GetParameter<T>(textRegex)?.GetValue<T>() ?? default;
    }
}

[Node("Read Parameter")]
[NodeGenericTypeFilter(typeof(bool), typeof(int), typeof(float))]
public sealed class ReadParameterNode<T>() : ActionValueTransformNode<string?, T>("Name", "Value") where T : struct
{
    protected override T TransformValue(string? name, PulseContext c)
    {
        if (string.IsNullOrEmpty(name)) return default;

        return c.GetParameter<T>(name)?.GetValue<T>() ?? default;
    }
}

[Node("Physbone Parameter Source", "Parameters/Receive")]
public sealed class PhysboneParameterSourceNode : Node, IContinuousNode, IHasTextProperty
{
    public int UpdateOffset => -2;

    [NodeProperty("text")]
    public string Text
    {
        get;
        set
        {
            field = value;
            grabbedRegex = TemplatedVRChatParameter.TemplateAsRegex($"{field}_IsGrabbed");
            posedRegex = TemplatedVRChatParameter.TemplateAsRegex($"{field}_IsPosed");
            angleRegex = TemplatedVRChatParameter.TemplateAsRegex($"{field}_Angle");
            stretchRegex = TemplatedVRChatParameter.TemplateAsRegex($"{field}_Stretch");
            squishRegex = TemplatedVRChatParameter.TemplateAsRegex($"{field}_Squish");
        }
    } = string.Empty;

    private Regex grabbedRegex = null!;
    private Regex posedRegex = null!;
    private Regex angleRegex = null!;
    private Regex stretchRegex = null!;
    private Regex squishRegex = null!;

    public ValueOutput<bool> Grabbed = new();
    public ValueOutput<bool> Posed = new();
    public ValueOutput<float> Angle = new();
    public ValueOutput<float> Stretch = new();
    public ValueOutput<float> Squish = new();

    protected override Task Process(PulseContext c)
    {
        if (string.IsNullOrWhiteSpace(Text)) return Task.CompletedTask;

        Grabbed.Write(c.GetParameter<bool>(grabbedRegex)?.GetValue<bool>() ?? false, c);
        Posed.Write(c.GetParameter<bool>(posedRegex)?.GetValue<bool>() ?? false, c);
        Angle.Write(c.GetParameter<float>(angleRegex)?.GetValue<float>() ?? 0f, c);
        Stretch.Write(c.GetParameter<float>(stretchRegex)?.GetValue<float>() ?? 0f, c);
        Squish.Write(c.GetParameter<float>(squishRegex)?.GetValue<float>() ?? 0f, c);
        return Task.CompletedTask;
    }
}

[Node("Raycast Parameter Source", "Parameters/Receive")]
public sealed class RaycastParameterSourceNode : Node, IContinuousNode, IHasTextProperty
{
    public int UpdateOffset => -2;

    [NodeProperty("text")]
    public string Text
    {
        get;
        set
        {
            field = value;
            hitRegex = TemplatedVRChatParameter.TemplateAsRegex($"{field}_Hit");
            ratioRegex = TemplatedVRChatParameter.TemplateAsRegex($"{field}_Ratio");
            distanceRegex = TemplatedVRChatParameter.TemplateAsRegex($"{field}_Distance");
        }
    } = string.Empty;

    private Regex hitRegex = null!;
    private Regex ratioRegex = null!;
    private Regex distanceRegex = null!;

    public ValueOutput<bool> Hit = new();
    public ValueOutput<float> Ratio = new();
    public ValueOutput<float> Distance = new();

    protected override Task Process(PulseContext c)
    {
        if (string.IsNullOrWhiteSpace(Text)) return Task.CompletedTask;

        Hit.Write(c.GetParameter<bool>(hitRegex)?.GetValue<bool>() ?? false, c);
        Ratio.Write(c.GetParameter<float>(ratioRegex)?.GetValue<float>() ?? 0f, c);
        Distance.Write(c.GetParameter<float>(distanceRegex)?.GetValue<float>() ?? 0f, c);
        return Task.CompletedTask;
    }
}

[Node("Wildcard Parameter Source", "Parameters/Receive/Wildcard")]
public sealed class WildcardParameterSourceNode<T, W0> : Node, IContinuousNode, IHasTextProperty where T : struct
{
    public int UpdateOffset => -2;

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

    public ValueOutput<T> Value = new();
    public ValueOutput<W0> Wildcard0 = new("Wildcard 0");

    protected override Task Process(PulseContext c)
    {
        if (string.IsNullOrWhiteSpace(Text)) return Task.CompletedTask;

        var parameter = c.GetParameter<T>(textRegex);
        if (parameter is null) return Task.CompletedTask;
        if (!parameter.IsWildcardType<W0>(0)) return Task.CompletedTask;

        Value.Write(parameter.GetValue<T>(), c);
        Wildcard0.Write(parameter.GetWildcard<W0>(0), c);
        return Task.CompletedTask;
    }
}

[Node("Wildcard Parameter Source 2", "Parameters/Receive/Wildcard")]
public sealed class WildcardParameterSourceNode<T, W0, W1> : Node, IContinuousNode, IHasTextProperty where T : struct
{
    public int UpdateOffset => -2;

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

    public ValueOutput<T> Value = new();
    public ValueOutput<W0> Wildcard0 = new("Wildcard 0");
    public ValueOutput<W1> Wildcard1 = new("Wildcard 1");

    protected override Task Process(PulseContext c)
    {
        if (string.IsNullOrWhiteSpace(Text)) return Task.CompletedTask;

        var parameter = c.GetParameter<T>(textRegex);
        if (parameter is null) return Task.CompletedTask;
        if (!parameter.IsWildcardType<W0>(0)) return Task.CompletedTask;
        if (!parameter.IsWildcardType<W1>(1)) return Task.CompletedTask;

        Value.Write(parameter.GetValue<T>(), c);
        Wildcard0.Write(parameter.GetWildcard<W0>(0), c);
        Wildcard1.Write(parameter.GetWildcard<W1>(1), c);
        return Task.CompletedTask;
    }
}

[Node("Wildcard Parameter Source 3", "Parameters/Receive/Wildcard")]
public sealed class WildcardParameterSourceNode<T, W0, W1, W2> : Node, IContinuousNode, IHasTextProperty where T : struct
{
    public int UpdateOffset => -2;

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

    public ValueOutput<T> Value = new();
    public ValueOutput<W0> Wildcard0 = new("Wildcard 0");
    public ValueOutput<W1> Wildcard1 = new("Wildcard 1");
    public ValueOutput<W2> Wildcard2 = new("Wildcard 2");

    protected override Task Process(PulseContext c)
    {
        if (string.IsNullOrWhiteSpace(Text)) return Task.CompletedTask;

        var parameter = c.GetParameter<T>(textRegex);
        if (parameter is null) return Task.CompletedTask;
        if (!parameter.IsWildcardType<W0>(0)) return Task.CompletedTask;
        if (!parameter.IsWildcardType<W1>(1)) return Task.CompletedTask;
        if (!parameter.IsWildcardType<W2>(2)) return Task.CompletedTask;

        Value.Write(parameter.GetValue<T>(), c);
        Wildcard0.Write(parameter.GetWildcard<W0>(0), c);
        Wildcard1.Write(parameter.GetWildcard<W1>(1), c);
        Wildcard2.Write(parameter.GetWildcard<W2>(2), c);
        return Task.CompletedTask;
    }
}

[Node("Wildcard Parameter Source 4", "Parameters/Receive/Wildcard")]
public sealed class WildcardParameterSourceNode<T, W0, W1, W2, W3> : Node, IContinuousNode, IHasTextProperty where T : struct
{
    public int UpdateOffset => -2;

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

    public ValueOutput<T> Value = new();
    public ValueOutput<W0> Wildcard0 = new("Wildcard 0");
    public ValueOutput<W1> Wildcard1 = new("Wildcard 1");
    public ValueOutput<W2> Wildcard2 = new("Wildcard 2");
    public ValueOutput<W3> Wildcard3 = new("Wildcard 3");

    protected override Task Process(PulseContext c)
    {
        if (string.IsNullOrWhiteSpace(Text)) return Task.CompletedTask;

        var parameter = c.GetParameter<T>(textRegex);
        if (parameter is null) return Task.CompletedTask;
        if (!parameter.IsWildcardType<W0>(0)) return Task.CompletedTask;
        if (!parameter.IsWildcardType<W1>(1)) return Task.CompletedTask;
        if (!parameter.IsWildcardType<W2>(2)) return Task.CompletedTask;
        if (!parameter.IsWildcardType<W3>(3)) return Task.CompletedTask;

        Value.Write(parameter.GetValue<T>(), c);
        Wildcard0.Write(parameter.GetWildcard<W0>(0), c);
        Wildcard1.Write(parameter.GetWildcard<W1>(1), c);
        Wildcard2.Write(parameter.GetWildcard<W2>(2), c);
        Wildcard3.Write(parameter.GetWildcard<W3>(3), c);
        return Task.CompletedTask;
    }
}