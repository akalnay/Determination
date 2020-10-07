////////////////////////////////////////////////////////
// Copyright (c) Alejandro Kalnay                     //
// License: GNU GPLv3                                 //
////////////////////////////////////////////////////////

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Determination.Demo
{
    public class DemoTests
    {
        #region DateTimeProvider Demos

        #region Test a method that depends on the current date

        // SUT
        internal string GetTodaysDateAsText(ICurrentDateTimeProvider currentDateTimeProvider)
        {
            return currentDateTimeProvider.Value.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
        }

        [Test]
        [Category("2 - Demo - CurrentDateTimeProvider - GetTodaysDateAsText Tests")]
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
        [Category("2 - Demo - CurrentDateTimeProvider - IsItTeaTime() Tests")]
        // Happy-path test for a method that determines if the current date-time falls within a range.
        // Only one date-time value is provided to the ICurrentDateTimeProvider parameter in the IsItTeaTime1() method.
        public void WhenTheIsItTeaTimeMethodIsInvoked_ThenTheResultIsTheExpectedValue()
        {
            Assert.IsTrue(IsItTeaTime1(CurrentDateTimeProviderStub.Create(new DateTime(2020, 10, 1, 18, 0, 0)),
                                                                            new DateTime(2020, 10, 1, 16, 0, 0),
                                                                            new DateTime(2020, 10, 1, 18, 0, 0)));
        }

        [Test]
        [Category("2 - Demo - CurrentDateTimeProvider - IsItTeaTime() Tests")]
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
        [Category("2 - Demo - CurrentDateTimeProvider - IsItTeaTime() Tests")]
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

        #endregion Tests for the CountdownTimer class

        #endregion Test a Countdown Timer 

        #endregion DateTimeProvider Demos

        #region ValueProvider Demos

        #region Card Game

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
            /// b) To fully test that the <see cref="RemainingCards"/> property no longer contains the
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
                while (continueSelecting)
                {
                    card = SelectRandomCard();
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
            /// <param name="valueProvider">
            /// Provides <see cref="Card"/> instances.
            /// </param>
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
            /// argument.
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
                while (continueSelecting)
                {
                    card = _valueProvider.Value;
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
                Card card = new Card(randomRank, randomSuit);
                return card;
            }

            private static T Min<T>() where T : struct, Enum => Enum.GetValues(typeof(T)).Cast<T>().Min();

            private static int Count<T>() where T : struct, Enum => Enum.GetNames(typeof(T)).Length;
        }

        #region CardGame Tests

        [Test]
        [Category("2 - Demo - Randomization - CardGame Tests")]
        // If the GetCard2() method is invoked and there are no cards remaining then an
        // InvalidOperationException is thrown.
        public void Test1()
        {
            Card[] allCards = Enum.GetValues(typeof(Rank))
                                  .Cast<Rank>()
                                  .SelectMany(rank => Enum.GetValues(typeof(Suit))
                                                          .Cast<Suit>()
                                                          .Select(suit => new Card(rank, suit)))
                                  .ToArray();
            CardGame cardGame = new CardGame(ValueProviderStub.Create<Card>(allCards));
            for (int i = 0; i < allCards.Length; i++)
                _ = cardGame.GetCard2();
            Assert.Throws<InvalidOperationException>(() => cardGame.GetCard2());
        }

        [Test]
        [Category("2 - Demo - Randomization - CardGame Tests")]
        // After a card is retrieved by the GetCard2() method the RemainingCards
        // property must no longer contain the card retrieved.
        public void Test2()
        {
            Card[] allCards = Enum.GetValues(typeof(Rank))
                                  .Cast<Rank>()
                                  .SelectMany(rank => Enum.GetValues(typeof(Suit))
                                                          .Cast<Suit>()
                                                          .Select(suit => new Card(rank, suit)))
                                  .ToArray();
            CardGame cardGame = new CardGame(ValueProviderStub.Create<Card>(allCards));
            for (int i = 0; i < allCards.Length; i++)
            {
                Card card = cardGame.GetCard2();
                CollectionAssert.DoesNotContain(cardGame.RemainingCards, card);
            }
        }

        [Test]
        [Category("2 - Demo - Randomization - CardGame Tests")]
        // After all cards have been retrieved by the GetCard2() method the RemainingCards
        // property must be empty.
        public void Test3()
        {
            Card[] allCards = Enum.GetValues(typeof(Rank))
                                  .Cast<Rank>()
                                  .SelectMany(rank => Enum.GetValues(typeof(Suit))
                                                          .Cast<Suit>()
                                                          .Select(suit => new Card(rank, suit)))
                                  .ToArray();
            CardGame cardGame = new CardGame(ValueProviderStub.Create<Card>(allCards));
            for (int i = 0; i < allCards.Length; i++)
                _ = cardGame.GetCard2();
            CollectionAssert.IsEmpty(cardGame.RemainingCards);
        }

        #endregion CardGame Tests

        #endregion Card Game

        #endregion ValueProvider Demos

        #region GuidProvider Demos

        #region Person

        // Anything that depends on the Guid assigned to a Person1 instance
        // can't be tested because the value of the Guid property does not
        // have an external provenance.
        internal class Person1
        {
            public Person1(string firstName, string lastName)
            {
                FirstName = firstName;
                LastName  = lastName;
                Guid      = Guid.NewGuid();
            }

            public Guid Guid { get; }

            public string FirstName { get; }

            public string LastName { get; }
        }

        // This class is fully testable.  The Guid value will be determined
        // by the particular implementation of the IGuidProvider interface.
        internal class Person2
        {
            public Person2(string firstName, string lastName, IGuidProvider guidProvider)
            {
                FirstName = firstName;
                LastName  = lastName;
                Guid      = guidProvider.Value;
            }

            public Guid Guid { get; }

            public string FirstName { get; }

            public string LastName { get; }
        }

        #endregion Person

        [Test]
        [Category("2 - Demo - GuidProvider - Person Tests")]
        // This test assigns a predefined Guid while creating an object instance.
        // The test verifies that the Guid value in the Person2's Guid property is
        // indeed the Guid assigned when the Person2 instance was created.
        public void WhenTheGuidPropertyOfAPerson2InstanceIsRetrieved_ThenItsValueIsTheGuidAssignedDuringTheCreationOfTheInstance()
        {
            // A real program might allow the framework to assign the Guid by doing:
            // Person2 person2 = new Person2("Jim", "Smith", new GuidProvider());
            Guid guid       = new Guid("7A0908FB-D9F9-4A6D-8F95-EFE4DE3D5027");
            Person2 person2 = new Person2("Jim", "Smith", GuidProviderStub.Create(guid));
            Assert.AreEqual(guid, person2.Guid);
        }

        #endregion GuidProvider Demos
    }
}
