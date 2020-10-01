using System;
using System.Collections;
using System.Collections.Generic;

namespace Determination
{
    public abstract class CalculatingEnumerableEnumeratorBase<TResult> : IEnumerator<TResult>
    {
        private TResult _previousResult;
        private bool _firstIteration = true;

        public TResult Current
        {
            get
            {
                TResult GetTheInitialValue(ref bool firstIteration, TResult initialValue)
                {
                    firstIteration = false;
                    return initialValue;
                }

                TResult result = _firstIteration ? GetTheInitialValue(ref _firstIteration, GetInitialValue()) : GetSubsequentValue(_previousResult);
                _previousResult = result;
                return result;
            }
        }

        protected abstract TResult GetInitialValue();

        protected abstract TResult GetSubsequentValue(TResult previousResult);

        object IEnumerator.Current => Current;

        public bool MoveNext() => true;

        public void Reset() => _firstIteration = true;

        #region IDisposable Support

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
