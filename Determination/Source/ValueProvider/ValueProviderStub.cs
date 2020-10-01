using System;

namespace Determination
{
    public sealed class ValueProviderStub<T> : DynamicValueProvider<T>, IValueProvider<T>
    {
        internal ValueProviderStub(params T[] values) : base(values)
        {
        }

        internal ValueProviderStub(Func<T, T, bool> compareCurrentAndNext, params T[] values) : base(compareCurrentAndNext, values)
        {
        }

        internal ValueProviderStub(string comparisonFailedMessage, Func<T, T, bool> compareCurrentAndNext, params T[] values) : base(comparisonFailedMessage, compareCurrentAndNext, values)
        {
        }
    }

    #region Factory Methods

    public static class ValueProviderStub
    {
        public static ValueProviderStub<T> Create<T>(params T[] values)
        {
            return new ValueProviderStub<T>(values);
        }

        public static ValueProviderStub<T> Create<T>(Func<T, T, bool> compareCurrentAndNext, params T[] values)
        {
            return new ValueProviderStub<T>(compareCurrentAndNext, values);
        }

        public static ValueProviderStub<T> Create<T>(string comparisonFailedMessage, Func<T, T, bool> compareCurrentAndNext, params T[] values)
        {
            return new ValueProviderStub<T>(comparisonFailedMessage, compareCurrentAndNext, values);
        }
    }

    #endregion Factory Methods
}
