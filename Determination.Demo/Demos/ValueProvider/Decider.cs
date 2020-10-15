////////////////////////////////////////////////////////
// Copyright (c) Alejandro Kalnay                     //
// License: GNU GPLv3                                 //
////////////////////////////////////////////////////////

using NUnit.Framework;
using System;
using System.Security.Cryptography;

namespace Determination.Demo
{
    public enum DecisionKind { RandomStandard, RandomCrypto }

    /// <summary>
    /// The <see cref="Decider"/> class demonstrates using the Determination API to select
    /// at runtime which particular implementation of a random number generator
    /// should be used.
    /// The <see cref="Decider.Decide(DecisionKind)"/> method returns a random <see cref="bool"/>
    /// value.  The method uses the decisionKind parameter to determine
    /// which random number implementation to use.
    /// </summary>
    internal class Decider
    {
        private static readonly Random _RANDOM                                     = new Random();
        private static readonly RNGCryptoServiceProvider _RNGCRYPTOSERVICEPROVIDER = new RNGCryptoServiceProvider();
        private static readonly IValueProvider<bool> _VALUEPROVIDERRANDOMSTANDARD  = ValueProvider.Create(GetNextRandomStandardValue);
        private static readonly IValueProvider<bool> _VALUEPROVIDERRANDOMCRYPTO    = ValueProvider.Create(GetNextRandomCryptoValue);

        private static bool GetNextRandomStandardValue() => Convert.ToBoolean(_RANDOM.Next(0, 2));

        private static bool GetNextRandomCryptoValue() => Convert.ToBoolean(Next(_RNGCRYPTOSERVICEPROVIDER, 0, 2));

        private static int Next(RNGCryptoServiceProvider rngCryptoServiceProvider, int minimum, int maximum)
        {
            const int byteCount = sizeof(int);
            byte[] bytes = new byte[byteCount];
            rngCryptoServiceProvider.GetBytes(bytes);
            UInt32 scale = BitConverter.ToUInt32(bytes, 0);
            return (int)(minimum + (maximum - minimum) * (scale / (uint.MaxValue + 1.0)));
        }

        /// <summary>
        /// Returns a random boolean value.
        /// </summary>
        /// <param name="decisionKind">Determines how the random value is generated.</param>
        /// <returns>A random boolean value.</returns>
        public static bool Decide(DecisionKind decisionKind)
        {
            IValueProvider<bool> valueProvider = GetValueProvider(decisionKind);
            return valueProvider.Value;
        }

        private static IValueProvider<bool> GetValueProvider(DecisionKind decisionKind)
        {
            IValueProvider<bool> valueProvider = decisionKind switch
            {
                DecisionKind.RandomStandard => _VALUEPROVIDERRANDOMSTANDARD,
                DecisionKind.RandomCrypto   => _VALUEPROVIDERRANDOMCRYPTO,
                _                           => throw new InvalidOperationException($"Unexpected value for {nameof(DecisionKind)} ({decisionKind}).")
            };
            return valueProvider;
        }
    }

    public sealed class DeciderTests
    {
        [TestCase(DecisionKind.RandomStandard)]
        [TestCase(DecisionKind.RandomCrypto)]
        [Category("2 - Demo - Randomization - Decider Tests")]
        // Determines that when the Decider.Decide() method is invoked
        // an exception is not thrown.
        public void WhenTheDecideMethodIsInvoked_ThenAnExceptionIsNotThrown(DecisionKind decisionKind)
        {
            Assert.DoesNotThrow(() => Decider.Decide(decisionKind));
        }
    }
}
