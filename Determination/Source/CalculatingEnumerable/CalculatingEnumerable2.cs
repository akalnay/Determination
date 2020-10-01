using System;
using System.Collections;
using System.Collections.Generic;

namespace Determination
{
    internal sealed class CalculatingEnumerable2<TFactor, TResult> : IEnumerable<TResult>
    {
        private class CalculatingEnumerable2Enumerator<TFctr, TRslt> : CalculatingEnumerableEnumeratorBase<TRslt>
        {
            private readonly TRslt _initialValue;
            private readonly TFctr _factor;
            private readonly Func<TRslt, TFctr, TRslt> _getCurrentValue;

            public CalculatingEnumerable2Enumerator(TRslt initialValue, TFctr factor, Func<TRslt, TFctr, TRslt> getCurrentValue)
            {
                _initialValue    = initialValue;
                _factor          = factor;
                _getCurrentValue = getCurrentValue;
            }

            protected override TRslt GetInitialValue() => _initialValue;

            protected override TRslt GetSubsequentValue(TRslt previousResult) => _getCurrentValue(previousResult, _factor);
        }

        private readonly TResult _initialValue;
        private readonly TFactor _factor;
        private readonly Func<TResult, TFactor, TResult> _getCurrentValue;

        public CalculatingEnumerable2(TResult initialValue, TFactor factor, Func<TResult, TFactor, TResult> getCurrentValue)
        {
            _initialValue    = initialValue;
            _factor          = factor;
            _getCurrentValue = getCurrentValue ?? throw new ArgumentNullException(nameof(getCurrentValue));
        }

        public IEnumerator<TResult> GetEnumerator() => new CalculatingEnumerable2Enumerator<TFactor, TResult>(_initialValue, _factor, _getCurrentValue);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
