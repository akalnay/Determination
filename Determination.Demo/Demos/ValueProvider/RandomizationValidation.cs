using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Determination.Demo
{
    public enum RandomizationKind { RandomStandard, RandomCrypto }

    public abstract class RandomizationValidation_TestsBase
    {
        protected static class TestCases
        {
            private static readonly Random _RANDOM                                     = new Random();
            private static readonly RNGCryptoServiceProvider _RNGCRYPTOSERVICEPROVIDER = new RNGCryptoServiceProvider();

            private static int GetNextRandomStandardValue(int min, int max) => _RANDOM.Next(min, max);

            private static int GetNextRandomCryptoValue(int min, int max) => Next(_RNGCRYPTOSERVICEPROVIDER, min, max);

            private static int Next(RNGCryptoServiceProvider rngCryptoServiceProvider, int minimum, int maximum)
            {
                const int byteCount = sizeof(int);
                byte[] bytes = new byte[byteCount];
                rngCryptoServiceProvider.GetBytes(bytes);
                UInt32 scale = BitConverter.ToUInt32(bytes, 0);
                return (int)(minimum + (maximum - minimum) * (scale / (uint.MaxValue + 1.0)));
            }

            public static IEnumerable<TestCaseData> TestCase1(string testName)
            {
                const int minValue                              = 1;
                const int maxValue                              = 1001;
                int count                                       = maxValue - minValue;
                IValueProvider<int> valueProviderRandomStandard = ValueProvider.Create(() => GetNextRandomStandardValue(minValue, maxValue));
                IValueProvider<int> valueProviderRandomCrypto   = ValueProvider.Create(() => GetNextRandomCryptoValue(minValue, maxValue));
                IValueProvider<int> valueProviderStub           = ValueProviderStub.Create(Enumerable.Range(minValue, count).ToArray());

                yield return new TestCaseData(valueProviderRandomStandard, count).SetName($"{testName} - Random Standard");
                yield return new TestCaseData(valueProviderRandomCrypto, count).SetName($"{testName} - Random Crypto");
                yield return new TestCaseData(valueProviderStub, count).SetName($"{testName} - ValueProviderStub");
            }

            public static IEnumerable<TestCaseData> TestCase2(string testName)
            {
                const int minValue              = 1;
                const int maxValue              = 101;
                const int iterations            = 1_000_000;
                const float maxDeviationPercent = 10;
                int count                       = maxValue - minValue;
                int[] values1                   = Enumerable.Range(minValue, count).ToArray();
                int[] values2                   = values1.Select(value => value == minValue + 1 ? minValue : value).ToArray();
                int[] values3                   = values1.Select(value => value == count - 1 ? count : value).ToArray();
                IValueProvider<int> valueProviderRandomStandard = ValueProvider.Create(() => GetNextRandomStandardValue(minValue, maxValue));
                IValueProvider<int> valueProviderRandomCrypto   = ValueProvider.Create(() => GetNextRandomCryptoValue(minValue, maxValue));
                IValueProvider<int> valueProviderStub1          = ValueProviderStub.Create(values1);
                IValueProvider<int> valueProviderStub2          = ValueProviderStub.Create(values2);
                IValueProvider<int> valueProviderStub3          = ValueProviderStub.Create(values3);

                yield return new TestCaseData(valueProviderRandomStandard, minValue, maxValue, iterations, maxDeviationPercent).SetName($"{testName} - Random Standard").Returns(true);
                yield return new TestCaseData(valueProviderRandomCrypto, minValue, maxValue, iterations, maxDeviationPercent).SetName($"{testName} - Random Crypto").Returns(true);
                yield return new TestCaseData(valueProviderStub1, minValue, maxValue, count, maxDeviationPercent).SetName($"{testName} - ValueProviderStub - 1").Returns(true);
                yield return new TestCaseData(valueProviderStub2, minValue, maxValue, count, maxDeviationPercent).SetName($"{testName} - ValueProviderStub - 2").Returns(false);
                yield return new TestCaseData(valueProviderStub3, minValue, maxValue, count, maxDeviationPercent).SetName($"{testName} - ValueProviderStub - 3").Returns(false);
            }
        }
    }

    public class RandomizationValidation_Tests : RandomizationValidation_TestsBase
    {
        [TestCaseSource(typeof(TestCases), nameof(TestCases.TestCase1), new object[] { nameof(Test1) } )]
        [MaxTime(100)]
        [Category("2 - Demo - Randomization - RandomizationValidation Tests")]
        // This test validates that randomized values include all
        // the possible values between a minimum (inclusive)
        // and a maximum (exclusive) value.
        //
        // Note 1: This test is not deterministic:  if the test
        // passes then we can be absolutely certain that
        // the randomization algorithm is capable of producing
        // all of the expected values, however that will not
        // indicate that the algorithm is correct at it
        // could be possible that if given enough iterations the algorithm
        // might return a value that is outside of the valid range.
        // Also, a test failure does not necessarily imply a faulty algorithm
        // as the failure could be due to the test taking longer
        // to execute than the time allowed by the test's
        // MaxTime attribute.
        //
        // Note 2:  While this is an instance where the
        // Determination API can't assist in making a given test
        // deterministic, it is still useful as a "sanity check"
        // for the test being run as it proves that the test
        // passes when the randomization algorithm returns all
        // the possible expected values.
        public void WhenAllRandomizedValuesInARangeAreGenerated_ThenTheTestPasses(IValueProvider<int> valueProvider, int count)
        {
            HashSet<int> hashSet = new HashSet<int>();
            while (hashSet.Count < count)
            {
                int randomValue = valueProvider.Value;
                if (!hashSet.Contains(randomValue))
                    hashSet.Add(randomValue);
            }
        }

        [TestCaseSource(typeof(TestCases), nameof(TestCases.TestCase2), new object[] { nameof(Test2) })]
        [Category("2 - Demo - Randomization - RandomizationValidation Tests")]
        // This test validates the evenness of the distribution of
        // randomly generated values.
        //
        // Note 1:  This test is non-deterministic because the 
        // likelyhood of an even distribution of random values
        // depends on the number of random values generated;
        // the test might fail with a small number of iterations
        // but succeed with a large number.
        //
        // Note 2:  While this is an instance where the
        // Determination API can't assist in making a given test
        // deterministic, the API is still useful as a "sanity check"
        // for the test being run as it proves that the when
        // the randomization values are evenly distributed
        // then the test passes.
        public bool WhenTheRandomizedValuesAreEvenlyDistributed_ThenTheTestPasses(IValueProvider<int> valueProvider, int minValue, int maxValue, int iterations, float maxDeviationPercent)
        {
            static Dictionary<int, int> GetDictionary(int minValue, int entryCount)
            {
                Dictionary<int, int> dictionary = new Dictionary<int, int>(entryCount);
                for (int i = minValue; i <= entryCount; i++)                    // Add entries to the dictionary
                    dictionary.Add(i, 0);                                       // and initialize them to zero
                return dictionary;
            }

            static void RandomizeValues(IValueProvider<int> valueProvider, Dictionary<int, int> dictionary, int iterations)
            {
                for (int i = 1; i <= iterations; i++)
                {
                    int randomValue         = valueProvider.Value;              // Get a random value
                    dictionary[randomValue] = dictionary[randomValue] + 1;      // Increase the count of how many times
                                                                                // a given random value appears
                }
            }

            static (int minFrequency, float minAllowedFrequency, int maxFrequency, float maxAllowedFrequency) GetFrequencies(IValueProvider<int> valueProvider, int minValue, int maxValue, int iterations, float maxDeviationPercent)
            {
                int entryCount                  = maxValue - minValue;
                float perfectFrequency          = iterations / (float)entryCount;
                Dictionary<int, int> dictionary = GetDictionary(minValue, entryCount);
                RandomizeValues(valueProvider, dictionary, iterations);
                int minFrequency          = dictionary.Values.Min();
                int maxFrequency          = dictionary.Values.Max();
                float allowedFrequency    = perfectFrequency * maxDeviationPercent / 100;
                float minAllowedFrequency = perfectFrequency - allowedFrequency;
                float maxAllowedFrequency = perfectFrequency + allowedFrequency;
                return (minFrequency, minAllowedFrequency, maxFrequency, maxAllowedFrequency);
            }

            (int minFrequency, float minAllowedFrequency, int maxFrequency, float maxAllowedFrequency) = GetFrequencies(valueProvider, minValue, maxValue, iterations, maxDeviationPercent);
            return minFrequency >= minAllowedFrequency && maxFrequency <= maxAllowedFrequency;
        }
    }
}
