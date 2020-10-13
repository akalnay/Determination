////////////////////////////////////////////////////////
// Copyright (c) Alejandro Kalnay                     //
// License: GNU GPLv3                                 //
////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Determination
{
    public sealed class CurrentDateTimeProviderStub : DynamicValueProvider<DateTime>, ICurrentDateTimeProvider
    {
        private const string _COMPARISONFAILEDERRORMESSAGE = "A new DateTime value must be greater than the previous one.";

        private CurrentDateTimeProviderStub(params DateTime[] values) : this(CompareCurrentAndNext, values)
        {
        }

        private CurrentDateTimeProviderStub(Func<DateTime, DateTime, bool> compareCurrentAndNext, params DateTime[] values) : this(_COMPARISONFAILEDERRORMESSAGE, compareCurrentAndNext, values)
        {
        }

        private CurrentDateTimeProviderStub(string comparisonFailedMessage, Func<DateTime, DateTime, bool> compareCurrentAndNext, params DateTime[] values) : base(comparisonFailedMessage, compareCurrentAndNext, values)
        {
        }

        private CurrentDateTimeProviderStub(IEnumerable<DateTime> values) : this(CompareCurrentAndNext, values)
        {
        }

        private CurrentDateTimeProviderStub(Func<DateTime, DateTime, bool> compareCurrentAndNext, IEnumerable<DateTime> values) : this(_COMPARISONFAILEDERRORMESSAGE, compareCurrentAndNext, values)
        {
        }

        private CurrentDateTimeProviderStub(string comparisonFailedMessage, Func<DateTime, DateTime, bool> compareCurrentAndNext, IEnumerable<DateTime> values) : base(comparisonFailedMessage, compareCurrentAndNext, values)
        {
        }

        protected override string CurrentValueToString() => CurrentValue.ToString(CultureInfo.CurrentCulture);

        private static bool CompareCurrentAndNext(DateTime currentValue, DateTime nextValue)
        {
            return nextValue > currentValue;
        }

        #region Factory Methods

        public static CurrentDateTimeProviderStub Create(params DateTime[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (values.Length < 1)
                throw new ArgumentException($"{nameof(values)} array is empty.");
            return new CurrentDateTimeProviderStub(values);
        }

        public static CurrentDateTimeProviderStub Create(Func<DateTime, DateTime, bool> compareCurrentAndNext, params DateTime[] values)
        {
            return new CurrentDateTimeProviderStub(compareCurrentAndNext, values);
        }

        public static CurrentDateTimeProviderStub Create(string comparisonFailedMessage, Func<DateTime, DateTime, bool> compareCurrentAndNext, params DateTime[] values)
        {
            return new CurrentDateTimeProviderStub(comparisonFailedMessage, compareCurrentAndNext, values);
        }

        public static CurrentDateTimeProviderStub Create<T>(DateTime initialValue, T factor, Func<DateTime, T, DateTime> getNextValue)
        {
            return new CurrentDateTimeProviderStub(new CalculatingEnumerable2<T, DateTime>(initialValue, factor, getNextValue));
        }

        public static CurrentDateTimeProviderStub Create<T>(Func<DateTime, DateTime, bool> compareCurrentAndNext, DateTime initialValue, T factor, Func<DateTime, T, DateTime> getNextValue)
        {
            return new CurrentDateTimeProviderStub(compareCurrentAndNext, new CalculatingEnumerable2<T, DateTime>(initialValue, factor, getNextValue));
        }

        public static CurrentDateTimeProviderStub Create<T>(string comparisonFailedMessage, Func<DateTime, DateTime, bool> compareCurrentAndNext, DateTime initialValue, T factor, Func<DateTime, T, DateTime> getNextValue)
        {
            return new CurrentDateTimeProviderStub(comparisonFailedMessage, compareCurrentAndNext, new CalculatingEnumerable2<T, DateTime>(initialValue, factor, getNextValue));
        }

        #endregion Factory Methods
    }
}
