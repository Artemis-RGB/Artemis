#pragma warning disable 1591
using System;

namespace Artemis.Core
{
    public static class Easings
    {
        /// <summary>
        ///     Constant Pi.
        /// </summary>
        private const double PI = Math.PI;

        /// <summary>
        ///     Constant Pi / 2.
        /// </summary>
        private const double HALFPI = Math.PI / 2.0;

        /// <summary>
        ///     Interpolate using the specified function.
        /// </summary>
        public static double Interpolate(double p, Functions function)
        {
            switch (function)
            {
                default:
                case Functions.Step: return Step(p);
                case Functions.Linear: return Linear(p);
                case Functions.QuadraticEaseOut: return QuadraticEaseOut(p);
                case Functions.QuadraticEaseIn: return QuadraticEaseIn(p);
                case Functions.QuadraticEaseInOut: return QuadraticEaseInOut(p);
                case Functions.CubicEaseIn: return CubicEaseIn(p);
                case Functions.CubicEaseOut: return CubicEaseOut(p);
                case Functions.CubicEaseInOut: return CubicEaseInOut(p);
                case Functions.QuarticEaseIn: return QuarticEaseIn(p);
                case Functions.QuarticEaseOut: return QuarticEaseOut(p);
                case Functions.QuarticEaseInOut: return QuarticEaseInOut(p);
                case Functions.QuinticEaseIn: return QuinticEaseIn(p);
                case Functions.QuinticEaseOut: return QuinticEaseOut(p);
                case Functions.QuinticEaseInOut: return QuinticEaseInOut(p);
                case Functions.SineEaseIn: return SineEaseIn(p);
                case Functions.SineEaseOut: return SineEaseOut(p);
                case Functions.SineEaseInOut: return SineEaseInOut(p);
                case Functions.CircularEaseIn: return CircularEaseIn(p);
                case Functions.CircularEaseOut: return CircularEaseOut(p);
                case Functions.CircularEaseInOut: return CircularEaseInOut(p);
                case Functions.ExponentialEaseIn: return ExponentialEaseIn(p);
                case Functions.ExponentialEaseOut: return ExponentialEaseOut(p);
                case Functions.ExponentialEaseInOut: return ExponentialEaseInOut(p);
                case Functions.ElasticEaseIn: return ElasticEaseIn(p);
                case Functions.ElasticEaseOut: return ElasticEaseOut(p);
                case Functions.ElasticEaseInOut: return ElasticEaseInOut(p);
                case Functions.BackEaseIn: return BackEaseIn(p);
                case Functions.BackEaseOut: return BackEaseOut(p);
                case Functions.BackEaseInOut: return BackEaseInOut(p);
                case Functions.BounceEaseIn: return BounceEaseIn(p);
                case Functions.BounceEaseOut: return BounceEaseOut(p);
                case Functions.BounceEaseInOut: return BounceEaseInOut(p);
            }
        }

        /// <summary>
        ///     An snappy animation that moves instantly to the next destination on the next keyframe
        /// </summary>
        public static double Step(double p)
        {
            return Math.Floor (p);
        }

        /// <summary>
        ///     Modeled after the line y = x
        /// </summary>
        public static double Linear(double p)
        {
            return p;
        }

        /// <summary>
        ///     Modeled after the parabola y = x^2
        /// </summary>
        public static double QuadraticEaseIn(double p)
        {
            return p * p;
        }

        /// <summary>
        ///     Modeled after the parabola y = -x^2 + 2x
        /// </summary>
        public static double QuadraticEaseOut(double p)
        {
            return -(p * (p - 2));
        }

        /// <summary>
        ///     Modeled after the piecewise quadratic
        ///     y = (1/2)((2x)^2)             ; [0, 0.5)
        ///     y = -(1/2)((2x-1)*(2x-3) - 1) ; [0.5, 1]
        /// </summary>
        public static double QuadraticEaseInOut(double p)
        {
            if (p < 0.5)
                return 2 * p * p;
            return -2 * p * p + 4 * p - 1;
        }

        /// <summary>
        ///     Modeled after the cubic y = x^3
        /// </summary>
        public static double CubicEaseIn(double p)
        {
            return p * p * p;
        }

        /// <summary>
        ///     Modeled after the cubic y = (x - 1)^3 + 1
        /// </summary>
        public static double CubicEaseOut(double p)
        {
            double f = p - 1;
            return f * f * f + 1;
        }

        /// <summary>
        ///     Modeled after the piecewise cubic
        ///     y = (1/2)((2x)^3)       ; [0, 0.5)
        ///     y = (1/2)((2x-2)^3 + 2) ; [0.5, 1]
        /// </summary>
        public static double CubicEaseInOut(double p)
        {
            if (p < 0.5)
                return 4 * p * p * p;
            double f = 2 * p - 2;
            return 0.5 * f * f * f + 1;
        }

        /// <summary>
        ///     Modeled after the quartic x^4
        /// </summary>
        public static double QuarticEaseIn(double p)
        {
            return p * p * p * p;
        }

        /// <summary>
        ///     Modeled after the quartic y = 1 - (x - 1)^4
        /// </summary>
        public static double QuarticEaseOut(double p)
        {
            double f = p - 1;
            return f * f * f * (1 - p) + 1;
        }

        /// <summary>
        ///     Modeled after the piecewise quartic
        ///     y = (1/2)((2x)^4)        ; [0, 0.5)
        ///     y = -(1/2)((2x-2)^4 - 2) ; [0.5, 1]
        /// </summary>
        public static double QuarticEaseInOut(double p)
        {
            if (p < 0.5)
                return 8 * p * p * p * p;
            double f = p - 1;
            return -8 * f * f * f * f + 1;
        }

        /// <summary>
        ///     Modeled after the quintic y = x^5
        /// </summary>
        public static double QuinticEaseIn(double p)
        {
            return p * p * p * p * p;
        }

        /// <summary>
        ///     Modeled after the quintic y = (x - 1)^5 + 1
        /// </summary>
        public static double QuinticEaseOut(double p)
        {
            double f = p - 1;
            return f * f * f * f * f + 1;
        }

        /// <summary>
        ///     Modeled after the piecewise quintic
        ///     y = (1/2)((2x)^5)       ; [0, 0.5)
        ///     y = (1/2)((2x-2)^5 + 2) ; [0.5, 1]
        /// </summary>
        public static double QuinticEaseInOut(double p)
        {
            if (p < 0.5)
                return 16 * p * p * p * p * p;
            double f = 2 * p - 2;
            return 0.5 * f * f * f * f * f + 1;
        }

        /// <summary>
        ///     Modeled after quarter-cycle of sine wave
        /// </summary>
        public static double SineEaseIn(double p)
        {
            return Math.Sin((p - 1) * HALFPI) + 1;
        }

        /// <summary>
        ///     Modeled after quarter-cycle of sine wave (different phase)
        /// </summary>
        public static double SineEaseOut(double p)
        {
            return Math.Sin(p * HALFPI);
        }

        /// <summary>
        ///     Modeled after half sine wave
        /// </summary>
        public static double SineEaseInOut(double p)
        {
            return 0.5 * (1 - Math.Cos(p * PI));
        }

        /// <summary>
        ///     Modeled after shifted quadrant IV of unit circle
        /// </summary>
        public static double CircularEaseIn(double p)
        {
            return 1 - Math.Sqrt(1 - p * p);
        }

        /// <summary>
        ///     Modeled after shifted quadrant II of unit circle
        /// </summary>
        public static double CircularEaseOut(double p)
        {
            return Math.Sqrt((2 - p) * p);
        }

        /// <summary>
        ///     Modeled after the piecewise circular function
        ///     y = (1/2)(1 - Math.Sqrt(1 - 4x^2))           ; [0, 0.5)
        ///     y = (1/2)(Math.Sqrt(-(2x - 3)*(2x - 1)) + 1) ; [0.5, 1]
        /// </summary>
        public static double CircularEaseInOut(double p)
        {
            if (p < 0.5)
                return 0.5 * (1 - Math.Sqrt(1 - 4 * (p * p)));
            return 0.5 * (Math.Sqrt(-(2 * p - 3) * (2 * p - 1)) + 1);
        }

        /// <summary>
        ///     Modeled after the exponential function y = 2^(10(x - 1))
        /// </summary>
        public static double ExponentialEaseIn(double p)
        {
            return p == 0.0 ? p : Math.Pow(2, 10 * (p - 1));
        }

        /// <summary>
        ///     Modeled after the exponential function y = -2^(-10x) + 1
        /// </summary>
        public static double ExponentialEaseOut(double p)
        {
            return p == 1.0 ? p : 1 - Math.Pow(2, -10 * p);
        }

        /// <summary>
        ///     Modeled after the piecewise exponential
        ///     y = (1/2)2^(10(2x - 1))         ; [0,0.5)
        ///     y = -(1/2)*2^(-10(2x - 1))) + 1 ; [0.5,1]
        /// </summary>
        public static double ExponentialEaseInOut(double p)
        {
            if (p == 0.0 || p == 1.0) return p;

            if (p < 0.5)
                return 0.5 * Math.Pow(2, 20 * p - 10);
            return -0.5 * Math.Pow(2, -20 * p + 10) + 1;
        }

        /// <summary>
        ///     Modeled after the damped sine wave y = sin(13pi/2*x)*Math.Pow(2, 10 * (x - 1))
        /// </summary>
        public static double ElasticEaseIn(double p)
        {
            return Math.Sin(13 * HALFPI * p) * Math.Pow(2, 10 * (p - 1));
        }

        /// <summary>
        ///     Modeled after the damped sine wave y = sin(-13pi/2*(x + 1))*Math.Pow(2, -10x) + 1
        /// </summary>
        public static double ElasticEaseOut(double p)
        {
            return Math.Sin(-13 * HALFPI * (p + 1)) * Math.Pow(2, -10 * p) + 1;
        }

        /// <summary>
        ///     Modeled after the piecewise exponentially-damped sine wave:
        ///     y = (1/2)*sin(13pi/2*(2*x))*Math.Pow(2, 10 * ((2*x) - 1))      ; [0,0.5)
        ///     y = (1/2)*(sin(-13pi/2*((2x-1)+1))*Math.Pow(2,-10(2*x-1)) + 2) ; [0.5, 1]
        /// </summary>
        public static double ElasticEaseInOut(double p)
        {
            if (p < 0.5)
                return 0.5 * Math.Sin(13 * HALFPI * (2 * p)) * Math.Pow(2, 10 * (2 * p - 1));
            return 0.5 * (Math.Sin(-13 * HALFPI * (2 * p - 1 + 1)) * Math.Pow(2, -10 * (2 * p - 1)) + 2);
        }

        /// <summary>
        ///     Modeled after the overshooting cubic y = x^3-x*sin(x*pi)
        /// </summary>
        public static double BackEaseIn(double p)
        {
            return p * p * p - p * Math.Sin(p * PI);
        }

        /// <summary>
        ///     Modeled after overshooting cubic y = 1-((1-x)^3-(1-x)*sin((1-x)*pi))
        /// </summary>
        public static double BackEaseOut(double p)
        {
            double f = 1 - p;
            return 1 - (f * f * f - f * Math.Sin(f * PI));
        }

        /// <summary>
        ///     Modeled after the piecewise overshooting cubic function:
        ///     y = (1/2)*((2x)^3-(2x)*sin(2*x*pi))           ; [0, 0.5)
        ///     y = (1/2)*(1-((1-x)^3-(1-x)*sin((1-x)*pi))+1) ; [0.5, 1]
        /// </summary>
        public static double BackEaseInOut(double p)
        {
            if (p < 0.5)
            {
                double f = 2 * p;
                return 0.5 * (f * f * f - f * Math.Sin(f * PI));
            }
            else
            {
                double f = 1 - (2 * p - 1);
                return 0.5 * (1 - (f * f * f - f * Math.Sin(f * PI))) + 0.5;
            }
        }

        /// <summary>
        /// </summary>
        public static double BounceEaseIn(double p)
        {
            return 1 - BounceEaseOut(1 - p);
        }

        /// <summary>
        /// </summary>
        public static double BounceEaseOut(double p)
        {
            if (p < 4 / 11.0)
                return 121 * p * p / 16.0;
            if (p < 8 / 11.0)
                return 363 / 40.0 * p * p - 99 / 10.0 * p + 17 / 5.0;
            if (p < 9 / 10.0)
                return 4356 / 361.0 * p * p - 35442 / 1805.0 * p + 16061 / 1805.0;
            return 54 / 5.0 * p * p - 513 / 25.0 * p + 268 / 25.0;
        }

        /// <summary>
        /// </summary>
        public static double BounceEaseInOut(double p)
        {
            if (p < 0.5)
                return 0.5 * BounceEaseIn(p * 2);
            return 0.5 * BounceEaseOut(p * 2 - 1) + 0.5;
        }

        /// <summary>
        ///     Easing Functions enumeration
        /// </summary>
        public enum Functions
        {
            Step,
            Linear,
            QuadraticEaseIn,
            QuadraticEaseOut,
            QuadraticEaseInOut,
            CubicEaseIn,
            CubicEaseOut,
            CubicEaseInOut,
            QuarticEaseIn,
            QuarticEaseOut,
            QuarticEaseInOut,
            QuinticEaseIn,
            QuinticEaseOut,
            QuinticEaseInOut,
            SineEaseIn,
            SineEaseOut,
            SineEaseInOut,
            CircularEaseIn,
            CircularEaseOut,
            CircularEaseInOut,
            ExponentialEaseIn,
            ExponentialEaseOut,
            ExponentialEaseInOut,
            ElasticEaseIn,
            ElasticEaseOut,
            ElasticEaseInOut,
            BackEaseIn,
            BackEaseOut,
            BackEaseInOut,
            BounceEaseIn,
            BounceEaseOut,
            BounceEaseInOut
        }
    }
}