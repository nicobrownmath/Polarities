using Microsoft.Xna.Framework;
using System;
using Terraria.Utilities;

namespace Polarities
{
    //Basically a set of pre-generated random data with methods
    //based on a system from Nalyddd
    public class PreGeneratedRandom
    {
        private int seedCache;
        private byte[] samples;
        private int sampleIndex;

        public PreGeneratedRandom(int seed, int capacity = 8)
        {
            seedCache = seed;
            samples = new byte[capacity];
            new UnifiedRandom(seed).NextBytes(samples);
        }

        public void SetIndex(int index)
        {
            sampleIndex = index % samples.Length;
        }

        private byte NextByte()
        {
            sampleIndex = (sampleIndex + 1) % samples.Length;
            return samples[sampleIndex];
        }

        public int Next()
        {
            return Math.Abs(BitConverter.ToInt32(new byte[] { NextByte(), NextByte(), NextByte(), NextByte() }));
        }

        //copied from Random.Random
        private double Sample()
        {
            return Next() * 4.656612875245797E-10;
        }

        private double GetSampleForLargeRange()
        {
            int num = Next();
            if ((Next() % 2 == 0) ? true : false)
            {
                num = -num;
            }
            double num2 = num;
            num2 += 2147483646.0;
            return num2 / 4294967293.0;
        }

        public double NextDouble()
        {
            return Sample();
        }

        public int Next(int maxValue)
        {
            if (maxValue < 0) throw new ArgumentOutOfRangeException("maxValue must be positive");
            return (int)(Sample() * maxValue);
        }

        public int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue) throw new ArgumentOutOfRangeException("minValue must be less than maxValue");
            long num = maxValue - (long)minValue;
            if (num <= int.MaxValue)
            {
                return (int)(Sample() * num) + minValue;
            }
            return (int)((long)(GetSampleForLargeRange() * num) + minValue);
        }

        //copied from Terraria.Utilities.UnifiedRandom
        public float NextFloat()
        {
            return (float)NextDouble();
        }

        public float NextFloat(float maxValue)
        {
            return (float)NextDouble() * maxValue;
        }

        public float NextFloat(float minValue, float maxValue)
        {
            return (float)NextDouble() * (maxValue - minValue) + minValue;
        }

        public float NextNormallyDistributedFloat(float timeMultiplier = 1)
        {
            return (float)(Math.Sqrt(-2 * Math.Log(NextFloat())) * Math.Cos(NextFloat(MathHelper.TwoPi)) * Math.Sqrt(timeMultiplier));
        }
    }
}

