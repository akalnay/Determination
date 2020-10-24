////////////////////////////////////////////////////////
// Copyright (c) Alejandro Kalnay                     //
// License: GNU GPLv3                                 //
////////////////////////////////////////////////////////

using NUnit.Framework;
using System;

namespace Determination.Demo
{
    internal static class IsItTeaTime
    {
        #region SUTs

        // A method to determine if the current date-time falls within a range.
        // The IsItTeaTime1() method stores the current date-time value being provided into a local variable and then
        // uses the stored value for its calculation.
        internal static bool IsItTeaTime1(ICurrentDateTimeProvider currentDateTimeProvider, DateTime startDateTime, DateTime endDateTime)
        {
            DateTime now = currentDateTimeProvider.Value;
            return now >= startDateTime && now <= endDateTime;
        }

        // This version of the method to determine if it is tea-time has a subtle bug:  the current date time value is retrieved twice,
        // once for each comparison.  It is possible that the first time the current date time is retrieved the value falls within the
        // required range, whereas the second time it is retrieved it falls outside of the range.
        // If the currentDateTimeProvider argument does not provide more than one DateTime then an InvalidOperationException
        // will be thrown.
        internal static bool IsItTeaTime2(ICurrentDateTimeProvider currentDateTimeProvider, DateTime startDateTime, DateTime endDateTime)
        {
            return currentDateTimeProvider.Value >= startDateTime && currentDateTimeProvider.Value <= endDateTime;
        }

        #endregion SUTs

        #region Test a method that determines if the current date-time falls within a provided range

        [Test]
        [Category("2 - Demo - CurrentDateTimeProvider - IsItTeaTime() Tests")]
        // Happy-path test for a method that determines if the current date-time falls within a range.
        // Only one date-time value is provided to the ICurrentDateTimeProvider parameter in the IsItTeaTime1() method.
        public static void WhenTheIsItTeaTimeMethodIsInvoked_ThenTheResultIsTheExpectedValue()
        {
            DateTime startDateTime = new DateTime(2020, 10, 1, 16, 0, 0);
            DateTime endDateTime   = new DateTime(2020, 10, 1, 18, 0, 0);
            Assert.IsTrue(IsItTeaTime1(CurrentDateTimeProviderStub.Create(endDateTime), startDateTime, endDateTime));
        }

        [Test]
        [Category("2 - Demo - CurrentDateTimeProvider - IsItTeaTime() Tests")]
        // Unhappy-path test for a method that determines if the current date-time falls within a range.
        // Only one date-time value is provided to the ICurrentDateTimeProvider parameter in the IsItTeaTime2() method.
        // The IsItTeaTime2() method accesses the current date-time being provided more than once therefore
        // throwing an InvalidOperationException.
        public static void WhenTheIsItTeaTimeMethodRetrievesTheCurrentDateTimeValueMoreThanOnce_ThenAnInvalidOperationExceptionIsThrown()
        {
            DateTime startDateTime = new DateTime(2020, 10, 1, 16, 0, 0);
            DateTime endDateTime   = new DateTime(2020, 10, 1, 18, 0, 0);
            Assert.Throws<InvalidOperationException>(() => IsItTeaTime2(CurrentDateTimeProviderStub.Create(endDateTime), startDateTime, endDateTime));
        }

        [Test]
        [Category("2 - Demo - CurrentDateTimeProvider - IsItTeaTime() Tests")]
        // Another unhappy-path test for a method that determines if the current date-time falls within a range.
        // Two date-time values are provided to the ICurrentDateTimeProvider parameter in the IsItTeaTime2() method.
        // While the first date-time provided to the ICurrentDateTimeProvider parameter falls within the required range,
        // the second value provided does not.  This causes the calculation to provide an erroneus result.
        public static void WhenTheIsItTeaTimeMethodRetrievesTheCurrentDateTimeValueMoreThanOnce_ThenAnInvalidOperationExceptionIsThrown2()
        {
            DateTime startDateTime = new DateTime(2020, 10, 1, 16, 0, 0);
            DateTime endDateTime   = new DateTime(2020, 10, 1, 18, 0, 0);
            Assert.IsFalse(IsItTeaTime2(CurrentDateTimeProviderStub.Create(startDateTime, endDateTime.AddTicks(1)), startDateTime, endDateTime));
        }

        #endregion Test a method that determines if the current date-time falls within a provided range
    }
}
