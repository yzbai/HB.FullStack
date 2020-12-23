using System;
using HB.FullStack.Common.IdGen;
using IdGen;

namespace HB.Infrastructure.IdGen
{
    public class IdGenDistributedId : IDistributedIdGen
    {
        private readonly IdGenerator _idGen;

        public IdGenDistributedId()
        {
            _idGen = new IdGenerator(0);
        }

        public long GetId()
        {
            return _idGen.CreateId();
        }
    }
}
