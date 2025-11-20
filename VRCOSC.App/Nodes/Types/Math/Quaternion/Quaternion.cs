// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Math.Quaternion;

[Node("Quaternion To Euler", "Math/Quaternion")]
[NodeCollapsed]
public sealed class QuaternionToEulerNode : Node
{
    public ValueInput<System.Numerics.Quaternion> Quaternion = new(defaultValue: System.Numerics.Quaternion.Identity);
    public ValueOutput<Vector3> Euler = new();

    protected override Task Process(PulseContext c)
    {
        Euler.Write(Quaternion.Read(c).ToEulerDegrees(), c);
        return Task.CompletedTask;
    }
}

[Node("Euler To Quaternion", "Math/Quaternion")]
[NodeCollapsed]
public sealed class EulerToQuaternionNode : Node
{
    public ValueInput<Vector3> Euler = new();
    public ValueOutput<System.Numerics.Quaternion> Quaternion = new();

    protected override Task Process(PulseContext c)
    {
        Quaternion.Write(Euler.Read(c).ToQuaternion(), c);
        return Task.CompletedTask;
    }
}

[Node("Multiply Quaternion", "Math/Quaternion")]
[NodeCollapsed]
public sealed class QuaternionMultiplyNode : Node
{
    public ValueInput<System.Numerics.Quaternion> A = new();
    public ValueInput<System.Numerics.Quaternion> B = new();

    public ValueOutput<System.Numerics.Quaternion> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(A.Read(c) * B.Read(c), c);
        return Task.CompletedTask;
    }
}