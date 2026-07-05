// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;

namespace VRCOSC.App.Utils;

public struct Transform
{
    public Vector3 Position;
    public Quaternion Rotation;

    public Transform()
    {
        Position = Vector3.Zero;
        Rotation = Quaternion.Identity;
    }

    public Transform(Vector3 position, Quaternion rotation)
    {
        Position = position;
        Rotation = rotation;
    }

    public Transform RelativeTo(Transform parent)
    {
        var inverseRotationOther = Quaternion.Inverse(parent.Rotation);
        var relativePosition = Vector3.Transform(Position - parent.Position, inverseRotationOther);
        var relativeRotation = inverseRotationOther * Rotation;
        return new Transform(relativePosition, relativeRotation);
    }

    public Transform Apply(Transform parent)
    {
        var worldPosition = parent.Position + Vector3.Transform(Position, parent.Rotation);
        var worldRotation = parent.Rotation * Rotation;
        return new Transform(worldPosition, worldRotation);
    }

    public static Transform Identity => new();

    public override string ToString() => $"{Position:F3} : {Rotation.ToEulerDegrees():F3}";
}

public static class TransformExtensions
{
    extension(Vector3 v)
    {
        public Quaternion ToQuaternion()
        {
            var pitch = float.DegreesToRadians(v.X);
            var yaw = float.DegreesToRadians(v.Y);
            var roll = float.DegreesToRadians(v.Z);
            return Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
        }
    }

    extension(Quaternion q)
    {
        public Vector3 ToEulerDegrees()
        {
            q = Quaternion.Normalize(q);

            var x = q.X;
            var y = q.Y;
            var z = q.Z;
            var w = q.W;

            var sinp = float.Clamp(2f * (w * x - y * z), -1f, 1f);
            var pitch = float.Asin(sinp);
            var yaw = float.Atan2(2f * (w * y + z * x), 1f - 2f * (x * x + y * y));
            var roll = float.Atan2(2f * (w * z + x * y), 1f - 2f * (x * x + z * z));

            return new Vector3(float.RadiansToDegrees(pitch), float.RadiansToDegrees(yaw), float.RadiansToDegrees(roll));
        }
    }
}