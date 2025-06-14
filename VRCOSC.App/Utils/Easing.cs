// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.Utils;

public static class Easing
{
    public static float Linear(float k)
    {
        return k;
    }

    public static class Quadratic
    {
        public static float In(float k)
        {
            return k * k;
        }

        public static float Out(float k)
        {
            return k * (2f - k);
        }

        public static float InOut(float k)
        {
            if ((k *= 2f) < 1f) return 0.5f * k * k;

            return -0.5f * ((k -= 1f) * (k - 2f) - 1f);
        }

        public static float Bezier(float k, float c)
        {
            return c * 2 * k * (1 - k) + k * k;
        }
    };

    public static class Cubic
    {
        public static float In(float k)
        {
            return k * k * k;
        }

        public static float Out(float k)
        {
            return 1f + (k -= 1f) * k * k;
        }

        public static float InOut(float k)
        {
            if ((k *= 2f) < 1f) return 0.5f * k * k * k;

            return 0.5f * ((k -= 2f) * k * k + 2f);
        }
    };

    public static class Quartic
    {
        public static float In(float k)
        {
            return k * k * k * k;
        }

        public static float Out(float k)
        {
            return 1f - (k -= 1f) * k * k * k;
        }

        public static float InOut(float k)
        {
            if ((k *= 2f) < 1f) return 0.5f * k * k * k * k;

            return -0.5f * ((k -= 2f) * k * k * k - 2f);
        }
    };

    public static class Quintic
    {
        public static float In(float k)
        {
            return k * k * k * k * k;
        }

        public static float Out(float k)
        {
            return 1f + (k -= 1f) * k * k * k * k;
        }

        public static float InOut(float k)
        {
            if ((k *= 2f) < 1f) return 0.5f * k * k * k * k * k;

            return 0.5f * ((k -= 2f) * k * k * k * k + 2f);
        }
    };

    public static class Sinusoidal
    {
        public static float In(float k)
        {
            return 1f - MathF.Cos(k * MathF.PI / 2f);
        }

        public static float Out(float k)
        {
            return MathF.Sin(k * MathF.PI / 2f);
        }

        public static float InOut(float k)
        {
            return 0.5f * (1f - MathF.Cos(MathF.PI * k));
        }
    };

    public static class Exponential
    {
        public static float In(float k)
        {
            return k == 0f ? 0f : MathF.Pow(1024f, k - 1f);
        }

        public static float Out(float k)
        {
            return Math.Abs(k - 1f) < float.Epsilon ? 1f : 1f - MathF.Pow(2f, -10f * k);
        }

        public static float InOut(float k)
        {
            if (k == 0f) return 0f;
            if (Math.Abs(k - 1f) < float.Epsilon) return 1f;
            if ((k *= 2f) < 1f) return 0.5f * MathF.Pow(1024f, k - 1f);

            return 0.5f * (-MathF.Pow(2f, -10f * (k - 1f)) + 2f);
        }
    };

    public static class Circular
    {
        public static float In(float k)
        {
            return 1f - MathF.Sqrt(1f - k * k);
        }

        public static float Out(float k)
        {
            return MathF.Sqrt(1f - (k -= 1f) * k);
        }

        public static float InOut(float k)
        {
            if ((k *= 2f) < 1f) return -0.5f * (MathF.Sqrt(1f - k * k) - 1);

            return 0.5f * (MathF.Sqrt(1f - (k -= 2f) * k) + 1f);
        }
    };

    public static class Elastic
    {
        public static float In(float k)
        {
            if (k == 0) return 0;
            if (Math.Abs(k - 1) < float.Epsilon) return 1;

            return -MathF.Pow(2f, 10f * (k -= 1f)) * MathF.Sin((k - 0.1f) * (2f * MathF.PI) / 0.4f);
        }

        public static float Out(float k)
        {
            if (k == 0) return 0;
            if (Math.Abs(k - 1) < float.Epsilon) return 1;

            return MathF.Pow(2f, -10f * k) * MathF.Sin((k - 0.1f) * (2f * MathF.PI) / 0.4f) + 1f;
        }

        public static float InOut(float k)
        {
            if ((k *= 2f) < 1f) return -0.5f * MathF.Pow(2f, 10f * (k -= 1f)) * MathF.Sin((k - 0.1f) * (2f * MathF.PI) / 0.4f);

            return MathF.Pow(2f, -10f * (k -= 1f)) * MathF.Sin((k - 0.1f) * (2f * MathF.PI) / 0.4f) * 0.5f + 1f;
        }
    };

    public static class Back
    {
        private const float s = 1.70158f;
        private const float s2 = 2.5949095f;

        public static float In(float k)
        {
            return k * k * ((s + 1f) * k - s);
        }

        public static float Out(float k)
        {
            return (k -= 1f) * k * ((s + 1f) * k + s) + 1f;
        }

        public static float InOut(float k)
        {
            if ((k *= 2f) < 1f) return 0.5f * (k * k * ((s2 + 1f) * k - s2));

            return 0.5f * ((k -= 2f) * k * ((s2 + 1f) * k + s2) + 2f);
        }
    };

    public static class Bounce
    {
        public static float In(float k)
        {
            return 1f - Out(1f - k);
        }

        public static float Out(float k)
        {
            return k switch
            {
                < 1f / 2.75f => 7.5625f * k * k,
                < 2f / 2.75f => 7.5625f * (k -= 1.5f / 2.75f) * k + 0.75f,
                < 2.5f / 2.75f => 7.5625f * (k -= 2.25f / 2.75f) * k + 0.9375f,
                _ => 7.5625f * (k -= 2.625f / 2.75f) * k + 0.984375f
            };
        }

        public static float InOut(float k)
        {
            if (k < 0.5f) return In(k * 2f) * 0.5f;

            return Out(k * 2f - 1f) * 0.5f + 0.5f;
        }
    };
}