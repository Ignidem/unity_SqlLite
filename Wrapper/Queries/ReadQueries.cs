using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace SqlLite.Wrapper
{
	public partial class SqliteHandler
	{
		public async Task<T> ReadAsync<T>(string query, Action<SqliteCommand> commandFormatter)
		{
			Type type = typeof(T);
			TableInfo table = await GetTableInfoAsync(type);
			T entry = table.ConstructEmpty<T>();

			using SqliteCommand cmd = CreateQuery(query);
			commandFormatter(cmd);
			using DbDataReader reader = await cmd.ExecuteReaderAsync();
				
			if (!await reader.ReadAsync()) return default;

			ReadEntry(entry, table, reader);
			return entry;
		}
		public async Task<T[]> ReadAllAsync<T>(string query, Action<SqliteCommand> commandFormatter = null)
		{
			Type type = typeof(T);
			TableInfo table = await GetTableInfoAsync(type);
			List<T> entries = new List<T>();

			using SqliteCommand cmd = CreateQuery(query);
			commandFormatter?.Invoke(cmd);
			using DbDataReader reader = await cmd.ExecuteReaderAsync();

			while (await reader.ReadAsync())
			{
				T entry = table.ConstructEmpty<T>();
				ReadEntry(entry, table, reader);
				entries.Add(entry);
			}

			return entries.ToArray();
		}
		public async Task<T> ReadOneAsync<T>(object id, string keyname = "Id", bool createIfNone = false)
		{
			TableInfo table = await GetTableInfoAsync(typeof(T));
			T entry = table.ConstructEmpty<T>();

			using SqliteCommand cmd = CreateQuery(string.Format(table.select, keyname));

			cmd.Parameters.Add(new SqliteParameter
			{
				ParameterName = "Id",
				Value = id
			});

			using DbDataReader reader = await cmd.ExecuteReaderAsync();

			if (!await reader.ReadAsync())
			{
				if (createIfNone && entry is ISqlTable sqlTab)
					await sqlTab.SaveAsync();
					
				return default;
			}

			ReadEntry(entry, table, reader);
			return entry;
		}

		public T[] ReadAll<T>(object key, string keyname)
		{
			TableInfo table = GetTableInfo(typeof(T));
			List<T> entries = new List<T>();

			string query = $"select * from {table.name} where {keyname}=@Id";
			using SqliteCommand cmd = CreateQuery(query);
			cmd.Parameters.Add(new SqliteParameter()
			{
				ParameterName = "Id",
				Value = key
			});

			using DbDataReader reader = cmd.ExecuteReader();

			while (reader.Read())
			{
				T entry = table.ConstructEmpty<T>();
				ReadEntry(entry, table, reader);
				entries.Add(entry);
			}

			return entries.ToArray();
		}
		public T ReadOne<T>(object id, string keyname = "Id", bool createIfNone = false)
		{
			TableInfo table = GetTableInfo(typeof(T));
			T entry = table.ConstructEmpty<T>();

			return ReadOne(id, keyname, createIfNone, table, entry);
		}
		public object ReadOne(Type type, object id, string keyname, bool createIfNone = false)
		{
			TableInfo table = GetTableInfo(type);
			object entry = table.ConstructEmpty<object>();

			return ReadOne(id, keyname, createIfNone, table, entry);
		}
		private T ReadOne<T>(object id, string keyname, bool createIfNone, TableInfo table, T entry)
		{
			using var cmd = CreateQuery(string.Format(table.select, keyname));
			cmd.Parameters.Add(new SqliteParameter
			{
				ParameterName = "Id",
				Value = id
			});

			using var reader = cmd.ExecuteReader();
			if (reader.Read())
			{
				ReadEntry(entry, table, reader);
				return entry;
			}

			if (!createIfNone)
				return default;
				
			if (entry is ISqlTable sqlTab)
				sqlTab.Save();

			return entry;
		}
	}
}
