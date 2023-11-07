using Mono.Data.Sqlite;
using SqlLite.Wrapper.QueryExtensions;
using SqlLite.Wrapper.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SqlLite.Wrapper
{
	public static partial class SqliteHandler
	{
		public static string DBPath
		{
			get => dbPath;
			set
			{
				dbPath = "URI=file:" + Application.persistentDataPath + '/' + value;
			}
		}
		private static string dbPath = "URI=file:" + Application.persistentDataPath + "/ql_dtbs.db";

		public static void SaveEntity<I>(object entity, I id)
		{
			Type type = entity.GetType();

			TableInfo table = GetTableInfo(type);

			if (table.idAutoIncr != null)
				id = table.AutoIncrementIndex(entity, id);

			CreateQuery(table.save, cmd =>
			{
				cmd.Parameters.Add(new SqliteParameter
				{
					ParameterName = table.identifier.Name,
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

				int result = cmd.ExecuteNonQuery();
			});
		}

		public static T LoadOne<T, I>(I id, string keyName, bool createIfnone)
		{
			return LoadOne<T>(typeof(T), id, keyName, createIfnone);
		}

		public static T[] LoadAll<T, K>(K id, string keyName, params Condition[] conditions)
		{
			Type type = typeof(T);
			TableInfo table = GetTableInfo(type);
			List<T> entries = new List<T>();

			StringBuilder sb = new StringBuilder(string.Format(table.select, keyName));

			for (int i = 0; i < conditions.Length; i++)
			{
				Condition cnd = conditions[i];
				sb.AppendFormat(" AND {0}", cnd.ToSQL());
			}

			CreateQuery(sb.ToString(), cmd =>
			{
				cmd.Parameters.Add(new SqliteParameter
				{
					ParameterName = "Id",
					Value = id
				});

				for (int i = 0; i < conditions.Length; i++)
				{
					Condition cnd = conditions[i];

					cmd.Parameters.Add(new SqliteParameter
					{
						ParameterName = cnd.valueParameterName,
						Value = cnd.value
					});
				}

#pragma warning disable IDE0063 // Use simple 'using' statement
				using (SqliteDataReader reader = cmd.ExecuteReader())
#pragma warning restore IDE0063 // Use simple 'using' statement
				{
					while (reader.Read())
					{
						T entry = table.ConstructEmpty<T>();
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

		public static void DeleteEntity<I>(Type type, I id)
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

		private static T LoadOne<T>(Type type, object id, string keyName, bool createIfnone)
		{
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
				table.identifier.SetValue(entry, id);
				SaveEntity(entry, id);
				return entry;
			}

			return found ? entry : default;
		}

		private static void ReadEntry(object entry, TableInfo table, SqliteDataReader reader)
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

			if (entry is IOnDeserialized deserialized)
				deserialized.OnFinishRead();
		}
	}
}
