# Determination #
The objective of the `Determination` library is to provide determinism to unit tests.  [Wikipedia](https://en.wikipedia.org/wiki/Main_Page) defines a deterministic algorithm as an [*algorithm which, given a particular input, will always produce the same output, with the underlying machine always passing through the same sequence of states*](https://en.wikipedia.org/wiki/Deterministic_algorithm).

Good unit tests are repeatable, they always produce the same results from a fixed set of inputs.  Intuitively, this makes a lot of sense but can be challenging to achieve in real practice: for example programming logic that depends on a current date or time might give different results as the current date or time changes.  That might be desirable for a software program, however it is not so for testing it; tests need to be consistent and therefore can't depend on the current date or time.

Solving this challenge might seem easy at first:  just provide the real date-time to the actual software, and substitute the value with a pre-defined one when running unit tests.  That solution works for many cases, but not all:  sometimes a test might need the date-time being used to be more like the real thing, for example when testing the passage of time.

Other examples of values that are non-deterministic are [`Globally Unique Identifiers`](https://en.wikipedia.org/wiki/Universally_unique_identifier) and values that are generated through randomization.

## Examples

### GetTodaysDateAsText()
```C#
// Untestable version of a method that returns the current date as a text string formatted as
// "yyyy/MM/dd".  In this method the date is provided directly by the operating system making
// it unsuitable for unit tests.
public string GetTodaysDateAsText()
{
    return DateTime.Now.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
}

// Testable version of the method that returns the current date as a text string formatted as
// "yyyy/MM/dd".  In this method the current date is provided by an interface parameter.  An
// implementation of the interface could retrieve the date from the operating systemn whereas
// an alternate implementation could instead use a user-provided value.
public string GetTodaysDateAsText(ICurrentDateTimeProvider currentDateTimeProvider)
{
    return currentDateTimeProvider.Value.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
}

[Test]
// This test provides a consistent date-time value to the method being tested. The test will 
// always provide the same result regardless of when it runs.
public void WhenTheGetTodaysDateAsTextMethodIsInvoked_ThenTheResultIsTodaysDateFormatedAsAStringWithTheFormatYearMonthDay()
{
    // A real program might retrieve the date from the operating system by using:
    // string todaysDateAsText = GetTodaysDateAsText(new CurrentDateTimeProvider());
    Assert.AreEqual("2020/10/02", GetTodaysDateAsText(CurrentDateTimeProviderStub.Create(new DateTime(2020, 10, 2))));
}
```
### IsItTeaTime()
```C#
// Untestable version of a method to determine if the current date-time falls within a range.
// The current date-time is provided directly by the operating system making the method
// unsuitable for unit tests.
public bool IsItTeaTime1(DateTime startDateTime, DateTime endDateTime)
{
    DateTime now = DateTime.Now;
    return now >= startDateTime && now <= endDateTime;
}

// Testable version of a method to determine if the current date-time falls within a range.
// The current date is provided by an interface parameter.  An implementation of the interface
// could retrieve the date from the operating systemn whereas an alternate implementation could
// rely on a user-provided value.
public bool IsItTeaTime1(ICurrentDateTimeProvider currentDateTimeProvider, DateTime startDateTime, DateTime endDateTime)
{
    DateTime now = currentDateTimeProvider.Value;
    return now >= startDateTime && now <= endDateTime;
}

// This testable version of the method to determine if it is tea-time has a subtle bug:
// the current date time value is retrieved twice, once for each comparison.  It is possible
// that the first time the current date time is retrieved the value falls within the required
// range, and when it is retrieved a second time it falls outside of the range.
public bool IsItTeaTime2(ICurrentDateTimeProvider currentDateTimeProvider, DateTime startDateTime, DateTime endDateTime)
{
    return currentDateTimeProvider.Value >= startDateTime && currentDateTimeProvider.Value <= endDateTime;
}

[Test]
// Happy-path test for a method that determines if the current date-time falls within a range.
// Only one date-time value is provided to the ICurrentDateTimeProvider parameter in the 
// IsItTeaTime1() method.  The IsItTeaTime1() method stores the current date-time value being
// provided into a local variable and then uses the stored value for its calculation.
public void WhenTheIsItTeaTimeMethodIsInvoked_ThenTheResultIsTheExpectedValue()
{
    Assert.IsTrue(IsItTeaTime1(CurrentDateTimeProviderStub.Create(new DateTime(2020, 10, 1, 18, 0, 0)), 
                                new DateTime(2020, 10, 1, 16, 0, 0), 
                                new DateTime(2020, 10, 1, 18, 0, 0)));
}

[Test]
// Unhappy-path test for a method that determines if the current date-time falls within a
// range.  Only one date-time value is provided to the ICurrentDateTimeProvider parameter in
// the IsItTeaTime2() method.  The IsItTeaTime2() method accesses the current date-time being
// provided more than once therefore throwing an InvalidOperationException.
public void WhenTheIsItTeaTimeMethodRetrievesTheCurrentDateTimeValueMoreThanOnce_ThenAnInvalidOperationExceptionIsThrown()
{
    Assert.Throws<InvalidOperationException>(() => IsItTeaTime2(CurrentDateTimeProviderStub.Create(new DateTime(2020, 10, 1, 18, 0, 0)),
                                new DateTime(2020, 10, 1, 16, 0, 0),
                                new DateTime(2020, 10, 1, 18, 0, 0)));
}

[Test]
// Another unhappy-path test for a method that determines if the current date-time falls
// within a range.  Two date-time values are provided to the ICurrentDateTimeProvider
// parameter in the IsItTeaTime2() method.  While the first date-time provided to the
// ICurrentDateTimeProvider parameter falls within the required range, the second value
// provided does not.  This causes the calculation to provide an erroneus result.
public void WhenTheIsItTeaTimeMethodRetrievesTheCurrentDateTimeValueMoreThanOnce_ThenAnInvalidOperationExceptionIsThrown2()
{
    DateTime dateTime1 = new DateTime(2020, 10, 1, 18, 0, 0);
    DateTime dateTime2 = dateTime1.AddTicks(1);
    Assert.IsFalse(IsItTeaTime2(CurrentDateTimeProviderStub.Create(dateTime1, dateTime2),
                                new DateTime(2020, 10, 1, 16, 0, 0),
                                new DateTime(2020, 10, 1, 18, 0, 0)));
}
```
### CountdownTimer
The CountdownTimer class gives functionality somewhat similar to that in a
microwave oven's timer.  The functionality differs in that the timer in a microwave
oven will countdown for a specified time (e.g. for one minute) whereas
the CountdownTimer will count down until it a specified date-time is reached.
```C#
#region CountdownTimer Class

/// <summary>
/// Conveys data regarding an iteration of a <see cref="CountdownTimer"/> loop.
/// </summary>
public class CountdownTimerLoopElapsedEventArgs : EventArgs
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
public sealed class CountdownTimer
{
    /// <summary>
    /// Occurs when an iteration of the countdown timer's loop elapses.
    /// </summary>
    public event EventHandler<CountdownTimerLoopElapsedEventArgs> LoopElapsed;

    private void OnLoopElapsed(TimeSpan remainingTime) => LoopElapsed?.Invoke(this, new CountdownTimerLoopElapsedEventArgs(remainingTime));

    private async Task DoTimerLoopsAsync(DateTime stopDateTime, ICurrentDateTimeProvider currentDateTimeProvider, int interval)
    {
        TimeSpan GetRemainingTime(DateTime stopDtTm, DateTime now)
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
    public async Task StartAsync(DateTime stopDateTime, int interval, ICurrentDateTimeProvider currentDateTimeProvider)
    {
        await DoTimerLoopsAsync(stopDateTime, currentDateTimeProvider, interval).ConfigureAwait(false);
    }
}

#endregion CountdownTimer Class

#region Tests for the CountdownTimer class

[Test]
[Category("CountdownTimer Tests")]
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
[Category("CountdownTimer Tests")]
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

```