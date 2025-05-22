// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Math.Map;

[Node("Remap -1,1 To 0,1", "Math/Map")]
[NodeCollapsed]
public class Remap1101Node : Node
{
    public ValueInput<float> Value = new();
    public ValueOutput<float> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(Utils.Interpolation.Map(Value.Read(c), -1, 1, 0, 1), c);
    }
}