using System;

namespace Determination
{
    public sealed class CurrentDateTimeProvider : ICurrentDateTimeProvider
    {
        public DateTime Value => DateTime.Now;
    }
}
