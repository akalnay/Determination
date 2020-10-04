using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Determination.Tests
{
    #region Tests for factory method Create with one argument:  Create(params DateTime[] values)

    public abstract class CurrentDateTimeProviderStub_Create1TestsBase
    {
        protected static class TestCases
        {
            public static IEnumerable<object[]> GetDateTimes()
            {
                DateTime dateTime1 = new DateTime(2020, 10, 1);
                DateTime dateTime2 = dateTime1.AddMonths(-1);       // dateTime2 is one month prior to dateTime1
                                                                    // By default, each date-time is expected to
                                                                    // be greater than the previous one.
                yield return new object[] { dateTime1, dateTime2 };
                yield return new object[] { dateTime1, dateTime1 }; // Both date-times are equal, they should be different
                                                                    // with the second one being greater than the first one.
            }
        }
    }

    public sealed class CurrentDateTimeProviderStub_Create1Tests : CurrentDateTimeProviderStub_Create1TestsBase
    {
        #region Happy Path Tests

        [Test]
        [Category("CurrentDateTimeProviderStub - Create(params DateTime[] values) Tests")]
        // Test that the Value property of a newly created CurrentDateTimeProviderStub instance
        // has the expected value.
        public void WhenTheValuePropertyIsRetrieved_ThenItHasTheValueOfTheArgumentProvidedToTheFactoryMethod()
        {
            DateTime dateTime = new DateTime(2020, 10, 1);
            Assert.AreEqual(dateTime, CurrentDateTimeProviderStub.Create(dateTime).Value);
        }

        #endregion Happy Path Tests

        #region Unhappy Path Tests

        [Test]
        [Category("CurrentDateTimeProviderStub - Create(params DateTime[] values) Tests")]
        // Test for empty params array.
        // Should throw an ArgumentException:
        // ArgumentException: values array is empty.
        public void WhenNoArgumentsAreProvidedToTheFactoryMethod_ThenAnArgumentExceptionIsThrown()
        {
            Assert.Throws<ArgumentException>(() => CurrentDateTimeProviderStub.Create());
        }

        [Test]
        [Category("CurrentDateTimeProviderStub - Create(params DateTime[] values) Tests")]
        // Test for null params array.
        // Should throw an ArgumentNullException.
        // ArgumentNullException: Value cannot be null.
        public void WhenTheArgumentIsNull_ThenAnArgumentNullExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(() => CurrentDateTimeProviderStub.Create((DateTime[])null));
        }

        [Test]
        [Category("CurrentDateTimeProviderStub - Create(params DateTime[] values) Tests")]
        // Test that the if the CurrentDateTimeProviderStub instance is seeded with only
        // one DateTime value and the instance's Value property is accessed more than once,
        // then an InvalidOperationException is thrown.
        // InvalidOperationException: Enumerator has passed the end of the enumerable.
        public void WhenAnInstanceHasTheValuePropertyAccessedMoreTimesThanTheNumberOfDateTimesProvided_ThenAnInvalidOperationExceptionIsThrown()
        {
            CurrentDateTimeProviderStub x = CurrentDateTimeProviderStub.Create(new DateTime(2020, 10, 1));
            _ = x.Value;
            Assert.Throws<InvalidOperationException>(() => _ = x.Value);
        }

        // Test that the if the CurrentDateTimeProviderStub instance is seeded with date-times
        // where the current date-time is less or equal than the previous one
        // then an InvalidOperationException is thrown:
        // InvalidOperationException: A new DateTime value must be greater than the previous one.
        [TestCaseSource(typeof(TestCases), nameof(TestCases.GetDateTimes))]
        [Category("CurrentDateTimeProviderStub - Create(params DateTime[] values) Tests")]
        public void WhenTheCurrentDateTimeIsNotGreaterThanThePreviousDateTime_ThanAnInvalidOperationExceptionIsThrown(DateTime dateTime1, DateTime dateTime2)
        {
            CurrentDateTimeProviderStub x = CurrentDateTimeProviderStub.Create(dateTime1, dateTime2);
            _ = x.Value;
            Assert.Throws<InvalidOperationException>(() => _ = x.Value);
        }

        #endregion Unhappy Path Tests
    }

    #endregion Tests for factory method:  Create(params DateTime[] values)
}
