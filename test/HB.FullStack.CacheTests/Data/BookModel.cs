//global using static HB.FullStack.BaseTest.BaseTestClass;
//using System;

//using HB.FullStack.Database.DbModels;

//namespace HB.FullStack.CacheTests
//{
//    [DbModel(DbSchema_Sqlite)]
//    [CacheModel]
//    public class BookModel : TimestampFlackIdDbModel
//    {
//        [DbModelProperty]
//        public string Name { get; set; } = default!;

//        [DbModelProperty]
//        public double Price { get; set; } = default!;
//    }

//    [DbModel(DbSchema_Sqlite)]
//    [CacheModel]
//    public class Guid_BookModel : TimestampGuidDbModel
//    {
//        [DbModelProperty]
//        public string Name { get; set; } = default!;

//        [DbModelProperty]
//        public double Price { get; set; } = default!;
//    }

//    [DbModel(DbSchema_Sqlite)]
//    [CacheModel]
//    public class Book : TimestampFlackIdDbModel
//    {
//        [DbModelProperty]
//        [CacheModelAltKey]
//        public string Name { get; set; } = null!;

//        [DbModelProperty]
//        [CacheModelAltKey]
//        public long BookID { get; set; }

//        [DbModelProperty]
//        public string? Publisher { get; set; }

//        [DbModelProperty]
//        public double Price { get; set; }
//    }

//    [DbModel(DbSchema_Sqlite)]
//    [CacheModel]
//    public class BookModel_Client : TimestampFlackIdDbModel
//    {
//        [DbModelProperty(NeedIndex = true)]
//        public string Name { get; set; } = default!;

//        [DbModelProperty]
//        public double Price { get; set; } = default!;
//    }

//    [DbModel(DbSchema_Sqlite)]
//    [CacheModel]
//    public class Book_Client : TimestampFlackIdDbModel
//    {
//        [CacheModelAltKey]
//        [DbModelProperty]
//        public string Name { get; set; } = null!;

//        [DbModelProperty]
//        [CacheModelAltKey]
//        public long BookID { get; set; }

//        [DbModelProperty]
//        public string? Publisher { get; set; }

//        [DbModelProperty]
//        public double Price { get; set; }
//    }
//}