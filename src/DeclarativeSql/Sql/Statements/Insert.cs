﻿using System.Linq;
using System.Text;



namespace DeclarativeSql.Sql.Statements
{
    /// <summary>
    /// Represents insert statement.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class Insert<T> : Statement<T>, IInsert<T>
    {
        #region Properties
        /// <summary>
        /// Gets the value priority of CreatedAt column.
        /// </summary>
        private ValuePriority CreatedAtPriority { get; }
        #endregion


        #region Constructors
        /// <summary>
        /// Creates instance.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="createdAtPriority"></param>
        public Insert(DbProvider provider, ValuePriority createdAtPriority)
            : base(provider)
            => this.CreatedAtPriority = createdAtPriority;
        #endregion


        #region override
        /// <summary>
        /// Builds query.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="bindParameter"></param>
        internal override void Build(StringBuilder builder, BindParameter bindParameter)
        {
            var bracket = this.DbProvider.KeywordBracket;
            var prefix = this.DbProvider.BindParameterPrefix;
            var columns = this.Table.Columns.Where(x => !x.IsAutoIncrement).ToArray();

            builder.Append("insert into ");
            builder.AppendLine(this.Table.FullName);
            builder.Append("(");
            foreach (var x in columns)
            {
                builder.AppendLine();
                builder.Append("    ");
                builder.Append(bracket.Begin);
                builder.Append(x.ColumnName);
                builder.Append(bracket.End);
                builder.Append(',');
            }
            builder.Length--;
            builder.AppendLine();
            builder.AppendLine(")");
            builder.AppendLine("values");
            builder.Append("(");
            foreach (var x in columns)
            {
                builder.AppendLine();
                builder.Append("    ");
                if ((x.IsCreatedAt || x.IsModifiedAt)
                    && this.CreatedAtPriority == ValuePriority.Attribute
                    && x.CreatedAt.DefaultValue != null)
                {
                    builder.Append(x.CreatedAt.DefaultValue);
                    builder.Append(',');
                }
                else
                {
                    builder.Append(prefix);
                    builder.Append(x.MemberName);
                    builder.Append(',');
                    bindParameter.Add(x.MemberName, null);
                }
            }
            builder.Length--;
            builder.AppendLine();
            builder.Append(")");
        }
        #endregion
    }
}