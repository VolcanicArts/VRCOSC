// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Math;

[Node("Quaternion To Euler", "Math/Utility")]
public class QuaternionToEulerNode : Node
{
    public ValueInput<Quaternion> Quaternion = new();
    public ValueOutput<Vector3> Euler = new();

    protected override void Process(PulseContext c)
    {
        Euler.Write(Quaternion.Read(c).QuaternionToEuler(), c);
    }
}

[Node("Euler To Quaternion", "Math/Utility")]
public class EulerToQuaternionNode : Node
{
    public ValueInput<Vector3> Euler = new();
    public ValueOutput<Quaternion> Quaternion = new();

    protected override void Process(PulseContext c)
    {
        Quaternion.Write(Euler.Read(c).EulerToQuaternion(), c);
    }
}

[Node("Quaternion From Angles", "Math/Utility")]
public class QuaternionFromAnglesNode : Node
{
    public ValueInput<float> X = new();
    public ValueInput<float> Y = new();
    public ValueInput<float> Z = new();
    public ValueOutput<Quaternion> Quaternion = new();

    protected override void Process(PulseContext c)
    {
        Quaternion.Write(System.Numerics.Quaternion.CreateFromYawPitchRoll(Y.Read(c), X.Read(c), Z.Read(c)), c);
    }
}