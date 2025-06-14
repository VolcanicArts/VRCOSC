// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Numerics;

namespace VRCOSC.App.Utils;

public readonly record struct Transform
{
    public readonly Vector3 Position;
    public readonly Quaternion Rotation;

    public Transform(Vector3 position, Quaternion rotation)
    {
        Position = position;
        Rotation = rotation;
    }

    public static Transform Zero => new(Vector3.Zero, Quaternion.Zero);
}

public static class TransformExtensions
{
    public static Vector3 QuaternionToEuler(this Quaternion q)
    {
        q = Quaternion.Normalize(q);

        var sinrCosp = 2 * (q.W * q.X + q.Y * q.Z);
        var cosrCosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
        var roll = MathF.Atan2(sinrCosp, cosrCosp);

        var sinp = 2 * (q.W * q.Y - q.Z * q.X);

        // Use 90 degrees if out of range
        var pitch = MathF.Abs(sinp) >= 1 ? MathF.CopySign(MathF.PI / 2, sinp) : MathF.Asin(sinp);

        var sinyCosp = 2 * (q.W * q.Z + q.X * q.Y);
        var cosyCosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
        var yaw = MathF.Atan2(sinyCosp, cosyCosp);

        return new Vector3(
            pitch * 180f / MathF.PI,
            yaw * 180f / MathF.PI,
            roll * 180f / MathF.PI
        );
    }

    public static Quaternion EulerToQuaternion(this Vector3 vector)
    {
        return Quaternion.CreateFromYawPitchRoll(vector.Y, vector.X, vector.Z);
    }
}