using System;
using System.Collections;
using System.Collections.Generic;

namespace Determination
{
    internal sealed class CalculatingEnumerable0<TResult> : IEnumerable<TResult>
    {
        private class CalculatingEnumerable0Enumerator<TRslt> : CalculatingEnumerableEnumeratorBase<TRslt>
        {
            private readonly Func<TRslt> _function;

            public CalculatingEnumerable0Enumerator(Func<TRslt> function)
            {
                _function = function;
            }

            protected override TRslt GetInitialValue() => _function();

            protected override TRslt GetSubsequentValue(TRslt previousResult) => _function();
        }

        private readonly Func<TResult> _function;

        public CalculatingEnumerable0(Func<TResult> function)
        {
            _function = function;
        }

        public IEnumerator<TResult> GetEnumerator() => new CalculatingEnumerable0Enumerator<TResult>(_function);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
