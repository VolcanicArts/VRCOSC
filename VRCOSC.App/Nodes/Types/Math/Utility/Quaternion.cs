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
    public ValueInput<Quaternion> Quaternion = new();
    public ValueOutput<Vector3> Euler = new();

    protected override Task Process(PulseContext c)
    {
        Euler.Write(Quaternion.Read(c).QuaternionToEuler(), c);
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
        Quaternion.Write(Euler.Read(c).EulerToQuaternion(), c);
        return Task.CompletedTask;
    }
}

[Node("Quaternion From Angles", "Math/Utility")]
public sealed class QuaternionFromAnglesNode : Node
{
    public ValueInput<float> X = new();
    public ValueInput<float> Y = new();
    public ValueInput<float> Z = new();
    public ValueOutput<Quaternion> Quaternion = new();

    protected override Task Process(PulseContext c)
    {
        Quaternion.Write(System.Numerics.Quaternion.CreateFromYawPitchRoll(Y.Read(c), X.Read(c), Z.Read(c)), c);
        return Task.CompletedTask;
    }
}