using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SqlLite.Wrapper
{
	public static partial class SqliteHandler
	{
		private static readonly string dbPath = "URI=file:" + Application.persistentDataPath + "/ql_dtbs.db";

		public static void SaveEntity<I>(object entity, I id)
		{
			Type type = entity.GetType();

			TableInfo table = GetTableInfo(type);

			CreateQuery(table.save, cmd =>
			{
				cmd.Parameters.Add(new SqliteParameter
				{
					ParameterName = "Id",
					Value = id
				});

				TableMember[] fields = table.fields;

				for (int i = 0; i < fields.Length; i++)
				{
					TableMember field = fields[i];

					cmd.Parameters.Add(new SqliteParameter
					{
						ParameterName = field.Name,
						Value = field.GetValue(entity)
					});
				}

				var result = cmd.ExecuteNonQuery();
			});
		}

		public static T LoadOne<T, I>(I id, string keyName, bool createIfnone)
		{
			Type type = typeof(T);
			bool found = false;

			TableInfo table = GetTableInfo(type);
			T entry = table.ConstructEmpty<T>();

			CreateQuery(string.Format(table.select, keyName), cmd =>
			{
				cmd.Parameters.Add(new SqliteParameter
				{
					ParameterName = "Id",
					Value = id
				});

#pragma warning disable IDE0063 // Use simple 'using' statement
				using (SqliteDataReader reader = cmd.ExecuteReader())
#pragma warning restore IDE0063 // Use simple 'using' statement
				{
					if (!reader.Read()) return;

					found = true;
					ReadEntry(entry, table, reader);
				}
			});

			if (!found && createIfnone)
			{
				if (entry is ISqlTable<I> ken)
					ken.Id = id;
				
				SaveEntity(entry, id);
				return entry;
			}

			return found ? entry : default;
		}

		private static T[] LoadAll<T, K>(K id, string keyName) where T : class, ITable, new()
		{
			Type type = typeof(T);
			TableInfo table = GetTableInfo(type);
			List<T> entries = new List<T>();
			CreateQuery(string.Format(table.select, keyName), cmd =>
			{
				cmd.Parameters.Add(new SqliteParameter
				{
					ParameterName = "Id",
					Value = id
				});

#pragma warning disable IDE0063 // Use simple 'using' statement
				using (SqliteDataReader reader = cmd.ExecuteReader())
#pragma warning restore IDE0063 // Use simple 'using' statement
				{
					while (reader.Read())
					{
						T entry = new T();
						ReadEntry(entry, table, reader);
						entries.Add(entry);
					}
				}
			});

			return entries.ToArray();
		}

		public static void DeleteEntity<T, I>(I id)
		{
			Type type = typeof(T);
			DeleteEntity(type, id);
		}

		private static void DeleteEntity<I>(Type type, I id)
		{
			if (!TableExists(type.Name)) return;
			CreateQuery($"DELETE FROM {type.Name} WHERE Id=@Id", cmd =>
			{
				cmd.Parameters.Add(new SqliteParameter
				{
					ParameterName = "Id",
					Value = id
				});

				cmd.ExecuteNonQuery();
			});
		}

		private static void DeleteAll<T, K>(K id, string keyName)
		{
			Type type = typeof(T);
			if (!TableExists(type.Name)) return;
			CreateQuery($"DELETE FROM {type.Name} WHERE {keyName}=@id", cmd =>
			{
				cmd.Parameters.Add(new SqliteParameter
				{
					ParameterName = "Id",
					Value = id
				});

				cmd.ExecuteNonQuery();
			});
		}

		private static void ReadEntry<T>(T entry, TableInfo table, SqliteDataReader reader)
		{
			Dictionary<string, object> values = GetColumnValues(reader);

			TableMember[] fields = table.fields;
			object v;
			for (int i = 0; i < fields.Length; i++)
			{
				TableMember field = fields[i];
				if (values.TryGetValue(field.Name, out v))
					field.SetValue(entry, v);
			}

			if (values.TryGetValue(table.identifier.Name, out v))
			{
				table.identifier.SetValue(entry, v);
			}
		}
	}
}
