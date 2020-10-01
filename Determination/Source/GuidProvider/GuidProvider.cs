using System;

namespace Determination
{
    public sealed class GuidProvider : IGuidProvider
    {
        public Guid Value => Guid.NewGuid();
    }
}
