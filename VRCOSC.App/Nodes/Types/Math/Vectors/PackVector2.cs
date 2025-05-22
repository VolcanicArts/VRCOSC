// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Math.Vectors;

[Node("Pack Vector2", "Math/Vector")]
public class PackVector2Node : Node
{
    public ValueInput<float> X = new();
    public ValueInput<float> Y = new();
    public ValueOutput<Vector2> Result = new();

    protected override void Process(PulseContext c)
    {
        var x = X.Read(c);
        var y = Y.Read(c);
        Result.Write(new Vector2(x, y), c);
    }
}