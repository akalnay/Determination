using System;
using System.Collections.Generic;
using System.Linq;

namespace Determination
{
    public abstract class DynamicValueProvider<T> : IDynamicValueProvider<T>
    {
        #region Fields

        private const string _comparisonFailedMessageDefault = "Current and next value comparison failed.";
        private readonly IEnumerator<T> _enumerator;
        private readonly Func<T, T, bool> _compareCurrentAndNext;
        private readonly string _comparisonFailedMessage;
        private T _current;
        private bool _firstIteration = true;
        private T _currentValue;

        #endregion Fields

        #region Constructors

        protected DynamicValueProvider(params T[] values) : this(CompareCurrentAndNext, values)
        {
        }

        protected DynamicValueProvider(Func<T, T, bool> compareCurrentAndNext, params T[] values) : this(_comparisonFailedMessageDefault, compareCurrentAndNext, values)
        {
        }

        protected DynamicValueProvider(string comparisonFailedMessage, Func<T, T, bool> compareCurrentAndNext, params T[] values) : this(comparisonFailedMessage, compareCurrentAndNext, values?.AsEnumerable() ?? throw new ArgumentNullException(nameof(values)))
        {
        }

        protected DynamicValueProvider(IEnumerable<T> values) : this(CompareCurrentAndNext, values)
        {
        }

        protected DynamicValueProvider(Func<T, T, bool> compareCurrentAndNext, IEnumerable<T> values) : this(_comparisonFailedMessageDefault, compareCurrentAndNext, values)
        {
        }

        protected DynamicValueProvider(string comparisonFailedMessage, Func<T, T, bool> compareCurrentAndNext, IEnumerable<T> values)
        {
            _enumerator              = values?.GetEnumerator() ?? throw new ArgumentNullException(nameof(values));
            _compareCurrentAndNext   = compareCurrentAndNext ?? throw new ArgumentNullException(nameof(compareCurrentAndNext));
            _comparisonFailedMessage = comparisonFailedMessage ?? throw new ArgumentNullException(nameof(comparisonFailedMessage));
        }

        private static bool CompareCurrentAndNext(T currentValue, T nextValue) => true;

        #endregion Constructors

        #region Properties

        public T Value
        {
            get
            {
                T GetTheFirstValue(IEnumerator<T> enumerator, ref bool firstIteration)
                {
                    try                             // It is not required for implementers of the IEnumerator interface to implement the Reset()
                    {                               // method.  An example of when Reset() is not implemented is when an IEnumerable is built out
                        enumerator.Reset();         // of yield statements.  For example:    
                    }                               // public static IEnumerable<string> GetTextValues()
                                                    // { 
                                                    //      yield return "abc"; 
                                                    // }   
                    catch (Exception e) when (e is NotSupportedException || e is NotImplementedException)      
                    {                                  
                    }                                
                    firstIteration = false;             
                    return GetNextValue(enumerator);    
                }

                T GetTheNextValue(IEnumerator<T> enumerator, T current, Func<T, T, bool> compareCurrentAndNext, string comparisonFailedMessage)
                {
                    T nextValue = GetNextValue(enumerator);
                    if (!compareCurrentAndNext(current, nextValue))
                        throw new InvalidOperationException(comparisonFailedMessage);
                    return nextValue;
                }

                T GetNextValue(IEnumerator<T> enumerator)
                {
                    bool moveResult = enumerator.MoveNext();
                    if (!moveResult)
                        throw new InvalidOperationException("Enumerator has passed the end of the enumerable.");
                    return enumerator.Current;
                }

                _current = _firstIteration ? GetTheFirstValue(_enumerator, ref _firstIteration) 
                                           : GetTheNextValue(_enumerator, _current, _compareCurrentAndNext, _comparisonFailedMessage);
                Count++;
                CurrentValue = _current;
                if (Count == 1)
                    InitialValue = CurrentValue;
                return _current;
            }
        }

        // Keeps track of how many times the Value property has been retrieved.
        public long Count { get; private set; }

        public T InitialValue { get; private set; }

        // Allows for inspecting the current value without causing the enumerator to iterate
        public T CurrentValue
        {
            get
            {
                if (Count < 1)
                    throw new InvalidOperationException($"The {nameof(CurrentValue)} property is only valid after the {nameof(Value)} property has been retrieved at least once.");
                return _currentValue;
            }
            private set => _currentValue = value;
        }

        #endregion Properties

        #region Methods

        protected virtual string CurrentValueToString() => CurrentValue.ToString();

        public override string ToString() => $"{GetType().FullName}{(Count < 1 ? String.Empty : " - " + CurrentValueToString())}";

        #endregion Methods
    }
}
