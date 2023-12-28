using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace SqlLite.Wrapper
{
	public partial class SqliteHandler
	{
		private static readonly Dictionary<Type, TableInfo> TableCache = new Dictionary<Type, TableInfo>();

		private async Task<TableInfo> GetTableInfoAsync(Type type)
		{
			if (TableCache.TryGetValue(type, out TableInfo table))
				return table;

			table = new TableInfo(type);
			await table.VerifyTable(this);

			TableCache.Add(type, table);
			return table;
		}

		private TableInfo GetTableInfo(Type type)
		{
			if (TableCache.TryGetValue(type, out TableInfo table))
				return table;

			table = new TableInfo(type);
			table.VerifyTableSync(this);

			TableCache.Add(type, table);
			return table;
		}

		private static void RebuildTable(TableInfo table)
		{
			List<Dictionary<string, object>> entries = new List<Dictionary<string, object>>();

			using (SqliteConnection connection = new SqliteConnection(defaultPath))
			{
				connection.Open();
				using (SqliteCommand cmd = connection.CreateCommand())
				{
					cmd.CommandType = System.Data.CommandType.Text;
					cmd.CommandText = $"select * from {table.name}";
					using (SqliteDataReader reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							Dictionary<string, object> entry = GetColumnValues(reader);
							entries.Add(entry);
						}
					}

					cmd.CommandText = $"drop table {table.name}";
					cmd.ExecuteNonQuery();

					cmd.CommandText = table.create;
					cmd.ExecuteNonQuery();

					if (entries.Count == 0) return;

					cmd.CommandText = table.save;

					for (int i = 0; i < entries.Count; i++)
					{
						Dictionary<string, object> entry = entries[i];

						cmd.Parameters.Add(new SqliteParameter
						{
							ParameterName = "Id",
							Value = entry[table.identifier.Name]
						});

						TableMember[] fields = table.fields;

						for (int j = 0; j < fields.Length; j++)
						{
							TableMember field = fields[j];
							Type t = field.FieldType;

							if (!entry.TryGetValue(field.Name, out object v)) v = GetDefault(t);
							else if (!v.GetType().Equals(t))
							{
								try { v = Convert.ChangeType(v, t); }
								catch (Exception) { v = GetDefault(t); }
							}

							cmd.Parameters.Add(new SqliteParameter
							{
								ParameterName = field.Name,
								Value = v
							});
						}

						var result = cmd.ExecuteNonQuery();
					}
				}
			}
		}

		private static object GetDefault(Type t)
			=> t.IsValueType ? Activator.CreateInstance(t) : null;

		private static string JoinFields(string seperator, TableMember[] fields, Func<TableMember, string> parse)
		{
			string str = null;
			for (int i = 0; i < fields.Length; i++)
			{
				TableMember field = fields[i];
				str += parse(field);
				if (i + 1 < fields.Length) str += seperator;
			}
			return str;
		}

		private static string SqlType(Type type)
		{
			if (type == typeof(string) || type == typeof(char))
				return "TEXT";

			if (type == typeof(short) || type == typeof(int) || type == typeof(long))
				return "INT";

			if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
				return "REAL";

			if (type == typeof(decimal))
				return "NUM";

			if (type == typeof(Guid))
				return "BLOB";

			throw new Exception($"{type.Name} is not a supported sqlite type.");
		}

		private static Dictionary<string, object> GetColumnValues(DbDataReader reader)
		{
			Dictionary<string, object> values = new Dictionary<string, object>();
			for (int i = 0; i < reader.FieldCount; i++)
			{
				values.Add(reader.GetName(i), reader.GetValue(i));
			}

			return values;
		}
	}
}
