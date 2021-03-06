﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DeclarativeSql.Annotations;



namespace DeclarativeSql.Mapping
{
    /// <summary>
    /// Provides table mapping information.
    /// </summary>
    internal sealed class TableInfo
    {
        #region Properties
        /// <summary>
        /// Gets the kind of database.
        /// </summary>
        public DbKind Database { get; private set; }


        /// <summary>
        /// Gets the type that is mapped to the table.
        /// </summary>
        public Type Type { get; private set; }


        /// <summary>
        /// Gets the schema name.
        /// </summary>
        public string Schema { get; private set; }


        /// <summary>
        /// Gets the table name.
        /// </summary>
        public string Name { get; private set; }


        /// <summary>
        /// Gets the full table name.
        /// </summary>
        public string FullName { get; private set; }


        /// <summary>
        /// Gets the column mapping information.
        /// </summary>
        public IReadOnlyList<ColumnInfo> Columns { get; private set; }


        /// <summary>
        /// Gets the column mapping information by member name.
        /// </summary>
        public IReadOnlyDictionary<string, ColumnInfo> ColumnsByMemberName { get; private set; }
        #endregion


        #region Constructors
        /// <summary>
        /// Creates instance.
        /// </summary>
        private TableInfo()
        {}
        #endregion


        #region Get
        /// <summary>
        /// Gets the table mapping information corresponding to the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="database"></param>
        /// <returns></returns>
        public static TableInfo Get<T>(DbKind database)
            => Cache<T>.Instances.TryGetValue(database, out var value)
            ? value
            : throw new NotSupportedException();
        #endregion


        #region Internal Cache
        /// <summary>
        /// Provides <see cref="TableInfo"/> cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private static class Cache<T>
        {
            /// <summary>
            /// Gets the instances by <see cref="DbKind"/>.
            /// </summary>
            public static IReadOnlyDictionary<DbKind, TableInfo> Instances { get; }


            /// <summary>
            /// Static constructors
            /// </summary>
            static Cache()
            {
                var type = typeof(T);
                var flags = BindingFlags.Instance | BindingFlags.Public;
                var attributes = type.GetCustomAttributes<TableAttribute>(true).ToDictionary(x => x.Database);
                var properties = type.GetProperties(flags);
                var fields = type.GetFields(flags);
                var dbs = Enum.GetValues(typeof(DbKind)) as DbKind[];
                var result = new Dictionary<DbKind, TableInfo>(dbs.Length);
                foreach (var db in dbs)
                {
                    attributes.TryGetValue(db, out var table);
                    var provider = DbProvider.ByDatabase[db];
                    var b = provider.KeywordBracket;
                    var schema = table?.Schema ?? provider.DefaultSchema;
                    var name = table?.Name ?? type.Name;
                    var columns
                        = properties.Select(x => new ColumnInfo(db, x))
                        .Concat(fields.Select(x => new ColumnInfo(db, x)))
                        .ToArray();
                    result[db] = new TableInfo
                    {
                        Database = db,
                        Type = type,
                        Schema = schema,
                        Name = name,
                        FullName
                            = string.IsNullOrEmpty(schema)
                            ? $"{b.Begin}{name}{b.End}"
                            : $"{b.Begin}{schema}{b.End}.{b.Begin}{name}{b.End}",
                        Columns = columns,
                        ColumnsByMemberName = columns.ToDictionary(x => x.MemberName),
                    };
                }
                Instances = result;
            }
        }
        #endregion
    }
}
