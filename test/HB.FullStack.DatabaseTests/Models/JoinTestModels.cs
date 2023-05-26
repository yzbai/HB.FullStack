/*
 * Author：Yuzhao Bai
 * Email: yzbai@brlite.com
 * Github: github.com/yzbai
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;

using HB.FullStack.Common;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.BaseTest.Models
{
    public interface IJoinTest_A : IDbModel
    {

    }

    public interface IJoinTest_B : IDbModel
    {

    }

    public interface IJoinTest_AB : IDbModel
    {
        object? AId { get; set; }
        object? BId { get; set; }
    }

    public interface IJoinTest_A_Sub : IDbModel
    {
        object? AId { get; set; }
    }


    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql)]
    public class MySql_Guid_JoinTest_A : DbModel<Guid>, IJoinTest_A
    {
        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql)]
    public class MySql_Long_JoinTest_A : DbModel<long>, IJoinTest_A
    {
        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
    }


    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Sqlite)]
    public class Sqlite_Guid_JoinTest_A : DbModel<Guid>, IJoinTest_A
    {
        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Sqlite)]
    public class Sqlite_Long_JoinTest_A : DbModel<long>, IJoinTest_A
    {
        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql)]
    public class MySql_Guid_JoinTest_B : DbModel<Guid>, IJoinTest_B
    {
        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql)]
    public class MySql_Long_JoinTest_B : DbModel<long>, IJoinTest_B
    {
        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
    }


    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Sqlite)]
    public class Sqlite_Guid_JoinTest_B : DbModel<Guid>, IJoinTest_B
    {
        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Sqlite)]
    public class Sqlite_Long_JoinTest_B : DbModel<long>, IJoinTest_B
    {
        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql)]
    public class MySql_Guid_JoinTest_AB : DbModel<Guid>, IJoinTest_AB
    {
        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }


        public Guid AId { get; set; }

        public Guid BId { get; set; }

        object? IJoinTest_AB.AId { get => AId; set => AId = (Guid)(value ?? Guid.Empty); }

        object? IJoinTest_AB.BId { get => BId; set => BId = (Guid)(value ?? Guid.Empty); }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql)]
    public class MySql_Long_JoinTest_AB : DbModel<long>, IJoinTest_AB
    {
        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }

        public long AId { get; set; }

        public long BId { get; set; }

        object? IJoinTest_AB.AId { get => AId; set => AId = (long)(value ?? 0); }

        object? IJoinTest_AB.BId { get => BId; set => BId = (long)(value ?? 0); }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Sqlite)]
    public class Sqlite_Guid_JoinTest_AB : DbModel<Guid>, IJoinTest_AB
    {
        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }


        public Guid AId { get; set; }

        public Guid BId { get; set; }

        object? IJoinTest_AB.AId { get => AId; set => AId = (Guid)(value ?? Guid.Empty); }

        object? IJoinTest_AB.BId { get => BId; set => BId = (Guid)(value ?? Guid.Empty); }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Sqlite)]
    public class Sqlite_Long_JoinTest_AB : DbModel<long>, IJoinTest_AB
    {
        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }

        public long AId { get; set; }

        public long BId { get; set; }

        object? IJoinTest_AB.AId { get => AId; set => AId = (long)(value ?? 0); }

        object? IJoinTest_AB.BId { get => BId; set => BId = (long)(value ?? 0); }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql)]
    public class MySql_Guid_JoinTest_A_Sub : DbModel<Guid>, IJoinTest_A_Sub
    {
        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }

        public Guid AId { get; set; }

        object? IJoinTest_A_Sub.AId { get => AId; set => AId = (Guid)(value ?? Guid.Empty); }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql)]
    public class MySql_Long_JoinTest_A_Sub : DbModel<long>, IJoinTest_A_Sub
    {
        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }

        public long AId { get; set; }

        object? IJoinTest_A_Sub.AId { get => AId; set => AId = (long)(value ?? 0); }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Sqlite)]
    public class Sqlite_Guid_JoinTest_A_Sub : DbModel<Guid>, IJoinTest_A_Sub
    {
        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }

        public Guid AId { get; set; }

        object? IJoinTest_A_Sub.AId { get => AId; set => AId = (Guid)(value ?? Guid.Empty); }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Sqlite)]
    public class Sqlite_Long_JoinTest_A_Sub : DbModel<long>, IJoinTest_A_Sub
    {
        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }

        public long AId { get; set; }

        object? IJoinTest_A_Sub.AId { get => AId; set => AId = (long)(value ?? 0); }
    }
}