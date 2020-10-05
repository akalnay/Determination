////////////////////////////////////////////////////////
// Copyright (c) Alejandro Kalnay                     //
// License: GNU GPLv3                                 //
////////////////////////////////////////////////////////

using NUnit.Framework;

namespace Determination.Tests
{
    // Sanity Check tests for the ValueProvider class
    public sealed class ValueProviderTests
    {
        [Test]
        [Category("ValueProvider Tests")]
        public void WhenTheValueProviderClassIsInstantiated_ThenAnExceptionIsNotThrown()
        {
            Assert.DoesNotThrow(() => _ = ValueProvider.Create(DirectionManager.GetNextRandomDirection));
        }

        [Test]
        [Category("ValueProvider Tests")]
        public void WhenTheValueProviderValuePropertyIsRetrieved_ThenAnExceptionIsNotThrown()
        {
            Assert.DoesNotThrow(() => _ = ValueProvider.Create(DirectionManager.GetNextRandomDirection).Value);
        }
    }
}
