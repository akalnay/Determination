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
public bool IsItTeaTime1(DateTime startDateTime, DateTime endDateTime)
{
    DateTime now = DateTime.Now;
    return now >= startDateTime && now <= endDateTime;
}

// Testable version of a method to determine if the current date-time falls within a range.
public bool IsItTeaTime1(ICurrentDateTimeProvider currentDateTimeProvider, DateTime startDateTime, DateTime endDateTime)
{
    DateTime now = currentDateTimeProvider.Value;
    return now >= startDateTime && now <= endDateTime;
}

// This version of the method to determine if it is tea-time has a subtle bug:  the current 
// date time value is retrieved twice, once for each comparison.  It is possible that the
// first time the current date time is retrieved the value falls within the required range,
// whereas the second time it is retrieved it falls outside of the range.
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
// range.
// Only one date-time value is provided to the ICurrentDateTimeProvider parameter in the 
// IsItTeaTime2() method.  The IsItTeaTime2() method accesses the current date-time being provided
// more than once therefore throwing an InvalidOperationException.
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
