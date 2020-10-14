# Determination #

## Introduction
The objective of the `Determination` API is to provide determinism to unit tests.  [Wikipedia](https://en.wikipedia.org/wiki/Main_Page) defines a deterministic algorithm as [*an algorithm which, given a particular input, will always produce the same output, with the underlying machine always passing through the same sequence of states*](https://en.wikipedia.org/wiki/Deterministic_algorithm).

Good unit tests are repeatable, they always produce the same results from a fixed set of inputs.  Intuitively, this makes a lot of sense but it can be challenging to achieve in real practice: for example a program may use the current date-time somewhere in its logic, or use a [Globally Unique Identifier](https://en.wikipedia.org/wiki/Universally_unique_identifier) to provide uniqueness to an object, or perhaps use randomization.  All of those are examples of values that will be different everytime a program runs, however testing any logic that depends on those values will require them to be consistent for each test iteration.

In addition to being repeatable, unit tests are also expected to run quickly.  A program may willfully introduce a delay in the execution of its logic; a delay that might be desirable in a production environment will have a detrimental impact on how swiftly tests execute.

Aside from improved testability, the `Determination` API brings in one additional benefit:  it allows flexibility in choosing how a given algorithm is implemented.  For example `.NET` offers a couple of mechanisms for randomization:  there is the [System.Random](https://docs.microsoft.com/en-us/dotnet/api/system.random?view=netcore-3.1) class and also the [System.Security.Cryptography.RNGCryptoServiceProvider](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.rngcryptoserviceprovider?view=netcore-3.1) class.  The `Determination` API allows a software developer to change on the fly the randomization algorithm used by production software and to also provide deterministic values for unit testing.

## API Overview
The `Determination` API strives to combine ease of use with versatility.  All the classes in the API implement the interface `IDynamicValueProvider<T>`.  This interface has the single property `Value` which is how a user of the API obtains the current value of what the class being used is meant to represent.  The value returned by the `Value` property is determined by how the class instance was constructed, everytime the `Value` property is accessed the value returned will be different as long as different values where provided during construction.  Depending on the particular use case, when constructing an instance of one of the provided classes it is possible to setup the `Value` property to have either one value, multiple (but finite) values, or an infinite number of values.

The functionality of the API can be divided into three main groups:

1. Dates and Times:  
Two classes are provided for working with dates and times:  `DateTimeProvider` and `DateTimeProviderStub`.  `DateTimeProvider` is meant for production software whereas `DateTimeProviderStub` is for unit testing purposes.  Both of these classes implement the interface `ICurrentDateTimeProvider`.
2. Guid's:  
Two classes are provided for working with Guid's:  `GuidProvider` and `GuidProviderStub`.  `GuidProvider` is meant for production software whereas `GuidProviderStub` is for unit testing purposes.  Both of these classes implement the interface `IGuidProvider`.
3. User defined values:  
Two classes are provided for working with user defined values:  `ValueProvider` and `ValueProviderStub`.  `ValueProvider` is meant for production software whereas `ValueProviderStub` is for unit testing purposes.  Both of these classes implement the interface `IValueProvider`.

In addition, it is also possible to extend the API by creating other descendents of the interface `IDynamicValueProvider<T>` and creating implementations of the new interface in a similar manner to how the `DateTimeProvider`/`DateTimeProviderStub` and `GuidProvider`/`GuidProviderStub` classes do it.

Looking at the examples below should help make things clearer.

## Examples
1. [Dates and Times](#dates-and-times)
2. [Guid](#guid)
3. [Randomization](#randomization)

### Dates and Times
1. [Get Today's Date As Text](#get-todays-date-as-text)
2. [Determine if a date falls within a range](#determine-if-a-date-falls-within-a-range)
3. [Countdown Timer](#countdown-timer)

#### Get Today's Date As Text
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
#### Determine if a date falls within a range
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
#### Countdown Timer
The CountdownTimer class gives functionality somewhat similar to that of a
microwave oven's timer.  The functionality differs in that the timer in a microwave
oven will countdown for a specified time (e.g. for one minute) whereas
the CountdownTimer will count down until a specified date-time is reached.
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
// Test that the CountdownTimer stops at the designated DateTime.
// This test requires that a different value is returned whenever a request for the current
// date-time is made.  This is with the intent of simulating the passage of time.
// In the test, time is incremented by 10 minute intervals; if the starting time is at noon,
// then the next time that the current date-time is requested it will be 10 minutes after
// that and so on.
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
// Test that the when the CountdownTimer's LoopElapsed event is raised then
// the event's RemainingTime property has the expected value.
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
### Guid
This example shows how to create software that uses Guid's to provide
a unique identifier to an object.  The ```Determination``` API
can provide a deterministic or non-deterministic Guid depending on the
need.
```C#
#region Person

internal class Person
{
    // This constructor will assign to the Guid property
    // a Guid generated by the system.
    // Person instances that were created with this constructor
    // can't test anything that depends on a specific Guid.
    public Person(string firstName, string lastName) : this(firstName, lastName, new GuidProvider())
    {
    }

    // This constructor will assign to the Guid property
    // a Guid provided by the developer.
    // Person instances that were created with this constructor
    // can test anything that depends on a specific Guid.
    public Person(string firstName, string lastName, IGuidProvider guidProvider)
    {
        FirstName = firstName;
        LastName  = lastName;
        Guid      = guidProvider.Value;
    }

    public string FirstName { get; }

    public string LastName { get; }

    public Guid Guid { get; }
}

#endregion Person

[Test]
[Category("2 - Demo - GuidProvider - Person Tests")]
// This test assigns a predefined Guid while creating an instance
// of the Person class.
// The test verifies that the value of the Person's Guid property is
// indeed the Guid assigned when the Person instance was created.
public void WhenTheGuidPropertyOfAPersonInstanceIsRetrieved_ThenItsValueIsTheGuidAssignedDuringTheCreationOfTheInstance()
{
    // A real program might allow the framework to assign the Guid by doing:
    // Person person = new Person("Jim", "Smith");
    Guid guid     = new Guid("7A0908FB-D9F9-4A6D-8F95-EFE4DE3D5027");
    Person person = new Person("Jim", "Smith", GuidProviderStub.Create(guid));
    Assert.AreEqual(guid, person.Guid);     // Validate that the person's Guid
                                            // is the pre-defined Guid value.
}
```
### Randomization
1. [Card Game](#card-game)
2. [Decider](#decider)

#### Card Game
This example shows how to use the `Determination` API to test software that uses randomization.  The CardGame class in the example has very simple functionality:  it allows a user to randomly retrieve a card from a set of cards.

There is one rule that must be verified before a card is retrieved:  
1. That there are still cards available to retrieve.  

And there are two rules that must be verified after a card is retrieved:
1. That after the card is retrieved that same card can't be retrieved again.
2. That after all possible cards have been retrieved the class property `RemainingCards` is empty.
```C#
#region Card struct and CardGame class

internal enum Suit { Clubs, Diamonds, Hearts, Spades }

internal enum Rank { Ace, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King }

internal struct Card
{
    public Card(Rank rank, Suit suit)
    {
        Rank = rank;
        Suit = suit;
    }

    public Rank Rank { get; }

    public Suit Suit { get; }

    public override string ToString() => $"{Rank} - {Suit}";
}

/// <summary>
/// Represents a simple card game.  The only functionality that this
/// game provides is the ability to randomly select a card from
/// a set of cards.
/// </summary>
/// <remarks>
/// Cards are selected via one of the two GetCard() methods.  The
/// functionality offered by the <see cref="GetCard1"/> and
/// <see cref="GetCard2"/> methods is identical, where they
/// differ is that the method <see cref="GetCard1"/> can't
/// be tested via unit tests, whereas  
/// <see cref="GetCard2(IValueProvider&lt;Card>)"/> can
/// be tested.
/// </remarks>
internal class CardGame
{
    private static readonly Random _RANDOM = new Random();

    private readonly IValueProvider<Card> _valueProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="CardGame"></see> class.
    /// </summary>
    /// <param name="valueProvider">
    /// Provides deterministic <see cref="Card"/> instances.
    /// </param>
    public CardGame(IValueProvider<Card> valueProvider)
    {
        _valueProvider = valueProvider;
        RemainingCards = new HashSet<Card>(GetAllCards());
    }

    /// <summary>
    /// A set with the remaining cards.
    /// </summary>
    /// <remarks>
    /// Every time a card is returned by one of the GetCard() methods,
    /// the card is removed from the <see cref="RemainingCards"/> set.
    /// </remarks>
    public HashSet<Card> RemainingCards { get; }

    /// <summary>
    /// Returns one card from the cards remaining in a set of cards.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Rules that should be verified for this method:
    /// </para>
    /// <para>
    /// 1) That if the method is invoked when the <see cref="RemainingCards"/>
    /// property is empty then an <see cref="InvalidOperationException"/> is thrown.
    /// </para>
    /// <para>
    /// 2) That the property <see cref="RemainingCards"/> no longer contains the card returned.
    /// </para>
    /// <para>
    /// 3) That after all possible cards have been returned the <see cref="RemainingCards"/>
    /// property is empty.
    /// </para>
    /// <para>
    /// This method is not testable because:
    /// </para>
    /// <para>
    /// a) It is impossible to set the <see cref="RemainingCards"/> property to an
    /// empty sequence as there isn't a way to decrease the number of remaining cards
    /// other than by invoking one of the GetCard() methods.  This makes it impossible
    /// to test that an <see cref="InvalidOperationException"/> will be thrown if the
    /// <see cref="GetCard1"/> method is invoked when there are no cards remaining.
    /// </para>
    /// <para>
    /// b) To fully test that the after the <see cref="GetCard1"/> method is invoked then
    /// the <see cref="RemainingCards"/> property no longer contains the
    /// card returned it is necessary to test against every possible card.
    /// </para>
    /// <para>
    /// c) It is impossible to get the method to return all possible cards in a
    /// deterministic way.
    /// The card returned is randomly selected, and while it is likely that eventually
    /// all cards will be returned one can't be certain of this.  Also waiting
    /// for all possible random card selections may considerably increase the amount of time
    /// that it takes to run tests.
    /// </para>
    /// </remarks>
    /// <returns>A randomly selected card.</returns>
    public Card GetCard1()
    {
        if (!RemainingCards.Any())
            throw new InvalidOperationException("There are no cards remaining to dispurse.");
        Card card              = default;
        bool continueSelecting = true;
        while (continueSelecting)               // Continue looping until the selected card is
        {                                       // one that hasn't been selected before.
            card = SelectRandomCard();          // Select a random card.
            if (RemainingCards.Contains(card))
            {
                RemainingCards.Remove(card);
                continueSelecting = false;
            }
        }
        return card;
    }

    /// <summary>
    /// Returns one card from the cards remaining in a set of cards.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Rules that should be verified for this method:
    /// </para>
    /// <para>
    /// 1) That if the method is invoked when the <see cref="RemainingCards"/>
    /// property is empty then an <see cref="InvalidOperationException"/> is thrown.
    /// </para>
    /// <para>
    /// 2) That the property <see cref="RemainingCards"/> no longer contains the card returned.
    /// </para>
    /// <para>
    /// 3) That after all possible cards have been returned the <see cref="RemainingCards"/>
    /// property is empty.
    /// </para>
    /// <para>
    /// This method is testable because:
    /// </para>
    /// <para>
    /// a) It is possible to set the <see cref="RemainingCards"/> property to an
    /// empty sequence by using the method to retrieve all possible cards.  All
    /// possible cards can be retrieved via the <paramref name="valueProvider"/>
    /// argument in the <see cref="CardGame"/> class constructor.
    /// </para>
    /// <para>
    /// b) It is possible to fully test that the <see cref="RemainingCards"/> 
    /// property no longer contains the card returned by testing against every possible card.
    /// </para>
    /// <para>
    /// c) It is possible to get the method to return all possible cards in a
    /// deterministic way.
    /// </para>
    /// </remarks>
    /// <returns>A randomly selected card.</returns>
    public Card GetCard2()
    {
        if (!RemainingCards.Any())
            throw new InvalidOperationException("There are no cards remaining to dispurse.");
        Card card              = default;
        bool continueSelecting = true;
        while (continueSelecting)               // Continue looping until the selected card is
        {                                       // one that hasn't been selected before.
            card = _valueProvider.Value;        // Select a card.  The card may or may not be
                                                // randomly selected depending on how the 
                                                // IValueProvider<Card> interface has been
                                                // implemented.
            if (RemainingCards.Contains(card))
            {
                RemainingCards.Remove(card);
                continueSelecting = false;
            }
        }
        return card;
    }

    private IEnumerable<Card> GetAllCards()
    {
        IEnumerable<Card> allCards = Enum.GetValues(typeof(Rank))
                                            .Cast<Rank>()
                                            .SelectMany(rank => Enum.GetValues(typeof(Suit))
                                                                    .Cast<Suit>()
                                                                    .Select(suit => new Card(rank, suit)));
        return allCards;
    }

    private Card SelectRandomCard()
    {
        Rank randomRank = (Rank)_RANDOM.Next((int)Min<Rank>(), Count<Rank>() + 1);
        Suit randomSuit = (Suit)_RANDOM.Next((int)Min<Suit>(), Count<Suit>() + 1);
        Card card       = new Card(randomRank, randomSuit);
        return card;
    }

    private static T Min<T>() where T : struct, Enum => Enum.GetValues(typeof(T)).Cast<T>().Min();

    private static int Count<T>() where T : struct, Enum => Enum.GetNames(typeof(T)).Length;
}

#endregion Card struct and CardGame class

#region CardGame Tests

private static Card[] GetAllCards()
{
    return Enum.GetValues(typeof(Rank))
                .Cast<Rank>()
                .SelectMany(rank => Enum.GetValues(typeof(Suit))
                                        .Cast<Suit>()
                                        .Select(suit => new Card(rank, suit)))
                .ToArray();
}

[Test]
[Category("2 - Demo - Randomization - CardGame Tests")]
// If the GetCard2() method is invoked when there are no cards remaining then an
// InvalidOperationException should be thrown.
// InvalidOperationException: There are no cards remaining to dispurse.
public void WhenCardsRemainingPropertyIsEmptyAndTheGetCard2MethodIsInvoked_ThenAnInvalidOperationExceptionIsThrown()
{
    Card[] allCards   = GetAllCards();
    CardGame cardGame = new CardGame(ValueProviderStub.Create<Card>(allCards));
    for (int i = 0; i < allCards.Length; i++)                                   // After the last iteration of this
        _ = cardGame.GetCard2();                                                // loop there will be no cards remaining.

    Assert.Throws<InvalidOperationException>(() => cardGame.GetCard2());        // Attempting to get a card when there
                                                                                // are no cards remaining should throw
                                                                                // an exception.
}

[Test]
[Category("2 - Demo - Randomization - CardGame Tests")]
// After a card is retrieved by the GetCard2() method the RemainingCards
// property must no longer contain the card retrieved.
public void WhenACardIsRetrievedByInvokingTheGetCard2Method_ThenTheCardIsNoLongerContainedInTheRemainingCardsSet()
{
    Card[] allCards   = GetAllCards();
    CardGame cardGame = new CardGame(ValueProviderStub.Create<Card>(allCards));
    for (int i = 0; i < allCards.Length; i++)
    {
        Card card = cardGame.GetCard2();                                        // Retrieve one card.

        CollectionAssert.DoesNotContain(cardGame.RemainingCards, card);         // Verify that the card retrieved is no
                                                                                // longer contained in the RemainingCards
                                                                                // set.
    }
}

[Test]
[Category("2 - Demo - Randomization - CardGame Tests")]
// After all cards have been retrieved by the GetCard2() method the RemainingCards
// property must be empty.
public void WhenAllCardsHaveBeenRetrievedByInvokingTheGetCard2Method_ThenTheRemainingCardsPropertyIsEmpty()
{
    Card[] allCards = GetAllCards();
    CardGame cardGame = new CardGame(ValueProviderStub.Create<Card>(allCards));
    for (int i = 0; i < allCards.Length; i++)                                   // After the last iteration of this
        _ = cardGame.GetCard2();                                                // loop there will be no cards remaining.

    CollectionAssert.IsEmpty(cardGame.RemainingCards);                          // RemainingCards property must be
                                                                                // empty at this point.
}

#endregion CardGame Tests                                                      
```
#### Decider

This example demonstrates using the `Determination` API to determine which implementation of a random number generator should be used.
```C#
public enum DecisionKind { RandomStandard, RandomCrypto }

/// <summary>
/// The <see cref="Decider"/> class demonstrates using the Determination API to select
/// at runtime which particular implementation of a random number generator
/// should be used.
/// The <see cref="Decider.Decide(DecisionKind)"/> method returns a random <see cref="bool"/>
/// value.  The method uses the decisionKind parameter to determine
/// which random number implementation to use.
/// </summary>
internal class Decider
{
    private static readonly Random _RANDOM                                     = new Random();
    private static readonly RNGCryptoServiceProvider _RNGCRYPTOSERVICEPROVIDER = new RNGCryptoServiceProvider();
    private static readonly IValueProvider<bool> _VALUEPROVIDERRANDOMSTANDARD  = ValueProvider.Create(GetNextRandomStandardValue);
    private static readonly IValueProvider<bool> _VALUEPROVIDERRANDOMCRYPTO    = ValueProvider.Create(GetNextRandomStandardValue);

    private static bool GetNextRandomStandardValue() => Convert.ToBoolean(_RANDOM.Next(0, 2));

    private static bool GetNextRandomCryptoValue() => Convert.ToBoolean(Next(_RNGCRYPTOSERVICEPROVIDER, 0, 2));

    private static int Next(RNGCryptoServiceProvider rngCryptoServiceProvider, int minimum, int maximum)
    {
        const int byteCount = 4;
        byte[] bytes = new byte[byteCount];
        rngCryptoServiceProvider.GetBytes(bytes);
        UInt32 scale = BitConverter.ToUInt32(bytes, 0);
        return (int)(minimum + (maximum - minimum) * (scale / (uint.MaxValue + 1.0)));
    }

    /// <summary>
    /// Returns a random boolean value.
    /// </summary>
    /// <param name="decisionKind">Determines how the random value is generated.</param>
    /// <returns>A random boolean value.</returns>
    public static bool Decide(DecisionKind decisionKind)
    {
        IValueProvider<bool> valueProvider = GetValueProvider(decisionKind);
        return valueProvider.Value;
    }

    private static IValueProvider<bool> GetValueProvider(DecisionKind decisionKind)
    {
        IValueProvider<bool> valueProvider = decisionKind switch
        {
            DecisionKind.RandomStandard => _VALUEPROVIDERRANDOMSTANDARD,
            DecisionKind.RandomCrypto   => _VALUEPROVIDERRANDOMCRYPTO,
            _                           => throw new InvalidOperationException($"Unexpected value for {nameof(DecisionKind)} ({decisionKind}).")
        };
        return valueProvider;
    }
}

public sealed class DeciderTests
{
    [TestCase(DecisionKind.RandomStandard)]
    [TestCase(DecisionKind.RandomCrypto)]
    [Category("2 - Demo - Randomization - Decider Tests")]
    // Determines that when the Decider.Decide() method is invoked
    // an exception is not thrown.
    public void WhenTheDecideMethodIsInvoked_ThenAnExceptionIsNotThrown(DecisionKind decisionKind)
    {
        Assert.DoesNotThrow(() => Decider.Decide(decisionKind));
    }
}
```
