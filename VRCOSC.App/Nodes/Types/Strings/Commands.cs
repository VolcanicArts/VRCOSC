// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Strings;

[Node("Parse Command", "Strings")]
public class ParseCommandNode : TryActionNode
{
    public override string DisplayName => "Parse Command";

    [InputMode(InputModes.Connection)]
    public ValueInput<string> Text = new();

    public ValueInput<string> Command = new();
    public ValueInput<CultureInfo> CultureInfo = new(defaultValue: System.Globalization.CultureInfo.CurrentCulture);

    protected override bool TryAction(IPulseContext c)
    {
        var message = Text.Read(c);
        var command = Command.Read(c);
        var cultureInfo = CultureInfo.Read(c);

        if (string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(command) || !message.StartsWith(command, true, cultureInfo))
            return false;

        return TryParseArguments(message, command, cultureInfo, c);
    }

    protected virtual bool TryParseArguments(string message, string command, CultureInfo cultureInfo, IPulseContext c)
    {
        return true;
    }
}

public class ParseCommandNode<T1> : ParseCommandNode where T1 : IParsable<T1>
{
    public ValueOutput<T1> Arg1 = new(typeof(T1).GetFriendlyName());

    protected override bool TryParseArguments(string message, string command, CultureInfo cultureInfo, IPulseContext c)
    {
        try
        {
            var remaining = message[(command.Length + 1)..];

            if (!T1.TryParse(remaining, cultureInfo, out var t1Value))
                return false;

            Arg1.Write(t1Value, c);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class ParseCommandNode<T1, T2> : ParseCommandNode<T1>
    where T1 : IParsable<T1>
    where T2 : IParsable<T2>
{
    public ValueOutput<T2> Arg2 = new(typeof(T2).GetFriendlyName());

    protected override bool TryParseArguments(string message, string command, CultureInfo cultureInfo, IPulseContext c)
    {
        try
        {
            var remaining = message[(command.Length + 1)..];
            var args = remaining.Split(" ", 2);

            if (!T1.TryParse(args[0], cultureInfo, out var t1Value))
                return false;

            Arg1.Write(t1Value, c);

            if (!T2.TryParse(args[1], cultureInfo, out var t2Value))
                return false;

            Arg2.Write(t2Value, c);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class ParseCommandNode<T1, T2, T3> : ParseCommandNode<T1, T2>
    where T1 : IParsable<T1>
    where T2 : IParsable<T2>
    where T3 : IParsable<T3>
{
    public ValueOutput<T3> Arg3 = new(typeof(T3).GetFriendlyName());

    protected override bool TryParseArguments(string message, string command, CultureInfo cultureInfo, IPulseContext c)
    {
        try
        {
            var remaining = message[(command.Length + 1)..];
            var args = remaining.Split(" ", 3);

            if (!T1.TryParse(args[0], cultureInfo, out var t1Value))
                return false;

            Arg1.Write(t1Value, c);

            if (!T2.TryParse(args[1], cultureInfo, out var t2Value))
                return false;

            Arg2.Write(t2Value, c);

            if (!T3.TryParse(args[2], cultureInfo, out var t3Value))
                return false;

            Arg3.Write(t3Value, c);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public sealed class ParseCommandNode<T1, T2, T3, T4> : ParseCommandNode<T1, T2, T3>
    where T1 : IParsable<T1>
    where T2 : IParsable<T2>
    where T3 : IParsable<T3>
    where T4 : IParsable<T4>
{
    public ValueOutput<T4> Arg4 = new(typeof(T4).GetFriendlyName());

    protected override bool TryParseArguments(string message, string command, CultureInfo cultureInfo, IPulseContext c)
    {
        try
        {
            var remaining = message[(command.Length + 1)..];
            var args = remaining.Split(" ", 4);

            if (!T1.TryParse(args[0], cultureInfo, out var t1Value))
                return false;

            Arg1.Write(t1Value, c);

            if (!T2.TryParse(args[1], cultureInfo, out var t2Value))
                return false;

            Arg2.Write(t2Value, c);

            if (!T3.TryParse(args[2], cultureInfo, out var t3Value))
                return false;

            Arg3.Write(t3Value, c);

            if (!T4.TryParse(args[3], cultureInfo, out var t4Value))
                return false;

            Arg4.Write(t4Value, c);
            return true;
        }
        catch
        {
            return false;
        }
    }
}