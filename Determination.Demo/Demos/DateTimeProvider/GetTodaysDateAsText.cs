////////////////////////////////////////////////////////
// Copyright (c) Alejandro Kalnay                     //
// License: GNU GPLv3                                 //
////////////////////////////////////////////////////////

using NUnit.Framework;
using System;
using System.Globalization;

namespace Determination.Demo
{
    internal static class GetTodaysDateAsText
    {
        #region Test a method that depends on the current date

        // SUT
        internal static string GetTodaysDate(ICurrentDateTimeProvider currentDateTimeProvider)
        {
            return currentDateTimeProvider.Value.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
        }

        [Test]
        [Category("2 - Demo - CurrentDateTimeProvider - GetTodaysDateAsText Tests")]
        // This test provides a consistent date-time value to the method being tested. The test will
        // always provide the same result regardless of when it runs.
        public static void WhenTheGetTodaysDateAsTextMethodIsInvoked_ThenTheResultIsTodaysDateFormatedAsAStringWithTheFormatYearMonthDay()
        {
            // A real program might retrieve the date from the operating system by using:
            // string todaysDateAsText = GetTodaysDateAsText(new CurrentDateTimeProvider());
            Assert.AreEqual("2020/10/02", GetTodaysDate(CurrentDateTimeProviderStub.Create(new DateTime(2020, 10, 2))));
        }

        #endregion Test a method that depends on the current date
    }
}
