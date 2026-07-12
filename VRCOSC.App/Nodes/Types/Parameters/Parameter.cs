// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Threading.Tasks;
using VRCOSC.App.OSC;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Parameters;

[Node("Send Parameter", "VRChat/Parameters/Send")]
[NodeGenerics(typeof(bool), typeof(int), typeof(float))]
public sealed class SendParameterNode<T> : ActionNode where T : unmanaged
{
    private AppManager appManager => field ??= AppManager.GetInstance();

    private readonly RegexCache pattern = new();

    public ValueInput<string> Name = new();
    public ValueInput<T> Value = new();

    protected override void DoAction(IPulseContext c)
    {
        var name = Name.Read(c);
        if (string.IsNullOrWhiteSpace(name)) return;

        appManager.SendToAllParameter(pattern.Get(OSCPatterns.ToSendPattern(name)), name, Value.Read(c));
    }
}

[Node("Drive Parameter", "VRChat/Parameters/Send")]
[NodeGenerics(typeof(bool), typeof(int), typeof(float))]
public sealed class DriveParameterNode<T> : Node, IUpdateNode where T : unmanaged
{
    public int UpdateOffset => 2;

    private AppManager appManager => field ??= AppManager.GetInstance();

    private readonly RegexCache pattern = new();

    public GlobalStore<T> CurrValue = new();

    [InputMode(InputModes.Inline)]
    public ValueInput<string> Name = new();

    public ValueInput<T> Value = new();

    protected override Task Process(IPulseContext c)
    {
        CurrValue.Write(Value.Read(c), c);
        return Task.CompletedTask;
    }

    public void OnUpdate(IPulseContext c)
    {
        var name = Name.Read(c);
        if (string.IsNullOrWhiteSpace(name)) return;

        appManager.SendToAllParameter(pattern.Get(OSCPatterns.ToSendPattern(name)), name, CurrValue.Read(c));
    }
}

[Node("Toggle Parameter", "VRChat/Parameters/Send")]
[NodeGenerics(typeof(bool), typeof(int), typeof(float))]
public sealed class ToggleParameterNode<T> : ActionNode where T : unmanaged
{
    public ValueInput<string> Name = new();
    public ValueInput<T> ValueOn;
    public ValueInput<T> ValueOff = new("Off");

    public ToggleParameterNode()
    {
        ValueOn = new ValueInput<T>("On", defaultValue: getOnValue());
    }

    protected override void DoAction(IPulseContext c)
    {
        var name = Name.Read(c);
        var valueOn = ValueOn.Read(c);
        var valueOff = ValueOff.Read(c);

        if (string.IsNullOrWhiteSpace(name)) return;

        var parameter = AppManager.GetInstance().GetParameter<T>(name);

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

        AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOSCConstants.ADDRESS_AVATAR_PARAMETERS}/{name}", sendValue);
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

[Node("Parameter Source", "VRChat/Parameters/Receive")]
[NodeGenerics(typeof(bool), typeof(int), typeof(float))]
public sealed class ParameterSourceNode<T>() : ValueSourceNode<T>("Value") where T : unmanaged
{
    public override int UpdateOffset => -2;

    private AppManager appManager => field ??= AppManager.GetInstance();

    private readonly RegexCache pattern = new();

    [InputMode(InputModes.Inline)]
    public ValueInput<string> Name = new();

    protected override T ComputeValue(IPulseContext c)
    {
        var name = Name.Read(c);

        return string.IsNullOrWhiteSpace(name)
            ? default
            : appManager.GetParameterValue<T>(pattern.Get(OSCPatterns.ToReceivePattern(name)));
    }
}

[Node("Physbone Parameter Source", "VRChat/Parameters/Receive")]
public sealed class PhysboneParameterSourceNode : Node, IContinuousNode
{
    public int UpdateOffset => -2;

    private AppManager appManager => field ??= AppManager.GetInstance();

    private readonly RegexCache grabbedPattern = new();
    private readonly RegexCache posedPattern = new();
    private readonly RegexCache anglePattern = new();
    private readonly RegexCache stretchPattern = new();
    private readonly RegexCache squishPattern = new();

    [InputMode(InputModes.Inline)]
    public ValueInput<string> Name = new();

    public ValueOutput<bool> Grabbed = new();
    public ValueOutput<bool> Posed = new();
    public ValueOutput<float> Angle = new();
    public ValueOutput<float> Stretch = new();
    public ValueOutput<float> Squish = new();

    protected override Task Process(IPulseContext c)
    {
        var name = Name.Read(c);
        if (string.IsNullOrWhiteSpace(name)) return Task.CompletedTask;

        Grabbed.Write(appManager.GetParameterValue<bool>(grabbedPattern.Get(OSCPatterns.ToReceivePattern($"{name}_IsGrabbed"))), c);
        Posed.Write(appManager.GetParameterValue<bool>(posedPattern.Get(OSCPatterns.ToReceivePattern($"{name}_IsPosed"))), c);
        Angle.Write(appManager.GetParameterValue<float>(anglePattern.Get(OSCPatterns.ToReceivePattern($"{name}_Angle"))), c);
        Stretch.Write(appManager.GetParameterValue<float>(stretchPattern.Get(OSCPatterns.ToReceivePattern($"{name}_Stretch"))), c);
        Squish.Write(appManager.GetParameterValue<float>(squishPattern.Get(OSCPatterns.ToReceivePattern($"{name}_Squish"))), c);
        return Task.CompletedTask;
    }
}

[Node("Raycast Parameter Source", "VRChat/Parameters/Receive")]
public sealed class RaycastParameterSourceNode : Node, IContinuousNode
{
    public int UpdateOffset => -2;

    private AppManager appManager => field ??= AppManager.GetInstance();

    private readonly RegexCache hitPattern = new();
    private readonly RegexCache ratioPattern = new();
    private readonly RegexCache distancePattern = new();

    [InputMode(InputModes.Inline)]
    public ValueInput<string> Name = new();

    public ValueOutput<bool> Hit = new();
    public ValueOutput<float> Ratio = new();
    public ValueOutput<float> Distance = new();

    protected override Task Process(IPulseContext c)
    {
        var name = Name.Read(c);
        if (string.IsNullOrWhiteSpace(name)) return Task.CompletedTask;

        Hit.Write(appManager.GetParameterValue<bool>(hitPattern.Get(OSCPatterns.ToReceivePattern($"{name}_Hit"))), c);
        Ratio.Write(appManager.GetParameterValue<float>(ratioPattern.Get(OSCPatterns.ToReceivePattern($"{name}_Ratio"))), c);
        Distance.Write(appManager.GetParameterValue<float>(distancePattern.Get(OSCPatterns.ToReceivePattern($"{name}_Distance"))), c);
        return Task.CompletedTask;
    }
}

[Node("Wildcard Parameter Source", "VRChat/Parameters/Receive")]
public class WildcardParameterSourceNode<T, W0> : Node, IContinuousNode where T : unmanaged
{
    public int UpdateOffset => -2;

    private AppManager appManager => field ??= AppManager.GetInstance();

    private readonly RegexCache pattern = new();

    [InputMode(InputModes.Inline)]
    public ValueInput<string> Name = new();

    public ValueOutput<T> Value = new();
    public ValueOutput<W0> Wildcard0 = new("Wildcard 0");

    protected override Task Process(IPulseContext c)
    {
        var name = Name.Read(c);
        if (string.IsNullOrWhiteSpace(name)) return Task.CompletedTask;

        var parameter = appManager.GetTemplatedParameter<T>(pattern.Get(OSCPatterns.ToReceivePattern(name)));
        if (parameter is null) return Task.CompletedTask;
        if (!ValidateWildcards(parameter)) return Task.CompletedTask;

        Value.Write(parameter.GetValue<T>(), c);
        WriteWildcards(parameter, c);
        return Task.CompletedTask;
    }

    protected virtual bool ValidateWildcards(TemplatedVRChatParameter parameter)
    {
        return parameter.IsWildcardType<W0>(0);
    }

    protected virtual void WriteWildcards(TemplatedVRChatParameter parameter, IPulseContext c)
    {
        Wildcard0.Write(parameter.GetWildcard<W0>(0), c);
    }
}

public class WildcardParameterSourceNode<T, W0, W1> : WildcardParameterSourceNode<T, W0> where T : unmanaged
{
    public ValueOutput<W1> Wildcard1 = new("Wildcard 1");

    protected override bool ValidateWildcards(TemplatedVRChatParameter parameter)
    {
        return base.ValidateWildcards(parameter) && parameter.IsWildcardType<W1>(1);
    }

    protected override void WriteWildcards(TemplatedVRChatParameter parameter, IPulseContext c)
    {
        base.WriteWildcards(parameter, c);
        Wildcard1.Write(parameter.GetWildcard<W1>(1), c);
    }
}

public class WildcardParameterSourceNode<T, W0, W1, W2> : WildcardParameterSourceNode<T, W0, W1> where T : unmanaged
{
    public ValueOutput<W2> Wildcard2 = new("Wildcard 2");

    protected override bool ValidateWildcards(TemplatedVRChatParameter parameter)
    {
        return base.ValidateWildcards(parameter) && parameter.IsWildcardType<W2>(2);
    }

    protected override void WriteWildcards(TemplatedVRChatParameter parameter, IPulseContext c)
    {
        base.WriteWildcards(parameter, c);
        Wildcard2.Write(parameter.GetWildcard<W2>(2), c);
    }
}

public sealed class WildcardParameterSourceNode<T, W0, W1, W2, W3> : WildcardParameterSourceNode<T, W0, W1, W2> where T : unmanaged
{
    public ValueOutput<W3> Wildcard3 = new("Wildcard 3");

    protected override bool ValidateWildcards(TemplatedVRChatParameter parameter)
    {
        return base.ValidateWildcards(parameter) && parameter.IsWildcardType<W3>(3);
    }

    protected override void WriteWildcards(TemplatedVRChatParameter parameter, IPulseContext c)
    {
        base.WriteWildcards(parameter, c);
        Wildcard3.Write(parameter.GetWildcard<W3>(3), c);
    }
}