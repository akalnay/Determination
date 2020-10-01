////////////////////////////////////////////////////////
// Copyright (c) Alejandro Kalnay                     //
// License: GNU GPLv3                                 //
////////////////////////////////////////////////////////

namespace Determination
{
    public interface IDynamicValueProvider<T>
    {
        T Value { get; }
    }
}