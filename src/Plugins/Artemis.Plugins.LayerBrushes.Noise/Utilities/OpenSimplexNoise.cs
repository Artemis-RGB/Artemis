using System;
using System.Runtime.CompilerServices;

namespace Artemis.Plugins.LayerBrushes.Noise.Utilities
{
    public class OpenSimplexNoise
    {
        private const double STRETCH_2D = -0.211324865405187; //(1/Math.sqrt(2+1)-1)/2;
        private const double STRETCH_3D = -1.0 / 6.0; //(1/Math.sqrt(3+1)-1)/3;
        private const double SQUISH_2D = 0.366025403784439; //(Math.sqrt(2+1)-1)/2;
        private const double SQUISH_3D = 1.0 / 3.0; //(Math.sqrt(3+1)-1)/3;
        private const double NORM_2D = 1.0 / 47.0;
        private const double NORM_3D = 1.0 / 103.0;

        private static readonly double[] gradients2D =
        {
            5, 2, 2, 5,
            -5, 2, -2, 5,
            5, -2, 2, -5,
            -5, -2, -2, -5
        };

        private static readonly double[] gradients3D =
        {
            -11, 4, 4, -4, 11, 4, -4, 4, 11,
            11, 4, 4, 4, 11, 4, 4, 4, 11,
            -11, -4, 4, -4, -11, 4, -4, -4, 11,
            11, -4, 4, 4, -11, 4, 4, -4, 11,
            -11, 4, -4, -4, 11, -4, -4, 4, -11,
            11, 4, -4, 4, 11, -4, 4, 4, -11,
            -11, -4, -4, -4, -11, -4, -4, -4, -11,
            11, -4, -4, 4, -11, -4, 4, -4, -11
        };
        
        private static readonly Contribution2[] lookup2D;
        private static readonly Contribution3[] lookup3D;

        private readonly byte[] perm;
        private readonly byte[] perm2D;
        private readonly byte[] perm3D;
        private readonly byte[] perm4D;

        static OpenSimplexNoise()
        {
            var base2D = new[]
            {
                new[] {1, 1, 0, 1, 0, 1, 0, 0, 0},
                new[] {1, 1, 0, 1, 0, 1, 2, 1, 1}
            };
            var p2D = new[] {0, 0, 1, -1, 0, 0, -1, 1, 0, 2, 1, 1, 1, 2, 2, 0, 1, 2, 0, 2, 1, 0, 0, 0};
            var lookupPairs2D = new[] {0, 1, 1, 0, 4, 1, 17, 0, 20, 2, 21, 2, 22, 5, 23, 5, 26, 4, 39, 3, 42, 4, 43, 3};

            var contributions2D = new Contribution2[p2D.Length / 4];
            for (var i = 0; i < p2D.Length; i += 4)
            {
                var baseSet = base2D[p2D[i]];
                Contribution2 previous = null, current = null;
                for (var k = 0; k < baseSet.Length; k += 3)
                {
                    current = new Contribution2(baseSet[k], baseSet[k + 1], baseSet[k + 2]);
                    if (previous == null)
                        contributions2D[i / 4] = current;
                    else
                        previous.Next = current;
                    previous = current;
                }

                current.Next = new Contribution2(p2D[i + 1], p2D[i + 2], p2D[i + 3]);
            }

            lookup2D = new Contribution2[64];
            for (var i = 0; i < lookupPairs2D.Length; i += 2)
                lookup2D[lookupPairs2D[i]] = contributions2D[lookupPairs2D[i + 1]];


            var base3D = new[]
            {
                new[] {0, 0, 0, 0, 1, 1, 0, 0, 1, 0, 1, 0, 1, 0, 0, 1},
                new[] {2, 1, 1, 0, 2, 1, 0, 1, 2, 0, 1, 1, 3, 1, 1, 1},
                new[] {1, 1, 0, 0, 1, 0, 1, 0, 1, 0, 0, 1, 2, 1, 1, 0, 2, 1, 0, 1, 2, 0, 1, 1}
            };
            var p3D = new[]
            {
                0, 0, 1, -1, 0, 0, 1, 0, -1, 0, 0, -1, 1, 0, 0, 0, 1, -1, 0, 0, -1, 0, 1, 0, 0, -1, 1, 0, 2, 1, 1, 0, 1, 1, 1, -1, 0, 2, 1, 0, 1, 1, 1, -1, 1, 0, 2, 0, 1, 1, 1, -1, 1, 1, 1, 3, 2, 1,
                0, 3, 1, 2, 0, 1, 3, 2, 0, 1, 3, 1, 0, 2, 1, 3, 0, 2, 1, 3, 0, 1, 2, 1, 1, 1, 0, 0, 2, 2, 0, 0, 1, 1, 0, 1, 0, 2, 0, 2, 0, 1, 1, 0, 0, 1, 2, 0, 0, 2, 2, 0, 0, 0, 0, 1, 1, -1, 1, 2, 0,
                0, 0, 0, 1, -1, 1, 1, 2, 0, 0, 0, 0, 1, 1, 1, -1, 2, 3, 1, 1, 1, 2, 0, 0, 2, 2, 3, 1, 1, 1, 2, 2, 0, 0, 2, 3, 1, 1, 1, 2, 0, 2, 0, 2, 1, 1, -1, 1, 2, 0, 0, 2, 2, 1, 1, -1, 1, 2, 2, 0,
                0, 2, 1, -1, 1, 1, 2, 0, 0, 2, 2, 1, -1, 1, 1, 2, 0, 2, 0, 2, 1, 1, 1, -1, 2, 2, 0, 0, 2, 1, 1, 1, -1, 2, 0, 2, 0
            };
            var lookupPairs3D = new[]
            {
                0, 2, 1, 1, 2, 2, 5, 1, 6, 0, 7, 0, 32, 2, 34, 2, 129, 1, 133, 1, 160, 5, 161, 5, 518, 0, 519, 0, 546, 4, 550, 4, 645, 3, 647, 3, 672, 5, 673, 5, 674, 4, 677, 3, 678, 4, 679, 3, 680,
                13, 681, 13, 682, 12, 685, 14, 686, 12, 687, 14, 712, 20, 714, 18, 809, 21, 813, 23, 840, 20, 841, 21, 1198, 19, 1199, 22, 1226, 18, 1230, 19, 1325, 23, 1327, 22, 1352, 15, 1353, 17,
                1354, 15, 1357, 17, 1358, 16, 1359, 16, 1360, 11, 1361, 10, 1362, 11, 1365, 10, 1366, 9, 1367, 9, 1392, 11, 1394, 11, 1489, 10, 1493, 10, 1520, 8, 1521, 8, 1878, 9, 1879, 9, 1906, 7,
                1910, 7, 2005, 6, 2007, 6, 2032, 8, 2033, 8, 2034, 7, 2037, 6, 2038, 7, 2039, 6
            };

            var contributions3D = new Contribution3[p3D.Length / 9];
            for (var i = 0; i < p3D.Length; i += 9)
            {
                var baseSet = base3D[p3D[i]];
                Contribution3 previous = null, current = null;
                for (var k = 0; k < baseSet.Length; k += 4)
                {
                    current = new Contribution3(baseSet[k], baseSet[k + 1], baseSet[k + 2], baseSet[k + 3]);
                    if (previous == null)
                        contributions3D[i / 9] = current;
                    else
                        previous.Next = current;
                    previous = current;
                }

                current.Next = new Contribution3(p3D[i + 1], p3D[i + 2], p3D[i + 3], p3D[i + 4]);
                current.Next.Next = new Contribution3(p3D[i + 5], p3D[i + 6], p3D[i + 7], p3D[i + 8]);
            }

            lookup3D = new Contribution3[2048];
            for (var i = 0; i < lookupPairs3D.Length; i += 2)
                lookup3D[lookupPairs3D[i]] = contributions3D[lookupPairs3D[i + 1]];
        }

        public OpenSimplexNoise()
            : this(DateTime.Now.Ticks)
        {
        }

        public OpenSimplexNoise(long seed)
        {
            perm = new byte[256];
            perm2D = new byte[256];
            perm3D = new byte[256];
            perm4D = new byte[256];
            var source = new byte[256];
            for (var i = 0; i < 256; i++)
                source[i] = (byte) i;
            seed = seed * 6364136223846793005L + 1442695040888963407L;
            seed = seed * 6364136223846793005L + 1442695040888963407L;
            seed = seed * 6364136223846793005L + 1442695040888963407L;
            for (var i = 255; i >= 0; i--)
            {
                seed = seed * 6364136223846793005L + 1442695040888963407L;
                var r = (int) ((seed + 31) % (i + 1));
                if (r < 0)
                    r += i + 1;
                perm[i] = source[r];
                perm2D[i] = (byte) (perm[i] & 0x0E);
                perm3D[i] = (byte) (perm[i] % 24 * 3);
                perm4D[i] = (byte) (perm[i] & 0xFC);
                source[r] = source[i];
            }
        }

        public double Evaluate(double x, double y)
        {
            var stretchOffset = (x + y) * STRETCH_2D;
            var xs = x + stretchOffset;
            var ys = y + stretchOffset;

            var xsb = FastFloor(xs);
            var ysb = FastFloor(ys);

            var squishOffset = (xsb + ysb) * SQUISH_2D;
            var dx0 = x - (xsb + squishOffset);
            var dy0 = y - (ysb + squishOffset);

            var xins = xs - xsb;
            var yins = ys - ysb;

            var inSum = xins + yins;

            var hash =
                (int) (xins - yins + 1) |
                ((int) inSum << 1) |
                ((int) (inSum + yins) << 2) |
                ((int) (inSum + xins) << 4);

            var c = lookup2D[hash];

            var value = 0.0;
            while (c != null)
            {
                var dx = dx0 + c.dx;
                var dy = dy0 + c.dy;
                var attn = 2 - dx * dx - dy * dy;
                if (attn > 0)
                {
                    var px = xsb + c.xsb;
                    var py = ysb + c.ysb;

                    var i = perm2D[(perm[px & 0xFF] + py) & 0xFF];
                    var valuePart = gradients2D[i] * dx + gradients2D[i + 1] * dy;

                    attn *= attn;
                    value += attn * attn * valuePart;
                }

                c = c.Next;
            }

            return value * NORM_2D;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Evaluate(double x, double y, double z)
        {
            var stretchOffset = (x + y + z) * STRETCH_3D;
            var xs = x + stretchOffset;
            var ys = y + stretchOffset;
            var zs = z + stretchOffset;

            var xsb = FastFloor(xs);
            var ysb = FastFloor(ys);
            var zsb = FastFloor(zs);

            var squishOffset = (xsb + ysb + zsb) * SQUISH_3D;
            var dx0 = x - (xsb + squishOffset);
            var dy0 = y - (ysb + squishOffset);
            var dz0 = z - (zsb + squishOffset);

            var xins = xs - xsb;
            var yins = ys - ysb;
            var zins = zs - zsb;

            var inSum = xins + yins + zins;

            var hash =
                (int) (yins - zins + 1) |
                ((int) (xins - yins + 1) << 1) |
                ((int) (xins - zins + 1) << 2) |
                ((int) inSum << 3) |
                ((int) (inSum + zins) << 5) |
                ((int) (inSum + yins) << 7) |
                ((int) (inSum + xins) << 9);

            var c = lookup3D[hash];

            var value = 0.0;
            while (c != null)
            {
                var dx = dx0 + c.dx;
                var dy = dy0 + c.dy;
                var dz = dz0 + c.dz;
                var attn = 2 - dx * dx - dy * dy - dz * dz;
                if (attn > 0)
                {
                    var px = xsb + c.xsb;
                    var py = ysb + c.ysb;
                    var pz = zsb + c.zsb;

                    var i = perm3D[(perm[(perm[px & 0xFF] + py) & 0xFF] + pz) & 0xFF];
                    var valuePart = gradients3D[i] * dx + gradients3D[i + 1] * dy + gradients3D[i + 2] * dz;

                    attn *= attn;
                    value += attn * attn * valuePart;
                }

                c = c.Next;
            }

            return value * NORM_3D;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FastFloor(double x)
        {
            var xi = (int) x;
            return x < xi ? xi - 1 : xi;
        }

        private class Contribution2
        {
            public readonly double dx;
            public readonly double dy;
            public readonly int xsb;
            public readonly int ysb;
            public Contribution2 Next;

            public Contribution2(double multiplier, int xsb, int ysb)
            {
                dx = -xsb - multiplier * SQUISH_2D;
                dy = -ysb - multiplier * SQUISH_2D;
                this.xsb = xsb;
                this.ysb = ysb;
            }
        }

        private class Contribution3
        {
            public readonly double dx;
            public readonly double dy;
            public readonly double dz;
            public readonly int xsb;
            public readonly int ysb;
            public readonly int zsb;
            public Contribution3 Next;

            public Contribution3(double multiplier, int xsb, int ysb, int zsb)
            {
                dx = -xsb - multiplier * SQUISH_3D;
                dy = -ysb - multiplier * SQUISH_3D;
                dz = -zsb - multiplier * SQUISH_3D;
                this.xsb = xsb;
                this.ysb = ysb;
                this.zsb = zsb;
            }
        }
    }
}