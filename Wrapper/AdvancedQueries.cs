using Mono.Data.Sqlite;
using SqlLite.Wrapper.QueryExtensions.QueryBuilder;
using System;

namespace SqlLite.Wrapper
{
	public static partial class SqliteHandler
	{
		public static T ReadQuery<T>(string query, Action<SqliteCommand> commandFormatter)
		{
			Type type = typeof(T);
			bool found = false;
			TableInfo table = GetTableInfo(type);
			T entry = table.ConstructEmpty<T>();

			CreateQuery(query, cmd =>
			{
				commandFormatter(cmd);

#pragma warning disable IDE0063 // Use simple 'using' statement
				using (SqliteDataReader reader = cmd.ExecuteReader())
#pragma warning restore IDE0063 // Use simple 'using' statement
				{
					if (!reader.Read()) return;

					found = true;
					ReadEntry(entry, table, reader);
				}
			});

			return found ? entry : default;
		}

		public static T LoadOne<T>(ConditionalQuery<T> expr)
		{
			Type type = typeof(T);
			bool found = false;

			TableInfo table = GetTableInfo(type);
			T entry = table.ConstructEmpty<T>();

			CreateQuery(expr.Query, cmd =>
			{
				expr.FormatParameters(cmd);

#pragma warning disable IDE0063 // Use simple 'using' statement
				using (SqliteDataReader reader = cmd.ExecuteReader())
#pragma warning restore IDE0063 // Use simple 'using' statement
				{
					if (!reader.Read()) return;

					found = true;
					ReadEntry(entry, table, reader);
				}
			});

			return found ? entry : default;
		}
	}
}
