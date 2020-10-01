////////////////////////////////////////////////////////
// Copyright (c) Alejandro Kalnay                     //
// License: GNU GPLv3                                 //
////////////////////////////////////////////////////////

using System;

namespace Determination
{
    public sealed class ValueProvider<T> : IValueProvider<T>
    {
        private readonly Func<T> _getValue;

        internal ValueProvider(Func<T> getValue)
        {
            _getValue = getValue ?? throw new ArgumentNullException(nameof(getValue));
        }

        public T Value => _getValue();
    }

    public static class ValueProvider
    {
        public static ValueProvider<T> Create<T>(Func<T> getValue)
        {
            return new ValueProvider<T>(getValue);
        }
    }
}
