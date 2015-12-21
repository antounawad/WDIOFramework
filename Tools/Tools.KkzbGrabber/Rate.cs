using System;

namespace Tools.KkzbGrabber
{
    struct Rate
    {
        public Rate(int decimals, int exponent)
        {
            while (decimals != 0 && decimals % 10 == 0)
            {
                decimals /= 10;
                ++exponent;
            }

            Decimals = decimals;
            Exponent = exponent;
        }

        public int Decimals { get; }
        public int Exponent { get; }

        public static implicit operator decimal(Rate rate)
        {
            return rate.Decimals * (rate.Exponent < 0 ? 1m / PowerOfTen(-rate.Exponent) : PowerOfTen(rate.Exponent));
        }

        private static int PowerOfTen(int exponent)
        {
            switch (exponent)
            {
                case 0:
                    return 1;
                case 1:
                    return 10;
                default:
                    if(exponent < 0) throw new ArgumentOutOfRangeException();

                    var e1 = exponent >> 1;
                    return PowerOfTen(e1) * PowerOfTen(exponent - e1);
            }
        }

        public override string ToString()
        {
            return $"{Decimals}*10^{Exponent}";
        }
    }
}
