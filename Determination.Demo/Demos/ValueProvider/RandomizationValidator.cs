using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Determination.Demo
{
    public enum RandomizationKind { RandomStandard, RandomCrypto }

    internal class RandomizationValidator
    {
        private static readonly Random _RANDOM                                     = new Random();
        private static readonly RNGCryptoServiceProvider _RNGCRYPTOSERVICEPROVIDER = new RNGCryptoServiceProvider();
        private readonly IValueProvider<int> _valueProvider;

        public RandomizationValidator(RandomizationKind randomizationKind, int minimum, int maximum)
        {
            RandomizationKind = randomizationKind;
            Minimum           = minimum;
            Maximum           = maximum;
            _valueProvider    = GetValueProvider(randomizationKind);
        }

        private IValueProvider<int> GetValueProvider(RandomizationKind randomizationKind)
        {
            IValueProvider<int> valueProvider = randomizationKind switch
            {
                RandomizationKind.RandomStandard => ValueProvider.Create(GetNextRandomStandardValue),
                RandomizationKind.RandomCrypto   => ValueProvider.Create(GetNextRandomCryptoValue),
                _                                => throw new InvalidOperationException($"Unexpected value for argument {nameof(randomizationKind)} ({randomizationKind})."),
            };
            return valueProvider;
        }

        public RandomizationKind RandomizationKind { get; }

        /// <summary>
        /// Minimum randomized value (inclusive) that should be returned by the randomization algorithm.
        /// </summary>
        public int Minimum { get; }

        /// <summary>
        /// Maximum randomized value (non-inclusive) that should be returned by the randomization algorithm.
        /// </summary>
        public int Maximum { get; }

        /// <summary>
        /// Gets the next random value.
        /// </summary>
        /// <returns>The next random value</returns>
        public int Next() => _valueProvider.Value;

        private int GetNextRandomStandardValue() => _RANDOM.Next(Minimum, Maximum);

        private int GetNextRandomCryptoValue() => Next(_RNGCRYPTOSERVICEPROVIDER, Minimum, Maximum);

        private static int Next(RNGCryptoServiceProvider rngCryptoServiceProvider, int minimum, int maximum)
        {
            const int byteCount = sizeof(int);
            byte[] bytes = new byte[byteCount];
            rngCryptoServiceProvider.GetBytes(bytes);
            UInt32 scale = BitConverter.ToUInt32(bytes, 0);
            return (int)(minimum + (maximum - minimum) * (scale / (uint.MaxValue + 1.0)));
        }
    }

    public class RandomizationValidator_Tests
    {
        [TestCase(RandomizationKind.RandomStandard, 1, 1001)]
        [TestCase(RandomizationKind.RandomCrypto,   1, 1001)]
        [MaxTime(100)]
        [Category("2 - Demo - Randomization - RandomizationValidator Tests")]
        // This test validates that randomized values include all
        // the possible values between a minimum (inclusive)
        // and a maximum (exclusive) value.
        // Note: This test is not deterministic:  if the test
        // passes then we can be absolutely certain that
        // the randomization algorithm is capable of producing
        // all of the expected values; on the other hand a test
        // failure does not necessarily imply a faulty algorithm
        // as the failure could be due to the test taking longer
        // to execute than the time allowed by the test's
        // MaxTime attribute.
        public void Test1(RandomizationKind randomizationKind, int minValue, int maxValue)
        {
            const int defaultValue = 0;
            Dictionary<int, int> dictionary = new Dictionary<int, int>(maxValue - minValue);
            for (int i = minValue; i <= maxValue - minValue; i++)
                dictionary.Add(i, defaultValue);
            RandomizationValidator randomizationValidator = new RandomizationValidator(randomizationKind, minValue, maxValue);
            while (dictionary.Values.Any(value => value == defaultValue))
            {
                int randomValue = randomizationValidator.Next();
                if (dictionary[randomValue] == 0)
                    dictionary[randomValue] = randomValue;
            }
        }

        [TestCase(RandomizationKind.RandomStandard, 1, 1001)]
        [TestCase(RandomizationKind.RandomCrypto,   1, 1001)]
        [MaxTime(100)]
        [Category("2 - Demo - Randomization - RandomizationValidator Tests")]
        // This test validates that randomized values include all
        // the possible values between a minimum (inclusive)
        // and a maximum (exclusive) value.
        // Note: This test is not deterministic:  if the test
        // passes then we can be absolutely certain that
        // the randomization algorithm is capable of producing
        // all of the expected values; on the other hand a test
        // failure does not necessarily imply a faulty algorithm
        // as the failure could be due to the test taking longer
        // to execute than the time allowed by the test's
        // MaxTime attribute.
        public void Test2(RandomizationKind randomizationKind, int minValue, int maxValue)
        {
            HashSet<int> hashSet                          = new HashSet<int>();
            RandomizationValidator randomizationValidator = new RandomizationValidator(randomizationKind, minValue, maxValue);
            while (hashSet.Count < maxValue - minValue)
            {
                int randomValue = randomizationValidator.Next();
                if (!hashSet.Contains(randomValue))
                    hashSet.Add(randomValue);
            }
        }

        [TestCase(RandomizationKind.RandomStandard, 1, 101, 100_000, 10)]
        [TestCase(RandomizationKind.RandomCrypto,   1, 101, 100_000, 10)]
        [Category("2 - Demo - Randomization - RandomizationValidator Tests")]
        // Validates the evenness of the distribution of randomly generated values.
        // This test is non-deterministic as the likelyhood of
        // an even distribution of random values increases with
        // the number of random values generated; the test might
        // fail with a small number of iterations and succeed with
        // a larger number of iterations.
        public void Test3(RandomizationKind randomizationKind, int minValue, int maxValue, int iterations, float maxDeviation)
        {
            const int defaultValue = 0;
            int entryCount = maxValue - minValue;
            Dictionary<int, int> dictionary = new Dictionary<int, int>(entryCount);
            for (int i = minValue; i <= maxValue - minValue; i++)
                dictionary.Add(i, defaultValue);
            RandomizationValidator randomizationValidator = new RandomizationValidator(randomizationKind, minValue, maxValue);
            for (int i = 1; i <= iterations; i++)
            {
                int randomValue = randomizationValidator.Next();
                dictionary[randomValue] = dictionary[randomValue] + 1;
            }
            int minFrequency = dictionary.Values.Min();
            int maxFrequency = dictionary.Values.Max();
            float perfectFrequency = iterations / (float)entryCount;
            float allowedFrequency = perfectFrequency * maxDeviation / 100;
            float minAllowedFrequency = perfectFrequency - allowedFrequency;
            float maxAllowedFrequency = perfectFrequency + allowedFrequency;
            Assert.GreaterOrEqual(minFrequency, minAllowedFrequency);
            Assert.LessOrEqual(maxFrequency, maxAllowedFrequency);
        }
    }
}
