﻿using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using NetModular.Lib.Data.Abstractions;
using NetModular.Lib.Data.Abstractions.Entities;
using NetModular.Lib.Data.Abstractions.Enums;
using NetModular.Lib.Data.Abstractions.Options;
using NetModular.Lib.Data.Core;
using NetModular.Lib.Utils.Core.Extensions;
using NetModular.Lib.Utils.Core.Helpers;
using Npgsql;

namespace NetModular.Lib.Data.PostgreSQL
{
    internal class PostgreSQLAdapter : SqlAdapterAbstract
    {
        public PostgreSQLAdapter(DbOptions dbOptions, DbModuleOptions options) : base(dbOptions, options)
        {
        }

        public override string Database => AppendQuote(Options.Database) + ".";

        public override SqlDialect SqlDialect => SqlDialect.PostgreSQL;

        /// <summary>
        /// 获取最后新增ID语句
        /// </summary>
        public override string IdentitySql => "RETURNING \"id\";";

        public override string FuncLength => "CHAR_LENGTH";

        public override bool ToLower => true;

        public override string GeneratePagingSql(string select, string table, string where, string sort, int skip, int take)
        {
            var sqlBuilder = new StringBuilder();
            sqlBuilder.AppendFormat("SELECT {0} FROM {1} ", select, table);
            if (!string.IsNullOrWhiteSpace(where))
                sqlBuilder.AppendFormat("WHERE {0} ", where);

            if (!string.IsNullOrWhiteSpace(sort))
                sqlBuilder.AppendFormat("ORDER BY {0} ", sort);

            sqlBuilder.AppendFormat("LIMIT {0} ", take);
            if (skip > 0)
            {
                sqlBuilder.AppendFormat("OFFSET {0} ", skip);
            }
            return sqlBuilder.ToString();
        }

        public override string GenerateFirstSql(string select, string table, string where, string sort)
        {
            return GeneratePagingSql(select, table, where, sort, 0, 1);
        }

        public override Guid GenerateSequentialGuid()
        {
            return GuidHelper.NewSequentialGuid(SequentialGuidType.SequentialAsString);
        }

        public override void CreateDatabase(List<IEntityDescriptor> entityDescriptors, IDatabaseCreateEvents events = null)
        {
            var connStrBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = DbOptions.Server,
                Port = DbOptions.Port > 0 ? DbOptions.Port : 5432,
                Database = "postgres",
                Username = DbOptions.UserId,
                Password = DbOptions.Password
            };

            if (DbOptions.NpgsqlDatabaseName.NotNull())
            {
                using var con1 = new NpgsqlConnection(connStrBuilder.ToString());
                con1.Open();
                var existsDatabase = con1.ExecuteScalar($"SELECT 1 FROM pg_catalog.pg_database u where u.datname='{DbOptions.NpgsqlDatabaseName}';").ToInt() > 0;
                if (!existsDatabase)
                {
                    //创建数据库
                    con1.Execute($"CREATE DATABASE {DbOptions.NpgsqlDatabaseName};");
                }
                con1.Close();

                connStrBuilder.Database = DbOptions.NpgsqlDatabaseName;
            }

            using var con = new NpgsqlConnection(connStrBuilder.ToString());
            con.Open();

            //判断数据库是否已存在
            var exist = con.ExecuteScalar($"SELECT 1 FROM pg_namespace WHERE nspname = '{Options.Database}' LIMIT 1;").ToInt() > 0;
            if (!exist)
            {
                //执行创建前事件
                events?.Before().GetAwaiter().GetResult();

                //创建数据库
                con.Execute($"CREATE SCHEMA {Options.Database};");
            }

            //创建表
            foreach (var entityDescriptor in entityDescriptors)
            {
                if (!entityDescriptor.Ignore)
                {
                    con.Execute(CreateTableSql(entityDescriptor));
                }
            }

            if (!exist)
            {
                //执行创建后事件
                events?.After().GetAwaiter().GetResult();
            }

            con.Close();
        }

        private string CreateTableSql(IEntityDescriptor entityDescriptor)
        {
            var columns = entityDescriptor.Columns;
            var sql = new StringBuilder();
            sql.AppendFormat("CREATE TABLE IF NOT EXISTS {0}.{1}(", AppendQuote(Options.Database), AppendQuote(entityDescriptor.TableName.ToLower()));

            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];

                sql.AppendFormat("{0} ", AppendQuote(column.Name.ToLower()));
                sql.AppendFormat("{0} ", Property2Column(column, out string def));

                if (column.IsPrimaryKey)
                {
                    sql.Append("PRIMARY KEY ");
                }

                if (!column.Nullable && !column.IsPrimaryKey)
                {
                    sql.Append("NOT NULL ");
                }

                if (def.NotNull())
                {
                    sql.Append(def);
                }

                if (i < columns.Count - 1)
                {
                    sql.Append(",");
                }
            }

            sql.Append(");");

            return sql.ToString();
        }

        /// <summary>
        /// 属性转换为列
        /// </summary>
        /// <param name="column"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public string Property2Column(IColumnDescriptor column, out string def)
        {
            def = "";
            var propertyType = column.PropertyInfo.PropertyType;
            var isNullable = propertyType.IsNullable();
            if (isNullable)
            {
                propertyType = Nullable.GetUnderlyingType(propertyType);
                if (propertyType == null)
                    throw new Exception("Property2Column error");
            }

            if (propertyType.IsEnum)
            {
                if (!isNullable)
                {
                    def = "DEFAULT 0";
                }

                return "SMALLINT";
            }

            if (propertyType == typeof(Guid))
                return "UUID";

            var typeCode = Type.GetTypeCode(propertyType);
            if (typeCode == TypeCode.String)
            {
                if (column.Max)
                    return "TEXT";

                if (column.Length < 1)
                    return "VARCHAR(50)";

                return $"VARCHAR({column.Length})";
            }

            if (typeCode == TypeCode.Char)
            {
                return $"CHAR({column.Length})";
            }

            if (typeCode == TypeCode.Boolean)
            {
                if (!isNullable)
                {
                    def = "DEFAULT FALSE";
                }
                return "boolean";
            }

            if (typeCode == TypeCode.Byte)
            {
                if (!isNullable)
                {
                    def = "DEFAULT 0";
                }
                return "SMALLINT";
            }

            if (typeCode == TypeCode.Int16)
            {
                if (column.IsPrimaryKey)
                {
                    return "SMALLSERIAL";
                }

                if (!isNullable)
                {
                    def = "DEFAULT 0";
                }
                return "SMALLINT";
            }

            if (typeCode == TypeCode.Int32)
            {
                if (column.IsPrimaryKey)
                {
                    return "SERIAL";
                }

                if (!isNullable)
                {
                    def = "DEFAULT 0";
                }
                return "INTEGER";
            }

            if (typeCode == TypeCode.Int64)
            {
                if (column.IsPrimaryKey)
                {
                    return "BIGSERIAL";
                }

                if (!isNullable)
                {
                    def = "DEFAULT 0";
                }
                return "BIGINT";
            }

            if (typeCode == TypeCode.DateTime)
            {
                if (!isNullable)
                {
                    def = "DEFAULT CURRENT_TIMESTAMP";
                }
                return "TIMESTAMP";
            }

            if (typeCode == TypeCode.Decimal || typeCode == TypeCode.Double || typeCode == TypeCode.Single)
            {
                if (!isNullable)
                {
                    def = "DEFAULT 0";
                }

                return "MONEY";
            }

            return string.Empty;
        }
    }
}
