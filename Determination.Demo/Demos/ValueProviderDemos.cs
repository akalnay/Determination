using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Determination.Demo
{
    public sealed class ValueProviderDemos
    {
        #region Card Game

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
                Card card = default;
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
                Card card = default;
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
                Card card = new Card(randomRank, randomSuit);
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
            Card[] allCards = GetAllCards();
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
            Card[] allCards = GetAllCards();
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

        #endregion Card Game
    }
}
