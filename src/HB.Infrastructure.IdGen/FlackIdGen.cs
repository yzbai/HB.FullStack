/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using HB.FullStack.Common.IdGen;

using IdGen;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Threading;

namespace HB.Infrastructure.IdGen
{
    /// <summary>
    /// warning: 不支持Web Garden
    /// </summary>
    public class FlackIdGen : IDistributedIdGen
    {
        public static void Initialize(IdGenSettings settings)
        {
            //var epoch = new DateTime(2020, 12, 22, 0, 0, 0, DateTimeKind.Utc);
            var structure = new IdStructure(settings.TimestampBits, settings.GeneratorIdBits, settings.SequenceBits);
            var options = new IdGeneratorOptions(structure, new DefaultTimeSource(settings.Epoch));

            StaticIdGen.IdGen = new FlackIdGen(settings.MachineId, options);
        }

        private readonly IdGenerator _idGen;

        public FlackIdGen(int generatorId, IdGeneratorOptions options)
        {
            _idGen = new IdGenerator(generatorId, options);
        }

        //TODO: 解决始终回拨问题
        //https://www.cnblogs.com/jpfss/p/11506960.html
        //https://www.jianshu.com/p/98c202f64652?utm_campaign=haruki&utm_content=note&utm_medium=reader_share&utm_source=weixin

        public long GetId()
        {
            try
            {
                return _idGen.CreateId();
            }
            catch (SequenceOverflowException ex)
            {
                Globals.Logger.LogCritical(ex, $"Id生成器不够用，每秒数量溢出，恭喜，恭喜。");
                return GetId();
            }
            catch (InvalidSystemClockException ex)
            {
                Globals.Logger.LogCritical(ex, $"发生时间回拨");
                Thread.Sleep(5);
                return GetId();
            }
        }
    }
}