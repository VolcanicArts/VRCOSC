// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;

namespace VRCOSC.App.Utils;

public static class Easing
{
    public static class Quadratic
    {
        public static T In<T>(T k) where T : IFloatingPointIeee754<T>
        {
            return k * k;
        }

        public static T Out<T>(T k) where T : IFloatingPointIeee754<T>
        {
            return k * (T.CreateChecked(2) - k);
        }

        public static T InOut<T>(T k) where T : IFloatingPointIeee754<T>
        {
            if ((k *= T.CreateChecked(2)) < T.One) return T.CreateChecked(0.5) * k * k;

            return T.CreateChecked(-0.5) * ((k -= T.One) * (k - T.CreateChecked(2)) - T.One);
        }
    };

    public static class Cubic
    {
        public static T In<T>(T k) where T : IFloatingPointIeee754<T>
        {
            return k * k * k;
        }

        public static T Out<T>(T k) where T : IFloatingPointIeee754<T>
        {
            return T.One + (k -= T.One) * k * k;
        }

        public static T InOut<T>(T k) where T : IFloatingPointIeee754<T>
        {
            if ((k *= T.CreateChecked(2)) < T.One) return T.CreateChecked(0.5) * k * k * k;

            return T.CreateChecked(0.5) * ((k -= T.CreateChecked(2)) * k * k + T.CreateChecked(2));
        }
    };

    public static class Quartic
    {
        public static T In<T>(T k) where T : IFloatingPointIeee754<T>
        {
            return k * k * k * k;
        }

        public static T Out<T>(T k) where T : IFloatingPointIeee754<T>
        {
            return T.One - (k -= T.One) * k * k * k;
        }

        public static T InOut<T>(T k) where T : IFloatingPointIeee754<T>
        {
            if ((k *= T.CreateChecked(2)) < T.One) return T.CreateChecked(0.5) * k * k * k * k;

            return T.CreateChecked(-0.5f) * ((k -= T.CreateChecked(2)) * k * k * k - T.CreateChecked(2));
        }
    };

    public static class Quintic
    {
        public static T In<T>(T k) where T : IFloatingPointIeee754<T>
        {
            return k * k * k * k * k;
        }

        public static T Out<T>(T k) where T : IFloatingPointIeee754<T>
        {
            return T.One + (k -= T.One) * k * k * k * k;
        }

        public static T InOut<T>(T k) where T : IFloatingPointIeee754<T>
        {
            if ((k *= T.CreateChecked(2)) < T.One) return T.CreateChecked(0.5) * k * k * k * k * k;

            return T.CreateChecked(0.5) * ((k -= T.CreateChecked(2)) * k * k * k * k + T.CreateChecked(2));
        }
    };

    public static class Sinusoidal
    {
        public static T In<T>(T k) where T : IFloatingPointIeee754<T>
        {
            return T.One - T.CosPi(k / T.CreateChecked(2));
        }

        public static T Out<T>(T k) where T : IFloatingPointIeee754<T>
        {
            return T.SinPi(k / T.CreateChecked(2));
        }

        public static T InOut<T>(T k) where T : IFloatingPointIeee754<T>
        {
            return T.CreateChecked(0.5) * (T.One - T.CosPi(k));
        }
    };

    public static class Exponential
    {
        public static T In<T>(T k) where T : IFloatingPointIeee754<T>
        {
            return k == T.Zero ? T.Zero : T.Pow(T.CreateChecked(1024), k - T.One);
        }

        public static T Out<T>(T k) where T : IFloatingPointIeee754<T>
        {
            return T.Abs(k - T.One) < T.Epsilon ? T.One : T.One - T.Pow(T.CreateChecked(2), T.CreateChecked(-10) * k);
        }

        public static T InOut<T>(T k) where T : IFloatingPointIeee754<T>
        {
            if (k == T.Zero) return T.Zero;
            if (T.Abs(k - T.One) < T.Epsilon) return T.One;
            if ((k *= T.CreateChecked(2)) < T.One) return T.CreateChecked(0.5) * T.Pow(T.CreateChecked(1024), k - T.One);

            return T.CreateChecked(0.5) * (-T.Pow(T.CreateChecked(2), T.CreateChecked(-10) * (k - T.One)) + T.CreateChecked(2));
        }
    };

    public static class Circular
    {
        public static T In<T>(T k) where T : IFloatingPointIeee754<T>
        {
            return T.One - T.Sqrt(T.One - k * k);
        }

        public static T Out<T>(T k) where T : IFloatingPointIeee754<T>
        {
            return T.Sqrt(T.One - (k -= T.One) * k);
        }

        public static T InOut<T>(T k) where T : IFloatingPointIeee754<T>
        {
            if ((k *= T.CreateChecked(2)) < T.One) return T.CreateChecked(-0.5) * (T.Sqrt(T.One - k * k) - T.One);

            return T.CreateChecked(0.5) * (T.Sqrt(T.One - (k -= T.CreateChecked(2)) * k) + T.One);
        }
    };

    public static class Elastic
    {
        public static T In<T>(T k) where T : IFloatingPointIeee754<T>
        {
            if (k == T.Zero) return T.Zero;
            if (T.Abs(k - T.One) < T.Epsilon) return T.One;

            return -T.Pow(T.CreateChecked(2), T.CreateChecked(10) * (k -= T.One)) * T.Sin((k - T.CreateChecked(0.1)) * (T.CreateChecked(2) * T.Pi) / T.CreateChecked(0.4));
        }

        public static T Out<T>(T k) where T : IFloatingPointIeee754<T>
        {
            if (k == T.Zero) return T.Zero;
            if (T.Abs(k - T.One) < T.Epsilon) return T.One;

            return T.Pow(T.CreateChecked(2), T.CreateChecked(-10) * k) * T.Sin((k - T.CreateChecked(0.1)) * (T.CreateChecked(2) * T.Pi) / T.CreateChecked(0.4)) + T.One;
        }

        public static T InOut<T>(T k) where T : IFloatingPointIeee754<T>
        {
            if ((k *= T.CreateChecked(2)) < T.One) return T.CreateChecked(-0.5) * T.Pow(T.CreateChecked(2), T.CreateChecked(10) * (k -= T.One)) * T.Sin((k - T.CreateChecked(0.1)) * (T.CreateChecked(2) * T.Pi) / T.CreateChecked(0.4));

            return T.Pow(T.CreateChecked(2), T.CreateChecked(-10) * (k -= T.One)) * T.Sin((k - T.CreateChecked(0.1)) * (T.CreateChecked(2) * T.Pi) / T.CreateChecked(0.4)) * T.CreateChecked(0.5) + T.One;
        }
    };

    public static class Back
    {
        public static T In<T>(T k) where T : IFloatingPointIeee754<T>
        {
            var s = T.CreateChecked(1.70158);
            return k * k * ((s + T.One) * k - s);
        }

        public static T Out<T>(T k) where T : IFloatingPointIeee754<T>
        {
            var s = T.CreateChecked(1.70158);
            return (k -= T.One) * k * ((s + T.One) * k + s) + T.One;
        }

        public static T InOut<T>(T k) where T : IFloatingPointIeee754<T>
        {
            var s = T.CreateChecked(2.5949095);

            if ((k *= T.CreateChecked(2)) < T.One) return T.CreateChecked(0.5) * (k * k * ((s + T.One) * k - s));

            return T.CreateChecked(0.5) * ((k -= T.CreateChecked(2)) * k * ((s + T.One) * k + s) + T.CreateChecked(2));
        }
    };

    public static class Bounce
    {
        public static T In<T>(T k) where T : IFloatingPointIeee754<T>
        {
            return T.One - Out(T.One - k);
        }

        public static T Out<T>(T k) where T : IFloatingPointIeee754<T>
        {
            if (k < T.One / T.CreateChecked(2.75)) return T.CreateChecked(7.5625) * k * k;

            if (k < T.CreateChecked(2) / T.CreateChecked(2.75)) return T.CreateChecked(7.5625) * (k -= T.CreateChecked(1.5) / T.CreateChecked(2.75)) * k + T.CreateChecked(0.75);

            if (k < T.CreateChecked(2.5) / T.CreateChecked(2.75)) return T.CreateChecked(7.5625) * (k -= T.CreateChecked(2.25) / T.CreateChecked(2.75)) * k + T.CreateChecked(0.9375);

            return T.CreateChecked(7.5625) * (k -= T.CreateChecked(2.625) / T.CreateChecked(2.75)) * k + T.CreateChecked(0.984375);
        }

        public static T InOut<T>(T k) where T : IFloatingPointIeee754<T>
        {
            if (k < T.CreateChecked(0.5)) return In(k * T.CreateChecked(2)) * T.CreateChecked(0.5);

            return Out(k * T.CreateChecked(2) - T.One) * T.CreateChecked(0.5) + T.CreateChecked(0.5);
        }
    };
}