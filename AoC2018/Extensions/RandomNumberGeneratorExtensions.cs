using System;
using System.Linq;
using System.Security.Cryptography;

namespace AoC2018.Extensions
{
    public static class RandomNumberGeneratorExtensions
    {
        public static int GetBetterRandom(this RandomNumberGenerator generator, int maxValue)
        {
            var bytesNeeded = (int) Math.Ceiling((decimal) maxValue / 255);
            var numbers = new byte[bytesNeeded];

            generator.GetBytes(numbers);

            var number = numbers.Sum(f => f);

            return number >= maxValue ? generator.GetBetterRandom(maxValue) : number;
        }
    }
}