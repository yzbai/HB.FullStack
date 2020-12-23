using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using HB.FullStack.Common.IdGen;
using IdGen;

namespace HB.Infrastructure.IdGen
{
    /// <summary>
    /// warning: 不支持Web Garden
    /// </summary>
    public class IdGenDistributedId : IDistributedIdGen
    {
        public static void Initialize(int machineId)
        {
            var epoch = new DateTime(2020, 12, 22, 0, 0, 0, DateTimeKind.Utc);
            var structure = new IdStructure(41, 9, 13);
            var options = new IdGeneratorOptions(structure, new DefaultTimeSource(epoch));

            IDistributedIdGen.IdGen = new IdGenDistributedId(machineId, options);
        }

        private readonly IdGenerator _idGen;

        public IdGenDistributedId(int generatorId, IdGeneratorOptions options)
        {
            _idGen = new IdGenerator(generatorId, options);
        }

        public long GetId()
        {
            return _idGen.CreateId();
        }
    }
}
