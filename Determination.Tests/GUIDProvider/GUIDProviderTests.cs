////////////////////////////////////////////////////////
// Copyright (c) Alejandro Kalnay                     //
// License: GNU GPLv3                                 //
////////////////////////////////////////////////////////

using NUnit.Framework;

namespace Determination.Tests
{
    // Sanity Check tests for the GUIDProvider class
    public sealed class GUIDProviderTests
    {
        [Test]
        [Category("GUIDProvider Tests")]
        public void WhenTheGuidProviderClassIsInstantiated_ThenAnExceptionIsNotThrown()
        {
            Assert.DoesNotThrow(() => _ = new GuidProvider());
        }

        [Test]
        [Category("GUIDProvider Tests")]
        public void WhenTheGuidProviderValuePropertyIsRetrieved_ThenAnExceptionIsNotThrown()
        {
            Assert.DoesNotThrow(() => _ = new GuidProvider().Value);
        }
    }
}
