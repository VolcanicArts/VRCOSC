// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
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

    public static Transform Identity => new();
}

public static class TransformExtensions
{
    public static Vector3 QuaternionToEuler(this Quaternion q)
    {
        q = Quaternion.Normalize(q);

        // Pitch (X-axis)
        var sinp = 2 * (q.W * q.X + q.Y * q.Z);
        var cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
        var pitch = MathF.Atan2(sinp, cosp);

        // Yaw (Y-axis)
        var siny = 2 * (q.W * q.Y - q.Z * q.X);

        var yaw = MathF.Abs(siny) >= 1
            ? MathF.CopySign(MathF.PI / 2, siny)
            : MathF.Asin(siny);

        // Roll (Z-axis)
        var sinr = 2 * (q.W * q.Z + q.X * q.Y);
        var cosr = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
        var roll = MathF.Atan2(sinr, cosr);

        return new Vector3(
            float.RadiansToDegrees(pitch),
            float.RadiansToDegrees(yaw),
            float.RadiansToDegrees(roll)
        );
    }

    public static Quaternion EulerToQuaternion(this Vector3 vector)
    {
        var pitch = float.DegreesToRadians(vector.X);
        var yaw = float.DegreesToRadians(vector.Y);
        var roll = float.DegreesToRadians(vector.Z);

        return Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
    }
}