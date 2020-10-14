////////////////////////////////////////////////////////
// Copyright (c) Alejandro Kalnay                     //
// License: GNU GPLv3                                 //
////////////////////////////////////////////////////////

using NUnit.Framework;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Determination.Demo
{
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

    public sealed class CountdownTimer_Tests
    {

        [Test]
        [Category("2 - Demo - CurrentDateTimeProvider - CountdownTimer Tests")]
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
        [Category("2 - Demo - CurrentDateTimeProvider - CountdownTimer Tests")]
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

    }
    #endregion Tests for the CountdownTimer class

    #endregion Test a Countdown Timer
}
