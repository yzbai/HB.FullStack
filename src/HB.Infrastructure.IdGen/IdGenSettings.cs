/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public class IdGenSettings
    {
        public int MachineId { get; set; }

        public DateTimeOffset Epoch { get; set; } = new DateTimeOffset(2020, 12, 22, 0, 0, 0, TimeSpan.Zero);

        public byte TimestampBits { get; set; } = 41;

        public byte GeneratorIdBits { get; set; } = 10;

        public byte SequenceBits { get; set; } = 12;
    }
}