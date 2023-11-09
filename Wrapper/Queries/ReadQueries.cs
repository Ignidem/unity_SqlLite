using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace SqlLite.Wrapper
{
	public partial class SqliteHandler
	{
		private static SqliteCommand ReadCommand(SqliteContext context, TableInfo table, string keyname, object key)
		{
			SqliteCommand command = context.CreateCommand(string.Format(table.select, keyname));
			command.Parameters.Add(new SqliteParameter()
			{
				ParameterName = "Id",
				Value = key
			});
			return command;
		}

		public async Task<T> ReadOneAsync<T>(string query, Action<SqliteCommand> commandFormatter)
		{
			Type type = typeof(T);
			TableInfo table = await GetTableInfoAsync(type);
			T entry = table.ConstructEmpty<T>();

			using SqliteContext context = await CreateContext().OpenAsync();
			SqliteCommand command = context.CreateCommand(query);
			commandFormatter(command);
			DbDataReader reader = await context.ReaderAsync(command);
			
			if (!await reader.ReadAsync()) return default;

			ReadEntry(entry, table, reader);
			return entry;
		}
		public async Task<T[]> ReadAllAsync<T>(string query, Action<SqliteCommand> commandFormatter = null)
		{
			Type type = typeof(T);
			TableInfo table = await GetTableInfoAsync(type);
			List<T> entries = new List<T>();

			using SqliteContext context = await CreateContext().OpenAsync();
			SqliteCommand command = context.CreateCommand(query);
			commandFormatter?.Invoke(command);
			DbDataReader reader = await context.ReaderAsync(command);

			while (await reader.ReadAsync())
			{
				T entry = table.ConstructEmpty<T>();
				ReadEntry(entry, table, reader);
				entries.Add(entry);
			}

			return entries.ToArray();
		}
		public async Task<T> ReadOneAsync<T>(object key, string keyname = "Id", bool createIfNone = false)
		{
			TableInfo table = await GetTableInfoAsync(typeof(T));
			T entry = table.ConstructEmpty<T>();

			using SqliteContext context = await CreateContext().OpenAsync();
			SqliteCommand command = ReadCommand(context, table, keyname, key);
			DbDataReader reader = await context.ReaderAsync(command);

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

			using SqliteContext context = CreateContext().Open();
			SqliteCommand command = ReadCommand(context, table, keyname, key);

			DbDataReader reader = context.Reader(command);

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
		private T ReadOne<T>(object key, string keyname, bool createIfNone, TableInfo table, T entry)
		{
			using SqliteContext context = CreateContext().Open();
			SqliteCommand command = ReadCommand(context, table, keyname, key);
			DbDataReader reader = context.Reader(command);

			if (reader.Read())
			{
				ReadEntry(entry, table, reader);
				return entry;
			}

			if (!createIfNone)
				return default;

			table.identifier.SetValue(this, entry, key);
			if (entry is ISqlTable sqlTab)
				sqlTab.Save();

			return entry;
		}
	}
}
