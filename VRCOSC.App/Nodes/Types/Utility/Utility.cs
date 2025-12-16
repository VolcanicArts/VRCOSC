// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

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

[Node("OwO", "Utility")]
public sealed class OwONode : ConstantNode<string>
{
    protected override string GetValue() => "What's this?";
}