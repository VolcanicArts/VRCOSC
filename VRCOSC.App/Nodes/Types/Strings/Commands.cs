// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Strings;

[Node("Parse Command", "Strings/Commands")]
public sealed class ParseCommandNode : Node, IFlowInput
{
    public FlowContinuation OnSuccess = new();
    public FlowContinuation OnFail = new();

    public ValueInput<string> Text = new();
    public ValueInput<string> Command = new();
    public ValueInput<CultureInfo> CultureInfo = new(defaultValue: System.Globalization.CultureInfo.CurrentCulture);

    protected override Task Process(PulseContext c)
    {
        var message = Text.Read(c);
        var command = Command.Read(c);
        var cultureInfo = CultureInfo.Read(c);

        if (string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(command) || !message.StartsWith(command, true, cultureInfo))
            return OnFail.Execute(c);

        return OnSuccess.Execute(c);
    }
}

[Node("Parse Command With Data 1", "Strings/Commands")]
public sealed class ParseCommandNode<T1> : Node, IFlowInput where T1 : IParsable<T1>
{
    public override string DisplayName => "Parse Command";

    public FlowContinuation OnSuccess = new();
    public FlowContinuation OnFail = new();

    public ValueInput<string> Text = new();
    public ValueInput<string> Command = new();
    public ValueInput<CultureInfo> CultureInfo = new(defaultValue: System.Globalization.CultureInfo.CurrentCulture);

    public ValueOutput<T1> Arg1 = new(typeof(T1).GetFriendlyName());

    protected override Task Process(PulseContext c)
    {
        var message = Text.Read(c);
        var command = Command.Read(c);
        var cultureInfo = CultureInfo.Read(c);

        if (string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(command) || !message.StartsWith(command, true, cultureInfo))
            return OnFail.Execute(c);

        try
        {
            var remaining = message[(command.Length + 1)..];

            if (!T1.TryParse(remaining, cultureInfo, out var t1Value))
                return OnFail.Execute(c);

            Arg1.Write(t1Value, c);
        }
        catch
        {
            return OnFail.Execute(c);
        }

        return OnSuccess.Execute(c);
    }
}

[Node("Parse Command With Data 2", "Strings/Commands")]
public sealed class ParseCommandNode<T1, T2> : Node, IFlowInput where T1 : IParsable<T1> where T2 : IParsable<T2>
{
    public override string DisplayName => "Parse Command";

    public FlowContinuation OnSuccess = new();
    public FlowContinuation OnFail = new();

    public ValueInput<string> Text = new();
    public ValueInput<string> Command = new();
    public ValueInput<CultureInfo> CultureInfo = new(defaultValue: System.Globalization.CultureInfo.CurrentCulture);

    public ValueOutput<T1> Arg1 = new(typeof(T1).GetFriendlyName());
    public ValueOutput<T2> Arg2 = new(typeof(T2).GetFriendlyName());

    protected override Task Process(PulseContext c)
    {
        var message = Text.Read(c);
        var command = Command.Read(c);
        var cultureInfo = CultureInfo.Read(c);

        if (string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(command) || !message.StartsWith(command, true, cultureInfo))
            return OnFail.Execute(c);

        try
        {
            var remaining = message[(command.Length + 1)..];
            var args = remaining.Split(" ", 2);

            if (!T1.TryParse(args[0], cultureInfo, out var t1Value))
                return OnFail.Execute(c);

            Arg1.Write(t1Value, c);

            if (!T2.TryParse(args[1], cultureInfo, out var t2Value))
                return OnFail.Execute(c);

            Arg2.Write(t2Value, c);
        }
        catch
        {
            return OnFail.Execute(c);
        }

        return OnSuccess.Execute(c);
    }
}

[Node("Parse Command With Data 3", "Strings/Commands")]
public sealed class ParseCommandNode<T1, T2, T3> : Node, IFlowInput where T1 : IParsable<T1> where T2 : IParsable<T2> where T3 : IParsable<T3>
{
    public override string DisplayName => "Parse Command";

    public FlowContinuation OnSuccess = new();
    public FlowContinuation OnFail = new();

    public ValueInput<string> Text = new();
    public ValueInput<string> Command = new();
    public ValueInput<CultureInfo> CultureInfo = new(defaultValue: System.Globalization.CultureInfo.CurrentCulture);

    public ValueOutput<T1> Arg1 = new(typeof(T1).GetFriendlyName());
    public ValueOutput<T2> Arg2 = new(typeof(T2).GetFriendlyName());
    public ValueOutput<T3> Arg3 = new(typeof(T3).GetFriendlyName());

    protected override Task Process(PulseContext c)
    {
        var message = Text.Read(c);
        var command = Command.Read(c);
        var cultureInfo = CultureInfo.Read(c);

        if (string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(command) || !message.StartsWith(command, true, cultureInfo))
            return OnFail.Execute(c);

        try
        {
            var remaining = message[(command.Length + 1)..];
            var args = remaining.Split(" ", 3);

            if (!T1.TryParse(args[0], cultureInfo, out var t1Value))
                return OnFail.Execute(c);

            Arg1.Write(t1Value, c);

            if (!T2.TryParse(args[1], cultureInfo, out var t2Value))
                return OnFail.Execute(c);

            Arg2.Write(t2Value, c);

            if (!T3.TryParse(args[2], cultureInfo, out var t3Value))
                return OnFail.Execute(c);

            Arg3.Write(t3Value, c);
        }
        catch
        {
            return OnFail.Execute(c);
        }

        return OnSuccess.Execute(c);
    }
}

[Node("Parse Command With Data 4", "Strings/Commands")]
public sealed class ParseCommandNode<T1, T2, T3, T4> : Node, IFlowInput where T1 : IParsable<T1> where T2 : IParsable<T2> where T3 : IParsable<T3> where T4 : IParsable<T4>
{
    public override string DisplayName => "Parse Command";

    public FlowContinuation OnSuccess = new();
    public FlowContinuation OnFail = new();

    public ValueInput<string> Text = new();
    public ValueInput<string> Command = new();
    public ValueInput<CultureInfo> CultureInfo = new(defaultValue: System.Globalization.CultureInfo.CurrentCulture);

    public ValueOutput<T1> Arg1 = new(typeof(T1).GetFriendlyName());
    public ValueOutput<T2> Arg2 = new(typeof(T2).GetFriendlyName());
    public ValueOutput<T3> Arg3 = new(typeof(T3).GetFriendlyName());
    public ValueOutput<T4> Arg4 = new(typeof(T4).GetFriendlyName());

    protected override Task Process(PulseContext c)
    {
        var message = Text.Read(c);
        var command = Command.Read(c);
        var cultureInfo = CultureInfo.Read(c);

        if (string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(command) || !message.StartsWith(command, true, cultureInfo))
            return OnFail.Execute(c);

        try
        {
            var remaining = message[(command.Length + 1)..];
            var args = remaining.Split(" ", 4);

            if (!T1.TryParse(args[0], cultureInfo, out var t1Value))
                return OnFail.Execute(c);

            Arg1.Write(t1Value, c);

            if (!T2.TryParse(args[1], cultureInfo, out var t2Value))
                return OnFail.Execute(c);

            Arg2.Write(t2Value, c);

            if (!T3.TryParse(args[2], cultureInfo, out var t3Value))
                return OnFail.Execute(c);

            Arg3.Write(t3Value, c);

            if (!T4.TryParse(args[3], cultureInfo, out var t4Value))
                return OnFail.Execute(c);

            Arg4.Write(t4Value, c);
        }
        catch
        {
            return OnFail.Execute(c);
        }

        return OnSuccess.Execute(c);
    }
}