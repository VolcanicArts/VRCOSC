// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Utility;

[Node("Float Progress Visual", "Utility")]
public sealed class FloatProgressVisualNode : ValueComputeNode<string>
{
    public ValueInput<float> Input = new();
    public ValueInput<int> Resolution = new(defaultValue: 10);
    public ValueInput<string> Line = new(defaultValue: "\u2501");
    public ValueInput<string> Position = new(defaultValue: "\u25CF");
    public ValueInput<string> Start = new(defaultValue: "\u2523");
    public ValueInput<string> End = new(defaultValue: "\u252B");

    protected override string ComputeValue(IPulseContext c)
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
        return visual;
    }
}

[Node("OwO", "Utility")]
public sealed class OwONode() : ConstantNode<string>("What's this?");

[Node("Changes Per Second", "Utility")]
[NodeCollapsed]
public sealed class ChangesPerSecondNode<T> : Node, IContinuousNode
{
    public int UpdateOffset => 1;

    private const int capacity = 256;

    public ValueInput<T> Value = new();
    public ValueOutput<float> ChangesPerSecond = new();

    public GlobalStore<State> StateStore = new();

    protected override Task Process(IPulseContext c)
    {
        var state = StateStore.Read(c);

        if (state is null)
        {
            state = new State();
            StateStore.Write(state, c);
        }

        var value = Value.Read(c);
        var now = Stopwatch.GetTimestamp() * (1.0 / Stopwatch.Frequency);

        if (!state.HasLastValue)
        {
            state.LastValue = value;
            state.HasLastValue = true;
        }
        else if (!EqualityComparer<T>.Default.Equals(state.LastValue, value))
        {
            state.LastValue = value;

            var index = (state.Head + state.Count) % capacity;
            state.Times[index] = now;

            if (state.Count == capacity)
                state.Head = (state.Head + 1) % capacity;
            else
                state.Count++;
        }

        while (state.Count > 0 && now - state.Times[state.Head] > 1.0)
        {
            state.Head = (state.Head + 1) % capacity;
            state.Count--;
        }

        ChangesPerSecond.Write(state.Count, c);
        return Task.CompletedTask;
    }

    public sealed record State
    {
        public double[] Times = new double[capacity];
        public int Head;
        public int Count;

        public T? LastValue;
        public bool HasLastValue;
    }
}