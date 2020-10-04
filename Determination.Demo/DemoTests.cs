using NUnit.Framework;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;

namespace Determination.Demo
{
    public class DemoTests
    {
        #region Test a method that depends on the current date

        // SUT
        internal string GetTodaysDateAsText(ICurrentDateTimeProvider currentDateTimeProvider)
        {
            return currentDateTimeProvider.Value.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
        }

        [Test]
        [Category("Demo - CurrentDateTimeProvider - GetTodaysDateAsText Tests")]
        // This test provides a consistent date-time value to the method being tested. The test will
        // always provide the same result regardless of when it runs.
        public void WhenTheGetTodaysDateAsTextMethodIsInvoked_ThenTheResultIsTodaysDateFormatedAsAStringWithTheFormatYearMonthDay()
        {
            // A real program might retrieve the date from the operating system by using:
            // string todaysDateAsText = GetTodaysDateAsText(new CurrentDateTimeProvider());
            Assert.AreEqual("2020/10/02", GetTodaysDateAsText(CurrentDateTimeProviderStub.Create(new DateTime(2020, 10, 2))));
        }

        #endregion Test a method that depends on the current date

        #region Test a method that determines if the current date-time falls within a provided range

        #region SUTs

        // A method to determine if the current date-time falls within a range.
        // The IsItTeaTime1() method stores the current date-time value being provided into a local variable and then
        // uses the stored value for its calculation.
        internal bool IsItTeaTime1(ICurrentDateTimeProvider currentDateTimeProvider, DateTime startDateTime, DateTime endDateTime)
        {
            DateTime now = currentDateTimeProvider.Value;
            return now >= startDateTime && now <= endDateTime;
        }

        // This version of the method to determine if it is tea-time has a subtle bug:  the current date time value is retrieved twice,
        // once for each comparison.  It is possible that the first time the current date time is retrieved the value falls within the
        // required range, whereas the second time it is retrieved it falls outside of the range.
        // If the currentDateTimeProvider argument does not provide more than one DateTime then an InvalidOperationException
        // will be thrown.
        internal bool IsItTeaTime2(ICurrentDateTimeProvider currentDateTimeProvider, DateTime startDateTime, DateTime endDateTime)
        {
            return currentDateTimeProvider.Value >= startDateTime && currentDateTimeProvider.Value <= endDateTime;
        }

        #endregion SUTs

        [Test]
        [Category("Demo - CurrentDateTimeProvider - IsItTeaTime() Tests")]
        // Happy-path test for a method that determines if the current date-time falls within a range.
        // Only one date-time value is provided to the ICurrentDateTimeProvider parameter in the IsItTeaTime1() method.
        public void WhenTheIsItTeaTimeMethodIsInvoked_ThenTheResultIsTheExpectedValue()
        {
            Assert.IsTrue(IsItTeaTime1(CurrentDateTimeProviderStub.Create(new DateTime(2020, 10, 1, 18, 0, 0)),
                                                                            new DateTime(2020, 10, 1, 16, 0, 0),
                                                                            new DateTime(2020, 10, 1, 18, 0, 0)));
        }

        [Test]
        [Category("Demo - CurrentDateTimeProvider - IsItTeaTime() Tests")]
        // Unhappy-path test for a method that determines if the current date-time falls within a range.
        // Only one date-time value is provided to the ICurrentDateTimeProvider parameter in the IsItTeaTime2() method.
        // The IsItTeaTime2() method accesses the current date-time being provided more than once therefore
        // throwing an InvalidOperationException.
        public void WhenTheIsItTeaTimeMethodRetrievesTheCurrentDateTimeValueMoreThanOnce_ThenAnInvalidOperationExceptionIsThrown()
        {
            Assert.Throws<InvalidOperationException>(() => IsItTeaTime2(CurrentDateTimeProviderStub.Create(new DateTime(2020, 10, 1, 18, 0, 0)),
                                                                            new DateTime(2020, 10, 1, 16, 0, 0),
                                                                            new DateTime(2020, 10, 1, 18, 0, 0)));
        }

        [Test]
        [Category("Demo - CurrentDateTimeProvider - IsItTeaTime() Tests")]
        // Another unhappy-path test for a method that determines if the current date-time falls within a range.
        // Two date-time values are provided to the ICurrentDateTimeProvider parameter in the IsItTeaTime2() method.
        // While the first date-time provided to the ICurrentDateTimeProvider parameter falls within the required range,
        // the second value provided does not.  This causes the calculation to provide an erroneus result.
        public void WhenTheIsItTeaTimeMethodRetrievesTheCurrentDateTimeValueMoreThanOnce_ThenAnInvalidOperationExceptionIsThrown2()
        {
            DateTime dateTime1 = new DateTime(2020, 10, 1, 18, 0, 0);
            DateTime dateTime2 = dateTime1.AddTicks(1);
            Assert.IsFalse(IsItTeaTime2(CurrentDateTimeProviderStub.Create(dateTime1, dateTime2),
                                                                            new DateTime(2020, 10, 1, 16, 0, 0),
                                                                            new DateTime(2020, 10, 1, 18, 0, 0)));
        }

        #endregion Test a method that determines if the current date-time falls within a provided range

        #region Test a Countdown Timer

        #region CountdownTimer Class

        /// <summary>
        /// Conveys data regarding an iteration of a <see cref="CountdownTimer"/> loop.
        /// </summary>
        internal class CountdownTimerLoopElapsedEventArgs : EventArgs
        {
            public CountdownTimerLoopElapsedEventArgs(TimeSpan remainingTime)
            {
                RemainingTime = remainingTime;
            }

            public TimeSpan RemainingTime { get; }
        }

        /// <summary>
        /// Represents a countdown timer.
        /// </summary>
        /// <remarks>
        /// The CountdownTimer class gives functionality somewhat similar to that in a
        /// microwave oven's timer.  Where the functionality differs in that the timer in
        /// a microwave oven will countdown for a specified time (e.g. for one minute) whereas
        /// the CountdownTimer will count down until a specified <see cref="DateTime"/>.
        /// </remarks>
        internal sealed class CountdownTimer
        {
            /// <summary>
            /// Occurs when an iteration of the countdown timer's loop elapses.
            /// </summary>
            public event EventHandler<CountdownTimerLoopElapsedEventArgs> LoopElapsed;

            private void OnLoopElapsed(TimeSpan remainingTime) => LoopElapsed?.Invoke(this, new CountdownTimerLoopElapsedEventArgs(remainingTime));

            private async Task DoTimerLoopsAsync(DateTime stopDateTime, ICurrentDateTimeProvider currentDateTimeProvider, int interval)
            {
                static TimeSpan GetRemainingTime(DateTime stopDtTm, DateTime now)
                {
                    TimeSpan result = stopDtTm.Subtract(now);
                    if (result < TimeSpan.Zero)
                        result = TimeSpan.Zero;
                    return result;
                }

                TimeSpan remainingTime;
                while ((remainingTime = GetRemainingTime(stopDateTime, currentDateTimeProvider.Value)) > TimeSpan.Zero)
                {
                    OnLoopElapsed(remainingTime);
                    await Task.Delay(interval).ConfigureAwait(false);
                }
            }

            /// <summary>
            /// Asynchronously starts the countdown timer
            /// </summary>
            /// <param name="stopDateTime">
            /// Determines when the Countdown Timer should stop.
            /// </param>
            /// <param name="interval">
            /// Determines the interval in milliseconds to use for asynchronously suspending execution of the timer.
            /// The timer is suspended in a non-blocking manner.
            /// </param>
            /// <param name="currentDateTimeProvider">
            /// Represents an instance of a class that when queried will provide a value for the current date-time.
            /// </param>
            /// <returns>
            /// The task object representing the asynchronous operation.
            /// </returns>
            internal async Task StartAsync(DateTime stopDateTime, int interval, ICurrentDateTimeProvider currentDateTimeProvider)
            {
                await DoTimerLoopsAsync(stopDateTime, currentDateTimeProvider, interval).ConfigureAwait(false);
            }
        }

        #endregion CountdownTimer Class

        #region Tests for the CountdownTimer class

        [Test]
        [Category("Demo - CurrentDateTimeProvider - CountdownTimer Tests")]
        // This is a test for a CountdownTimer.  For the test to pass, the CountdownTimer must
        // stop at exactly 1 PM on 10/01/2020.
        // An instance of the CurrentDateTimeProviderStub class is in charge of providing the
        // current date-time to the CountdownTimer.
        // The test is set to simulate that it is being started at 12 noon on 10/01/2020.
        // The CurrentDateTimeProviderStub instance provides the current date-time in
        // increments of 10 minutes.
        // At the first occurrence of querying the CurrentDateTimeProviderStub instance for the
        // current date-time, the date-time returned will be 12 noon on 10/01/2020.
        // A subsequent query for the current date-time will return 12:10 PM on 10/01/2020.
        // A query after that will return 12:20 PM on 10/01/2020, and so on.
        public async Task WhenTheCountdownTimerStops_ThenTheCurrentDateTimeIsTheDesignatedStopTime()
        {
            const double minutesIncrement = 10;
            CountdownTimer countdownTimer = new CountdownTimer();
            DateTime stopDateTime = new DateTime(2020, 10, 1, 13, 0, 0);            // 10/01/2020 at 1 PM
            CurrentDateTimeProviderStub currentDateTimeProvider =
                CurrentDateTimeProviderStub.Create(stopDateTime.AddHours(-1),       // The first date-time
                                                                                    // that will be returned
                                                                                    // by the currentDateTimeProvider
                                                                                    // will be exactly one hour prior
                                                                                    // to the designated stopDateTime.

                                                    minutesIncrement,               // After the first date-time
                                                                                    // has been retrieved, whenever the
                                                                                    // currentDateTimeProvider
                                                                                    // is queried for the
                                                                                    // current DateTime, the
                                                                                    // value returned will be
                                                                                    // an increment of the previous
                                                                                    // value returned.
                                                    (dateTime, minutes) => dateTime.AddMinutes(minutes));
            await countdownTimer.StartAsync(stopDateTime, 0, currentDateTimeProvider).ConfigureAwait(false);
            Assert.AreEqual(stopDateTime, currentDateTimeProvider.CurrentValue);
        }

        [Test]
        [Category("Demo - CurrentDateTimeProvider - CountdownTimer Tests")]
        // Another test for a CountdownTimer.  
        // This test determines if the CountdownTimer's LoopElapsed event has the expected
        // values for the RemainingTime property of the event's handler.
        // For the test to pass, the RemaningTime property must have held the values
        // { 60 minutes, 50 minutes, 40 minutes, 30 minutes, 20 minutes, 10 minutes }.
        // The CountdownTimer should stop at exactly 1 PM on 10/01/2020.
        // The test is set to simulate that it is being started at 12 noon on 10/01/2020.
        public async Task WhenTheCountdownTimerEventLoopElapsedIsRaised_ThenTheRemainingTimeHasTheExpectedValue()
        {
            const double minutesIncrement = 10;
            TimeSpan[] expectedRemainingTimes =
            {
                TimeSpan.FromMinutes(60), TimeSpan.FromMinutes(50), TimeSpan.FromMinutes(40),
                TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(20), TimeSpan.FromMinutes(10)
            };
            Collection<TimeSpan> actualRemainingTimes = new Collection<TimeSpan>();
            CountdownTimer countdownTimer = new CountdownTimer();
            countdownTimer.LoopElapsed += (o, e) => actualRemainingTimes.Add(e.RemainingTime);
            DateTime stopDateTime = new DateTime(2020, 10, 1, 13, 0, 0);            // 10/01/2020 at 1 PM
            CurrentDateTimeProviderStub currentDateTimeProvider =
                CurrentDateTimeProviderStub.Create(stopDateTime.AddHours(-1),       // The first date-time
                                                                                    // that will be returned
                                                                                    // by the currentDateTimeProvider
                                                                                    // will be exactly one hour prior
                                                                                    // to the designated stopDateTime.

                                                    minutesIncrement,               // After the first date-time
                                                                                    // has been retrieved, whenever the
                                                                                    // currentDateTimeProvider
                                                                                    // is queried for the
                                                                                    // current DateTime, the
                                                                                    // value returned will be
                                                                                    // an increment of the previous
                                                                                    // value returned.
                                                    (dateTime, minutes) => dateTime.AddMinutes(minutes));
            await countdownTimer.StartAsync(stopDateTime, 0, currentDateTimeProvider).ConfigureAwait(false);
            CollectionAssert.AreEqual(expectedRemainingTimes, actualRemainingTimes);
        }

        #endregion Tests for the CountdownTimer class

        #endregion Test a Countdown Timer
    }
}
