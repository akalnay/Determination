using NUnit.Framework;

namespace Determination.Tests
{
    // Sanity Check Tests
    public sealed class Tests2
    {
        [Test]
        public void TestA()
        {
            Assert.DoesNotThrow(() => _ = new CurrentDateTimeProvider());
        }

        [Test]
        public void TestB()
        {
            Assert.DoesNotThrow(() => _ = new CurrentDateTimeProvider().Value);
        }
    }
}
