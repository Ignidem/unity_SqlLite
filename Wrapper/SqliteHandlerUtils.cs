using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace SqlLite.Wrapper
{
	public static partial class SqliteHandler
	{
		private static readonly Dictionary<Type, TableInfo> TableCache = new Dictionary<Type, TableInfo>();

		private static TableInfo GetTableInfo(Type type)
		{
			if (TableCache.TryGetValue(type, out TableInfo table))
				return table;

			table = new TableInfo(type);

			TableCache.Add(type, table);
			return table;
		}

		private static bool TableExists(string name)
		{
			bool exists = false;
			CreateQuery($"SELECT 1 FROM sqlite_master WHERE type='table' AND name='{name}'", cmd =>
			{
				cmd.ExecuteNonQuery();
				using (SqliteDataReader reader = cmd.ExecuteReader())
				{
					exists = reader.Read();
				}
			});
			return exists;
		}

		private static void CreateTable(TableInfo table)
		{
			(bool create, bool rebuild) = VerifyTable(table);

			if (rebuild)
			{
				RebuildTable(table);
				return;
			}

			if (create) CreateQuery(table.create, cmd => cmd.ExecuteNonQuery());
		}

		private static void RebuildTable(TableInfo table)
		{
			List<Dictionary<string, object>> entries = new List<Dictionary<string, object>>();

			using (SqliteConnection connection = new SqliteConnection(dbPath))
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
							Type t = field.ValueType;

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

		private static (bool, bool) VerifyTable(TableInfo table)
		{
			bool create = false;
			bool rebuild = false;
			CreateQuery($"PRAGMA table_info({table.name})", cmd =>
			{
				cmd.ExecuteNonQuery();
				using (SqliteDataReader reader = cmd.ExecuteReader())
				{
					if (!reader.Read())
					{
						create = true;
						return;
					}

					Dictionary<string, Dictionary<string, object>> data = new Dictionary<string, Dictionary<string, object>>();
					do
					{
						Dictionary<string, object> values = GetColumnValues(reader);
						data.Add(values["name"].ToString(), values);
					}
					while (reader.Read());

					if (data.TryGetValue(table.identifier.Name, out Dictionary<string, object> idf))
					{
						string type = idf["type"].ToString();
						if (type.Equals(SqlType(table.identifier.ValueType), StringComparison.OrdinalIgnoreCase))
							data.Remove(table.identifier.Name);
						else rebuild = true;
					}
					else rebuild = true;

					for (int i = 0; !rebuild && i < table.fields.Length; i++)
					{
						TableMember mem = table.fields[i];

						if (data.TryGetValue(mem.Name, out Dictionary<string, object> f))
						{
							string type = f["type"].ToString();
							if (type.Equals(SqlType(mem.ValueType), StringComparison.OrdinalIgnoreCase))
								data.Remove(mem.Name);
							else rebuild = true;
						}
						else rebuild = true;
					}

					if (data.Count > 0) //There are extra columns
						rebuild = true;
				}
			});

			return (create, rebuild);
		}

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

			return "TEXT";
		}

		private static void CreateQuery(string query, Action<SqliteCommand> action)
		{
			UseConnection(connection =>
			{
#pragma warning disable IDE0063 // Use simple 'using' statement
				using (SqliteCommand cmd = connection.CreateCommand())
#pragma warning restore IDE0063 // Use simple 'using' statement
				{
					cmd.CommandType = System.Data.CommandType.Text;
					cmd.CommandText = query;
					action(cmd);
				}
			});
		}

		private static void UseConnection(Action<SqliteConnection> action)
		{
#pragma warning disable IDE0063 // Use simple 'using' statement
			using (SqliteConnection connection = new SqliteConnection(dbPath))
#pragma warning restore IDE0063 // Use simple 'using' statement
			{
				connection.Open();
				action(connection);
			}
		}

		private static Dictionary<string, object> GetColumnValues(SqliteDataReader reader)
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
