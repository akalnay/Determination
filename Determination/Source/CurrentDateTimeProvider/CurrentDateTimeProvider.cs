////////////////////////////////////////////////////////
// Copyright (c) Alejandro Kalnay                     //
// License: GNU GPLv3                                 //
////////////////////////////////////////////////////////

using System;

namespace Determination
{
    public sealed class CurrentDateTimeProvider : ICurrentDateTimeProvider
    {
        public DateTime Value => DateTime.Now;
    }
}
