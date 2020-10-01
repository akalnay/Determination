using System;
using System.Linq;

namespace Determination
{
    public sealed class GuidProviderStub : DynamicValueProvider<Guid>, IGuidProvider
    {
        private const string _comparisonFailedErrorMessage = "A new Guid value must be different than the previous one.";

        private GuidProviderStub(params Guid[] values) : this(CompareCurrentAndNext, values)
        {
        }

        private GuidProviderStub(Func<Guid, Guid, bool> compareCurrentAndNext, params Guid[] values) : this(_comparisonFailedErrorMessage, compareCurrentAndNext, values)
        {
        }

        private GuidProviderStub(string comparisonFailedMessage, Func<Guid, Guid, bool> compareCurrentAndNext, params Guid[] values) : base(comparisonFailedMessage, compareCurrentAndNext, values)
        {
        }

        private static bool CompareCurrentAndNext(Guid currentValue, Guid nextValue) => nextValue != currentValue;

        #region Factory Methods

        public static GuidProviderStub Create(params string[] values)
        {
            return Create(values?.Select(value => Guid.Parse(value))?.ToArray());
        }

        public static GuidProviderStub Create(params Guid[] values)
        {
            return new GuidProviderStub(values);
        }

        public static GuidProviderStub Create(Func<Guid, Guid, bool> compareCurrentAndNext, params string[] values)
        {
            return Create(compareCurrentAndNext, values?.Select(value => Guid.Parse(value))?.ToArray());
        }

        public static GuidProviderStub Create(Func<Guid, Guid, bool> compareCurrentAndNext, params Guid[] values)
        {
            return new GuidProviderStub(compareCurrentAndNext, values);
        }

        public static GuidProviderStub Create(string comparisonFailedMessage, Func<Guid, Guid, bool> compareCurrentAndNext, params string[] values)
        {
            return Create(comparisonFailedMessage, compareCurrentAndNext, values?.Select(value => Guid.Parse(value))?.ToArray());
        }

        public static GuidProviderStub Create(string comparisonFailedMessage, Func<Guid, Guid, bool> compareCurrentAndNext, params Guid[] values)
        {
            return new GuidProviderStub(comparisonFailedMessage, compareCurrentAndNext, values);
        }

        #endregion Factory Methods
    }
}
