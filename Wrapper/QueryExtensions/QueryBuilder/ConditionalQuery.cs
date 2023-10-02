using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SqlLite.Wrapper.QueryExtensions.QueryBuilder
{
	public class ConditionalQuery<T> : ExpressionVisitor
	{
		public static implicit operator ConditionalQuery<T>(Expression<Func<T, bool>> expr)
		{
			return new ConditionalQuery<T>(expr);
		}

		private Stack<Condition> conditions;

		public string Query { get; internal set; }

		public ConditionalQuery(Expression<Func<T, bool>> expr) 
		{
			Visit(expr);
			throw new NotImplementedException();
		}

		internal void FormatParameters(SqliteCommand cmd)
		{
			throw new NotImplementedException();
		}
	}
}
