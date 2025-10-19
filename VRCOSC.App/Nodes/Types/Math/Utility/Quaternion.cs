// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Math.Utility;

[Node("Quaternion To Euler", "Math/Utility")]
[NodeCollapsed]
public sealed class QuaternionToEulerNode : Node
{
    public ValueInput<Quaternion> Quaternion = new(defaultValue: System.Numerics.Quaternion.Identity);
    public ValueOutput<Vector3> Euler = new();

    protected override Task Process(PulseContext c)
    {
        Euler.Write(Quaternion.Read(c).ToEulerDegrees(), c);
        return Task.CompletedTask;
    }
}

[Node("Euler To Quaternion", "Math/Utility")]
[NodeCollapsed]
public sealed class EulerToQuaternionNode : Node
{
    public ValueInput<Vector3> Euler = new();
    public ValueOutput<Quaternion> Quaternion = new();

    protected override Task Process(PulseContext c)
    {
        Quaternion.Write(Euler.Read(c).ToQuaternion(), c);
        return Task.CompletedTask;
    }
}