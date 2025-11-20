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

    public Transform RelativeTo(Transform parent)
    {
        var inverseRotationOther = Quaternion.Inverse(parent.Rotation);
        var relativePosition = Vector3.Transform(Position - parent.Position, inverseRotationOther);
        var relativeRotation = inverseRotationOther * Rotation;
        return new Transform(relativePosition, relativeRotation);
    }

    public static Transform Identity => new();
}

public static class TransformExtensions
{
    /// <summary>
    /// Quaternion ← Euler (DEGREES). Convention: X=Pitch, Y=Yaw, Z=Roll, order Y→X→Z (YXZ).
    /// </summary>
    public static Quaternion ToQuaternion(this Vector3 v)
    {
        var pitch = float.DegreesToRadians(v.X);
        var yaw = float.DegreesToRadians(v.Y);
        var roll = float.DegreesToRadians(v.Z);
        return Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
    }

    /// <summary>
    /// Euler (DEGREES) ← Quaternion. Convention: X=Pitch, Y=Yaw, Z=Roll, order Y→X→Z (YXZ).
    /// Robust near gimbal lock (|Yaw|≈90°) and clamps asin input.
    /// </summary>
    public static Vector3 ToEulerDegrees(this Quaternion q)
    {
        q = Quaternion.Normalize(q);
        var (w, x, y, z) = (q.W, q.X, q.Y, q.Z);

        // Yaw (Y) is the middle angle for YXZ
        var siny = 2f * (w * y - z * x);
        var yaw = MathF.Asin(float.Clamp(siny, -1f, 1f));

        // Singularity when |siny|≈1 → |yaw|≈90°
        const float eps = 1e-6f;

        if (MathF.Abs(siny) > 1f - eps)
        {
            // Canonicalize by preserving Pitch (X), zeroing Roll (Z).
            var pitch = MathF.Atan2(2f * (w * x - y * z), 1f - 2f * (x * x + z * z));
            const float roll = 0f;
            return new Vector3(float.RadiansToDegrees(pitch), float.RadiansToDegrees(yaw), float.RadiansToDegrees(roll));
        }

        {
            // Regular case
            var sinp = 2f * (w * x + y * z);
            var cosp = 1f - 2f * (x * x + y * y);
            var pitch = MathF.Atan2(sinp, cosp);

            var sinr = 2f * (w * z + x * y);
            var cosr = 1f - 2f * (y * y + z * z);
            var roll = MathF.Atan2(sinr, cosr);

            return new Vector3(float.RadiansToDegrees(pitch), float.RadiansToDegrees(yaw), float.RadiansToDegrees(roll));
        }
    }
}