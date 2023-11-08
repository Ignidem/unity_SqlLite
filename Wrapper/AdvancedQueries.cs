using Mono.Data.Sqlite;
using SqlLite.Wrapper.QueryExtensions.QueryBuilder;
using SqlLite.Wrapper.Serialization;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace SqlLite.Wrapper
{
	public partial class SqliteHandler
	{
		const string existsFormat = "select name from sqlite_master where name = '{0}' and type = '{1}'";

		public async Task<int> ExecuteQueryAsync(string query)
		{
			using SqliteCommand cmd = CreateQuery(query);
			return await cmd.ExecuteNonQueryAsync();
		}
		public async Task<bool> ExistsAsync(string name, string type) 
		{
			using SqliteCommand cmd = CreateQuery(string.Format(existsFormat, name, type));
			DbDataReader reader = await cmd.ExecuteReaderAsync();
			return await reader.ReadAsync();
		}

		public int ExecuteQuery(string query)
		{
			using SqliteCommand cmd = CreateQuery(query);
			return cmd.ExecuteNonQuery();
		}

		public bool Exists(string name, string type)
		{
			using SqliteCommand cmd = CreateQuery(string.Format(existsFormat, name, type));
			using DbDataReader reader = cmd.ExecuteReader();
			return reader.Read();
		}

		private void ReadEntry(object entry, TableInfo table, DbDataReader reader)
		{
			Dictionary<string, object> values = GetColumnValues(reader);

			TableMember[] fields = table.fields;
			object v;
			for (int i = 0; i < fields.Length; i++)
			{
				TableMember field = fields[i];
				if (values.TryGetValue(field.Name, out v))
					field.SetValue(this, entry, v);
			}

			if (values.TryGetValue(table.identifier.Name, out v))
			{
				table.identifier.SetValue(this, entry, v);
			}

			if (entry is IOnDeserialized deserialized)
				deserialized.OnFinishRead();
		}
	}
}
