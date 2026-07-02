// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using VRCOSC.App.Nodes;

namespace VRCOSC.App.ChatBox.Clips.Variables.Instances;

public class PulseClipVariable : ClipVariable
{
    public PulseClipVariable()
    {
    }

    public PulseClipVariable(ClipVariableReference reference)
        : base(reference)
    {
    }

    protected override string Format(object value)
    {
        var graphs = NodeManager.GetInstance().Graphs;
        var variables = graphs.SelectMany(g => g.GraphVariables.Values);
        var variable = variables.FirstOrDefault(v => v.GetId() == Guid.Parse(VariableID));
        return (string?)variable?.GetValue() ?? string.Empty;
    }
}