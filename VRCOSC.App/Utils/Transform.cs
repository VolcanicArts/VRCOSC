// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;

namespace VRCOSC.App.Utils;

public record struct Transform
{
    public readonly Vector3 Position;
    public readonly Quaternion Rotation;
    public readonly Vector3 Scale;

    public Transform()
    {
        Position = Vector3.Zero;
        Rotation = Quaternion.Identity;
        Scale = Vector3.One;
    }

    public Transform(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Position = position;
        Rotation = Quaternion.Normalize(rotation);
        Scale = scale;
    }

    public static Transform Identity => new();

    public Vector3 Right => Vector3.Transform(Vector3.UnitX, Rotation);
    public Vector3 Left => Vector3.Transform(-Vector3.UnitX, Rotation);
    public Vector3 Up => Vector3.Transform(Vector3.UnitY, Rotation);
    public Vector3 Down => Vector3.Transform(-Vector3.UnitY, Rotation);
    public Vector3 Forward => Vector3.Transform(Vector3.UnitZ, Rotation);
    public Vector3 Backward => Vector3.Transform(-Vector3.UnitZ, Rotation);

    public Transform Inverse()
    {
        var invScale = new Vector3(1f / Scale.X, 1f / Scale.Y, 1f / Scale.Z);
        var invRotation = Quaternion.Conjugate(Rotation);
        var invPosition = Vector3.Transform(-Position, invRotation) * invScale;
        return new Transform(invPosition, invRotation, invScale);
    }

    public Transform Apply(Transform offset)
    {
        var position = Position + Vector3.Transform(offset.Position * Scale, Rotation);
        var rotation = Rotation * offset.Rotation;
        var scale = Scale * offset.Scale;
        return new Transform(position, rotation, scale);
    }

    public Transform ToLocalOf(Transform parent) => parent.Inverse().Apply(this);

    public override string ToString() => $"{Position:F3} : {Rotation.ToEulerDegrees():F3} : {Scale:F3}";
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