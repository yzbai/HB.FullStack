using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.Config
{
    public class DbSchema
    {
        public bool IsDefault { get; set; }

        public string Name { get; set; } = null!;

        public DbEngineType EngineType { get; set; }

        /// <summary>
        /// Start from 1
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Can be set lately
        /// </summary>
        public ConnectionString? ConnectionString { get; set; }

        public IList<ConnectionString>? SlaveConnectionStrings { get; set; }

        public IList<DbTableSchema> Tables { get; set; } = new List<DbTableSchema>();
        

        #region Other Settings

        public string? TableNameSuffixToRemove { get; set; } = "Model";

        public bool AutomaticCreateTable { get; set; } = true;

        public bool AddDropStatementWhenCreateTable { get; set; }

        public int MaxBatchNumber { get; set; } = 500;

        /// <summary>
        /// 非强制指定下，是否真正删除
        /// </summary>
        public bool TrulyDelete { get; set; } = false;

        #endregion

        #region Lengths

        public int DefaultVarcharFieldLength { get; set; } = DEFAULT_VARCHAR_LENGTH;

        public int MaxVarcharFieldLength { get; set; } = MAX_VARCHAR_LENGTH;

        public int MaxMediumTextFieldLength { get; set; } = MAX_MEDIUM_TEXT_LENGTH;

        public int MaxLastUserFieldLength { get; set; } = MAX_LAST_USER_LENGTH;

        /// <summary>
        /// Max bytes per row
        /// </summary>
        public int MaxRowBytes { get; set; } = MYSQL_MAX_ROW_BYTES;

        /// <summary>
        /// Encoding Length
        /// eg: If you use utfmb4, then EncodingLength = 4, MaxRowLength = MaxRowBytes / 4
        /// </summary>
        int EncodingLength { get; set; } = MYSQL_UTFMB4;

        /// <summary>
        /// Max char length per row (all fields).
        /// </summary>
        int MaxRowLength => EncodingLength <= 0 ? MaxRowBytes / 4 : MaxRowBytes / EncodingLength;

        public const int MYSQL_MAX_ROW_BYTES = 65535;
        public const int MYSQL_UTFMB4 = 4;
        public const int MAX_LAST_USER_LENGTH = 100;
        public const int MAX_VARCHAR_LENGTH = 16379;
        public const int DEFAULT_VARCHAR_LENGTH = 200;
        public const int MAX_MEDIUM_TEXT_LENGTH = 4194303;

        #endregion

        #region Internal

        internal IDbEngine DbEngine { get; set; } = null!;



        #endregion
    }
}