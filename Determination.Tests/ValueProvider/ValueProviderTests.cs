////////////////////////////////////////////////////////
// Copyright (c) Alejandro Kalnay                     //
// License: GNU GPLv3                                 //
////////////////////////////////////////////////////////

using NUnit.Framework;
using System;
using System.Linq;

namespace Determination.Tests
{
    // Sanity Check tests for the ValueProvider class
    public sealed class ValueProviderTests
    {
        private enum Direction { Up = 1, Right, Down, Left }

        private static readonly Random _RANDOM = new Random();

        [Test]
        [Category("ValueProvider Tests")]
        public void WhenTheValueProviderClassIsInstantiated_ThenAnExceptionIsNotThrown()
        {
            Assert.DoesNotThrow(() => _ = ValueProvider.Create(GetNextRandomDirection));
        }

        [Test]
        [Category("ValueProvider Tests")]
        public void WhenTheValueProviderValuePropertyIsRetrieved_ThenAnExceptionIsNotThrown()
        {
            Assert.DoesNotThrow(() => _ = ValueProvider.Create(GetNextRandomDirection).Value);
        }

        private static Direction GetNextRandomDirection() => (Direction)_RANDOM.Next((int)Min<Direction>(), Count<Direction>() + 1);

        private static T Min<T>() where T : struct, Enum => Enum.GetValues(typeof(T)).Cast<T>().Min();

        private static int Count<T>() where T : struct, Enum => Enum.GetNames(typeof(T)).Length;
    }
}
