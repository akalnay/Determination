////////////////////////////////////////////////////////
// Copyright (c) Alejandro Kalnay                     //
// License: GNU GPLv3                                 //
////////////////////////////////////////////////////////

using NUnit.Framework;

namespace Determination.Tests
{
    // Sanity Check tests for the CurrentDateTimeProvider class
    public sealed class CurrentDateTimeProviderTests
    {
        [Test]
        [Category("1 - CurrentDateTimeProvider Tests")]
        public void WhenTheCurrentDateTimeProviderClassIsInstantiated_ThenAnExceptionIsNotThrown()
        {
            Assert.DoesNotThrow(() => _ = new CurrentDateTimeProvider());
        }

        [Test]
        [Category("1 - CurrentDateTimeProvider Tests")]
        public void WhenTheCurrentDateTimeProviderValuePropertyIsRetrieved_ThenAnExceptionIsNotThrown()
        {
            Assert.DoesNotThrow(() => _ = new CurrentDateTimeProvider().Value);
        }
    }
}
