using OrmBenchmark.Core;

using System;

namespace OrmBenchmark.Ado
{
    public class Post : IPost
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public long CreationDate { get; set; }
        public long LastChangeDate { get; set; }
        public int? Counter1 { get; set; }
        public int? Counter2 { get; set; }
        public int? Counter3 { get; set; }
        public int? Counter4 { get; set; }
        public int? Counter5 { get; set; }
        public int? Counter6 { get; set; }
        public int? Counter7 { get; set; }
        public int? Counter8 { get; set; }
        public int? Counter9 { get; set; }

        //public int NotExistColumn { get; set; }
    }
}
