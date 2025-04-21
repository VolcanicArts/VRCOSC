// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Nodes.Types.Constants;

[Node("String Value", "Value")]
[NodeValueOutput("Value")]
public class StringTextNode : Node
{
    public Observable<string> Text { get; } = new(string.Empty);

    [NodeProcess]
    private string process() => Text.Value;
}

[Node("Int Value", "Value")]
[NodeValueOutput("Value")]
public class IntTextNode : Node
{
    public Observable<int> Int { get; } = new();

    [NodeProcess]
    private int process() => Int.Value;
}

[Node("Float Value", "Value")]
[NodeValueOutput("Value")]
public class FloatTextNode : Node
{
    public Observable<float> Float { get; } = new();

    [NodeProcess]
    private float process() => Float.Value;
}

[Node("TimeSpan Value", "Value")]
[NodeValueOutput("Value")]
public class TimeSpanNode : Node
{
    public Observable<TimeSpan> TimeSpan { get; } = new();

    [NodeProcess]
    private TimeSpan process() => TimeSpan.Value;
}