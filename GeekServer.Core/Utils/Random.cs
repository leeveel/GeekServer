//using System;

//namespace Geek.Server
//{
//    public class Random
//    {
//        private long seed = 100;

//        public Random(long seed)
//        {
//            SetSeed(seed);
//        }

//        public Random() { }

//        public void SetSeed(long seed)
//        {
//            this.seed = (seed ^ 0x5DEECE66DL) & ((1L << 48) - 1);
//        }

//        public int NextInt(int n)
//        {
//            if (n <= 0) throw new ArgumentException("n must be positive");

//            if ((n & -n) == n)  // i.e., n is a power of 2
//            {
//                var ret = (int)((n * (long)Next(31)) >> 31);
//                return ret;
//            }

//            long bits, val;
//            do
//            {
//                bits = Next(31);
//                val = bits % (long)n;
//            }
//            while (bits - val + (n - 1) < 0);
//            return (int)val;
//        }

//        protected long Next(int bits)
//        {
//            seed = (seed * 0x5DEECE66DL + 0xBL) & ((1L << 48) - 1);

//            return (long)(seed >> (48 - bits));
//        }
//    }
//}