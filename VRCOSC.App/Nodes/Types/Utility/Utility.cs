// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Utility;

[Node("Float Progress Visual", "Utility")]
public sealed class FloatProgressVisualNode : Node
{
    public ValueInput<float> Input = new();
    public ValueInput<int> Resolution = new(defaultValue: 10);
    public ValueInput<string> Line = new(defaultValue: "\u2501");
    public ValueInput<string> Position = new(defaultValue: "\u25CF");
    public ValueInput<string> Start = new(defaultValue: "\u2523");
    public ValueInput<string> End = new(defaultValue: "\u252B");

    public ValueOutput<string> Result = new();

    protected override Task Process(PulseContext c)
    {
        var input = Input.Read(c);
        var resolution = Resolution.Read(c);
        var line = Line.Read(c);
        var position = Position.Read(c);
        var start = Start.Read(c);
        var end = End.Read(c);

        input = float.Clamp(input, 0f, 1f);

        var dotPosition = resolution * input;

        var visual = string.Empty;
        visual += start;

        for (var i = 0; i < resolution; i++)
        {
            visual += (i <= dotPosition && i + 1 > dotPosition) ? position : line;
        }

        visual += end;

        Result.Write(visual, c);
        return Task.CompletedTask;
    }
}

[Node("Parse", "Utility")]
public sealed class ParseNode<T> : Node, IFlowInput where T : IParsable<T>
{
    public FlowContinuation Success = new("On Success");
    public FlowContinuation Failed = new("On Failed");

    public ValueInput<string> Input = new();
    public ValueInput<CultureInfo> Culture = new("Culture", CultureInfo.CurrentCulture);
    public ValueOutput<T> Output = new();

    protected override async Task Process(PulseContext c)
    {
        if (T.TryParse(Input.Read(c), Culture.Read(c), out var parsedInput))
        {
            Output.Write(parsedInput, c);
            await Success.Execute(c);
        }
        else
        {
            Output.Write(default!, c);
            await Failed.Execute(c);
        }
    }
}