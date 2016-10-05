using System;

namespace xbAV.Utilities.Kkzb
{
    public struct Rate
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

        public override bool Equals(object obj)
        {
            if (!(obj is Rate))
            {
                return false;
            }

            return this == (Rate)obj;
        }

        public override int GetHashCode()
        {
            return Decimals + 23 * Exponent;
        }

        public static bool operator ==(Rate r1, Rate r2)
        {
            return r1.Decimals == r2.Decimals && r1.Exponent == r2.Exponent;
        }

        public static bool operator !=(Rate r1, Rate r2)
        {
            return r1.Decimals != r2.Decimals || r1.Exponent != r2.Exponent;
        }
    }
}
