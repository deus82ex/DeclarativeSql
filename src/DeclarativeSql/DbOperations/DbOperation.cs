﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dapper;
using DeclarativeSql.Sql;



namespace DeclarativeSql.DbOperations
{
    using Ctor = Func<IDbConnection, IDbTransaction, int?, DbOperation>;


    /// <summary>
    /// Provides database operations.
    /// </summary>
    internal class DbOperation
    {
        #region Properties
        /// <summary>
        /// Gets the database connection.
        /// </summary>
        protected IDbConnection Connection { get; }


        /// <summary>
        /// Gets the database transaction.
        /// </summary>
        protected IDbTransaction Transaction { get; }


        /// <summary>
        /// Gets the database provider.
        /// </summary>
        protected DbProvider DbProvider { get; }


        /// <summary>
        /// Gets the operation timeout.
        /// </summary>
        protected int? Timeout { get; }


        /// <summary>
        /// Gets the <see cref="DbOperation"/> factory by database connection type.
        /// </summary>
        internal static Dictionary<Type, Ctor> Factory { get; }
            = new Dictionary<Type, Ctor>();
        #endregion


        #region Constructors
        /// <summary>
        /// Creates instance.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="provider"></param>
        /// <param name="timeout"></param>
        protected DbOperation(IDbConnection connection, IDbTransaction transaction, DbProvider provider, int? timeout)
        {
            this.Connection = connection;
            this.Transaction = transaction;
            this.DbProvider = provider;
            this.Timeout = timeout;
        }
        #endregion


        #region Create
        /// <summary>
        /// Creates instance.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static DbOperation Create(IDbConnection connection, int? timeout)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            return Create(connection, null, timeout);
        }


        /// <summary>
        /// Creates instance.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static DbOperation Create(IDbTransaction transaction, int? timeout)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));
            return Create(transaction.Connection, transaction, timeout);
        }


        /// <summary>
        /// Creates instance.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private static DbOperation Create(IDbConnection connection, IDbTransaction transaction, int? timeout)
        {
            //--- override
            var type = connection.GetType();
            if (Factory.TryGetValue(type, out var ctor))
                return ctor(connection, transaction, timeout);

            //--- default
            return type.FullName switch
            {
                "System.Data.SqlClient.SqlConnection"
                    => new SqlServerOperation(connection, transaction, DbProvider.SqlServer, timeout),

                "Microsoft.Data.SqlClient.SqlConnection"
                    => new SqlServerOperation(connection, transaction, DbProvider.SqlServer, timeout),

                "MySql.Data.MySqlClient.MySqlConnection"
                    => new MySqlOperation(connection, transaction, DbProvider.MySql, timeout),

                "Microsoft.Data.Sqlite.SqliteConnection"
                    => new SqliteOperation(connection, transaction, DbProvider.Sqlite, timeout),

                "Npgsql.NpgsqlConnection"
                    => new DbOperation(connection, transaction, DbProvider.PostgreSql, timeout),

                "Oracle.ManagedDataAccess.Client.OracleConnection"
                    => new DbOperation(connection, transaction, DbProvider.Oracle, timeout),

                _ => throw new NotSupportedException(),
            };
        }
        #endregion


        #region Count
        /// <summary>
        /// Gets the number of records in the specified table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>Record count</returns>
        public virtual ulong Count<T>()
        {
            var query = this.DbProvider.QueryBuilder.Count<T>().Build();
            return this.Connection.ExecuteScalar<ulong>(query.Statement, query.BindParameter, this.Transaction, this.Timeout);
        }


        /// <summary>
        /// Gets the number of records that match the specified condition in the specified table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns>Record count</returns>
        public virtual ulong Count<T>(Expression<Func<T, bool>> predicate)
        {
            var query = this.DbProvider.QueryBuilder.Count<T>().Where(predicate).Build();
            return this.Connection.ExecuteScalar<ulong>(query.Statement, query.BindParameter, this.Transaction, this.Timeout);
        }


        /// <summary>
        /// Gets the number of records in the specified table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>Record count</returns>
        public virtual Task<ulong> CountAsync<T>()
        {
            var query = this.DbProvider.QueryBuilder.Count<T>().Build();
            return this.Connection.ExecuteScalarAsync<ulong>(query.Statement, query.BindParameter, this.Transaction, this.Timeout);
        }


        /// <summary>
        /// Gets the number of records that match the specified condition in the specified table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns>Record count</returns>
        public virtual Task<ulong> CountAsync<T>(Expression<Func<T, bool>> predicate)
        {
            var query = this.DbProvider.QueryBuilder.Count<T>().Where(predicate).Build();
            return this.Connection.ExecuteScalarAsync<ulong>(query.Statement, query.BindParameter, this.Transaction, this.Timeout);
        }
        #endregion


        #region Select
        /// <summary>
        /// Gets all records from the specified table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="properties"></param>
        /// <returns></returns>
        public virtual List<T> Select<T>(Expression<Func<T, object>> properties)
        {
            var query = this.DbProvider.QueryBuilder.Select(properties).Build();
            return this.Connection.Query<T>(query.Statement, query.BindParameter, this.Transaction, true, this.Timeout) as List<T>;
        }


        /// <summary>
        /// Gets records that match the specified condition from the specified table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public virtual List<T> Select<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> properties)
        {
            var query = this.DbProvider.QueryBuilder.Select(properties).Where(predicate).Build();
            return this.Connection.Query<T>(query.Statement, query.BindParameter, this.Transaction, true, this.Timeout) as List<T>;
        }


        /// <summary>
        /// Gets all records from the specified table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="properties"></param>
        /// <returns></returns>
        public virtual async Task<List<T>> SelectAsync<T>(Expression<Func<T, object>> properties)
        {
            var query = this.DbProvider.QueryBuilder.Select(properties).Build();
            var result = await this.Connection.QueryAsync<T>(query.Statement, query.BindParameter, this.Transaction, this.Timeout).ConfigureAwait(false);
            return result as List<T>;
        }


        /// <summary>
        /// Gets records that match the specified condition from the specified table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public virtual async Task<List<T>> SelectAsync<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> properties)
        {
            var query = this.DbProvider.QueryBuilder.Select(properties).Where(predicate).Build();
            var result = await this.Connection.QueryAsync<T>(query.Statement, query.BindParameter, this.Transaction, this.Timeout).ConfigureAwait(false);
            return result as List<T>;
        }
        #endregion


        #region Insert
        /// <summary>
        /// Inserts the specified data into the table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="createdAt"></param>
        /// <returns>Effected rows count</returns>
        public virtual int Insert<T>(T data, ValuePriority createdAt)
        {
            var query = this.DbProvider.QueryBuilder.Insert<T>(createdAt).Build();
            return this.Connection.Execute(query.Statement, data, this.Transaction, this.Timeout);
        }


        /// <summary>
        /// Inserts the specified data into the table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="createdAt"></param>
        /// <returns>Effected rows count</returns>
        public virtual Task<int> InsertAsync<T>(T data, ValuePriority createdAt)
        {
            var query = this.DbProvider.QueryBuilder.Insert<T>(createdAt).Build();
            return this.Connection.ExecuteAsync(query.Statement, data, this.Transaction, this.Timeout);
        }
        #endregion


        #region InsertMulti
        /// <summary>
        /// Inserts the specified data into the table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="createdAt"></param>
        /// <returns>Effected rows count</returns>
        public virtual int InsertMulti<T>(IEnumerable<T> data, ValuePriority createdAt)
        {
            var query = this.DbProvider.QueryBuilder.Insert<T>(createdAt).Build();
            return this.Connection.Execute(query.Statement, data, this.Transaction, this.Timeout);
        }


        /// <summary>
        /// Inserts the specified data into the table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="createdAt"></param>
        /// <returns>Effected rows count</returns>
        public virtual Task<int> InsertMultiAsync<T>(IEnumerable<T> data, ValuePriority createdAt)
        {
            var query = this.DbProvider.QueryBuilder.Insert<T>(createdAt).Build();
            return this.Connection.ExecuteAsync(query.Statement, data, this.Transaction, this.Timeout);
        }
        #endregion


        #region BulkInsert
        /// <summary>
        /// Inserts the specified data into the table using the bulk method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="createdAt"></param>
        /// <returns>Effected rows count</returns>
        public virtual int BulkInsert<T>(IEnumerable<T> data, ValuePriority createdAt)
            => throw new NotSupportedException();


        /// <summary>
        /// Inserts the specified data into the table using the bulk method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="createdAt"></param>
        /// <returns>Effected rows count</returns>
        public virtual Task<int> BulkInsertAsync<T>(IEnumerable<T> data, ValuePriority createdAt)
            => throw new NotSupportedException();
        #endregion


        #region InsertAndGetId
        /// <summary>
        /// Inserts the specified data into the table and returns the automatically incremented ID.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="createdAt"></param>
        /// <returns>Auto incremented ID</returns>
        public virtual long InsertAndGetId<T>(T data, ValuePriority createdAt)
            => throw new NotSupportedException();


        /// <summary>
        /// Inserts the specified data into the table and returns the automatically incremented ID.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="createdAt"></param>
        /// <returns>Auto incremented ID</returns>
        public virtual Task<long> InsertAndGetIdAsync<T>(T data, ValuePriority createdAt)
            => throw new NotSupportedException();
        #endregion


        #region InsertIgnore
        /// <summary>
        /// Inserts the specified data into the table.
        /// Insertion processing is not performed when there is a collision with a unique constraint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="createdAt"></param>
        /// <returns>Effected row count</returns>
        public virtual int InsertIgnore<T>(T data, ValuePriority createdAt)
            => throw new NotSupportedException();


        /// <summary>
        /// Inserts the specified data into the table.
        /// Insertion processing is not performed when there is a collision with a unique constraint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="createdAt"></param>
        /// <returns>Effected row count</returns>
        public virtual Task<int> InsertIgnoreAsync<T>(T data, ValuePriority createdAt)
            => throw new NotSupportedException();
        #endregion


        #region InsertIgnoreMulti
        /// <summary>
        /// Inserts the specified data into the table.
        /// Insertion processing is not performed when there is a collision with a unique constraint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="createdAt"></param>
        /// <returns>Effected row count</returns>
        public virtual int InsertIgnoreMulti<T>(IEnumerable<T> data, ValuePriority createdAt)
            => throw new NotSupportedException();


        /// <summary>
        /// Inserts the specified data into the table.
        /// Insertion processing is not performed when there is a collision with a unique constraint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="createdAt"></param>
        /// <returns>Effected row count</returns>
        public virtual Task<int> InsertIgnoreMultiAsync<T>(IEnumerable<T> data, ValuePriority createdAt)
            => throw new NotSupportedException();
        #endregion


        #region Update
        /// <summary>
        /// Updates records with the specified data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="properties"></param>
        /// <param name="modifiedAt"></param>
        /// <returns>Effected rows count</returns>
        public virtual int Update<T>(T data, Expression<Func<T, object>> properties, ValuePriority modifiedAt)
        {
            var query = this.DbProvider.QueryBuilder.Update(properties, modifiedAt).Build();
            return this.Connection.Execute(query.Statement, data, this.Transaction, this.Timeout);
        }


        /// <summary>
        /// Updates records that match the specified conditions with the specified data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="predicate"></param>
        /// <param name="properties"></param>
        /// <param name="modifiedAt"></param>
        /// <returns>Effected rows count</returns>
        public virtual int Update<T>(T data, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> properties, ValuePriority modifiedAt)
        {
            var query = this.DbProvider.QueryBuilder.Update(properties, modifiedAt).Build();
            if (query.BindParameter == null)
            {
                return this.Connection.Execute(query.Statement, data, this.Transaction, this.Timeout);
            }
            else
            {
                query.BindParameter.Merge(data);
                return this.Connection.Execute(query.Statement, query.BindParameter, this.Transaction, this.Timeout);
            }
        }


        /// <summary>
        /// Updates records with the specified data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="properties"></param>
        /// <param name="modifiedAt"></param>
        /// <returns>Effected rows count</returns>
        public virtual Task<int> UpdateAsync<T>(T data, Expression<Func<T, object>> properties, ValuePriority modifiedAt)
        {
            var query = this.DbProvider.QueryBuilder.Update(properties, modifiedAt).Build();
            return this.Connection.ExecuteAsync(query.Statement, data, this.Transaction, this.Timeout);
        }


        /// <summary>
        /// Updates records that match the specified conditions with the specified data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="predicate"></param>
        /// <param name="properties"></param>
        /// <param name="modifiedAt"></param>
        /// <returns>Effected rows count</returns>
        public virtual Task<int> UpdateAsync<T>(T data, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> properties, ValuePriority modifiedAt)
        {
            var query = this.DbProvider.QueryBuilder.Update(properties, modifiedAt).Build();
            if (query.BindParameter == null)
            {
                return this.Connection.ExecuteAsync(query.Statement, data, this.Transaction, this.Timeout);
            }
            else
            {
                query.BindParameter.Merge(data);
                return this.Connection.ExecuteAsync(query.Statement, query.BindParameter, this.Transaction, this.Timeout);
            }
        }
        #endregion


        #region Delete
        /// <summary>
        /// Deletes all records from the specified table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>Effected rows count</returns>
        public virtual int Delete<T>()
        {
            var query = this.DbProvider.QueryBuilder.Delete<T>().Build();
            return this.Connection.Execute(query.Statement, query.BindParameter, this.Transaction, this.Timeout);
        }


        /// <summary>
        /// Deletes records that match the specified conditions from the specified table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns>Effected rows count</returns>
        public virtual int Delete<T>(Expression<Func<T, bool>> predicate)
        {
            var query = this.DbProvider.QueryBuilder.Delete<T>().Where(predicate).Build();
            return this.Connection.Execute(query.Statement, query.BindParameter, this.Transaction, this.Timeout);
        }


        /// <summary>
        /// Deletes all records from the specified table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>Effected rows count</returns>
        public virtual Task<int> DeleteAsync<T>()
        {
            var query = this.DbProvider.QueryBuilder.Delete<T>().Build();
            return this.Connection.ExecuteAsync(query.Statement, query.BindParameter, this.Transaction, this.Timeout);
        }


        /// <summary>
        /// Deletes records that match the specified conditions from the specified table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns>Effected rows count</returns>
        public virtual Task<int> DeleteAsync<T>(Expression<Func<T, bool>> predicate)
        {
            var query = this.DbProvider.QueryBuilder.Delete<T>().Where(predicate).Build();
            return this.Connection.ExecuteAsync(query.Statement, query.BindParameter, this.Transaction, this.Timeout);
        }
        #endregion


        #region Truncate
        /// <summary>
        /// Truncates the specified table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>-1</returns>
        public virtual int Truncate<T>()
        {
            var query = this.DbProvider.QueryBuilder.Truncate<T>().Build();
            return this.Connection.Execute(query.Statement, query.BindParameter, this.Transaction, this.Timeout);
        }


        /// <summary>
        /// Truncates the specified table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>-1</returns>
        public virtual Task<int> TruncateAsync<T>()
        {
            var query = this.DbProvider.QueryBuilder.Truncate<T>().Build();
            return this.Connection.ExecuteAsync(query.Statement, query.BindParameter, this.Transaction, this.Timeout);
        }
        #endregion
    }
}
