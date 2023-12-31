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
			SqliteCommand command = context.CreateCommand(string.Format(table.SelectQuery, keyname));
			command.Parameters.Add(new SqliteParameter()
			{
				ParameterName = "Id",
				Value = key
			});
			return command;
		}

		public async Task<T> ReadOneAsync<T>(string query, Action<SqliteCommand> commandFormatter)
		{
			using SqliteContext context = await CreateContext().OpenAsync(); 
			object _target = null;
			try
			{
				Type type = typeof(T);
				TableInfo table = await GetTableInfoAsync(type);
				T entry = table.ConstructEmpty<T>();
				_target = entry;
				SqliteCommand command = context.CreateCommand(query);
				commandFormatter(command);
				DbDataReader reader = await context.ReaderAsync(command);

				if (!await reader.ReadAsync()) return default;

				await ReadEntryAsync(entry, table, reader);
				OnCommandExecuted(command, 1, entry);
				return entry;
			}
			catch (Exception e)
			{
				OnException(e, context, _target);
				throw e;
			}
		}
		public async Task<T[]> ReadAllAsync<T>(string query, Action<SqliteCommand> commandFormatter = null)
		{
			using SqliteContext context = await CreateContext().OpenAsync(); 
			object _target = null;
			try
			{
				Type type = typeof(T);
				TableInfo table = await GetTableInfoAsync(type);
				List<T> entries = new List<T>();

				SqliteCommand command = context.CreateCommand(query);
				commandFormatter?.Invoke(command);
				DbDataReader reader = await context.ReaderAsync(command);

				while (await reader.ReadAsync())
				{
					T entry = table.ConstructEmpty<T>();
					_target = entry;
					await ReadEntryAsync(entry, table, reader);
					entries.Add(entry);
					OnCommandExecuted(command, 0, entry);
				}

				return entries.ToArray();
			}
			catch (Exception e)
			{
				OnException(e, context, _target);
				throw e;
			}
		}

		public async Task<T[]> ReadAllAsync<T>(object key, string keyname = "Id")
		{
			using SqliteContext context = await CreateContext().OpenAsync();
			object _target = null;
			try
			{
				Type type = typeof(T);
				TableInfo table = await GetTableInfoAsync(type);
				List<T> entries = new List<T>();

				SqliteCommand command = ReadCommand(context, table, keyname, key);
				DbDataReader reader = await context.ReaderAsync(command);

				while (await reader.ReadAsync())
				{
					T entry = table.ConstructEmpty<T>();
					_target = entry;
					await ReadEntryAsync(entry, table, reader);
					entries.Add(entry);
					OnCommandExecuted(command, 1, entry);
				}

				return entries.ToArray();
			}
			catch (Exception e)
			{
				OnException(e, context, _target);
				throw e;
			}
		}
		public async Task<T[]> ReadAllAsync<T>(string keyname = "Id", params object[] ids)
		{
			using SqliteContext context = await CreateContext().OpenAsync();
			object _target = null;
			try
			{
				Type type = typeof(T);
				TableInfo table = await GetTableInfoAsync(type);
				List<T> entries = new List<T>();
				const string selectIn = "SELECT * FROM {0} WHERE {1} IN ({3})";

				string idsParams = null;
				SqliteCommand command = context.CreateCommand(null);
				for(int i = 0; i < ids.Length; i++)
				{
					object key = ids[i];
					string paramName = $"@Key{i}d";
					idsParams += paramName;
					if (i + 1 < ids.Length)
						idsParams += ',';

					command.Parameters.AddWithValue(paramName, key);
				}

				command.CommandText = string.Format(selectIn, table.name, keyname, idsParams);
				DbDataReader reader = await context.ReaderAsync(command);

				while (await reader.ReadAsync())
				{
					T entry = table.ConstructEmpty<T>();
					_target = entry;
					await ReadEntryAsync(entry, table, reader);
					entries.Add(entry);
					OnCommandExecuted(command, 1, entry);
				}

				return entries.ToArray();
			}
			catch (Exception e)
			{
				OnException(e, context, _target);
				throw e;
			}
		}

		public async Task<T> ReadOneAsync<T>(object key, string keyname = "Id", bool createIfNone = false)
		{
			using SqliteContext context = await CreateContext().OpenAsync();
			object _target = null;
			try
			{
				TableInfo table = await GetTableInfoAsync(typeof(T));
				T entry = table.ConstructEmpty<T>();
				_target = entry;
				SqliteCommand command = ReadCommand(context, table, keyname, key);
				DbDataReader reader = await context.ReaderAsync(command);

				if (!await reader.ReadAsync())
				{
					if (createIfNone && entry is ISqlTable sqlTab)
						await sqlTab.SaveAsync();

					return default;
				}

 				await ReadEntryAsync(entry, table, reader);
				OnCommandExecuted(command, 1, entry);
				return entry;
			}
			catch (Exception e)
			{
				OnException(e, context, _target);
				throw e;
			}
		}
		public async Task<object> ReadOneAsync(Type type, object key, string keyname, bool createIfNone = false)
		{
			using SqliteContext context = await CreateContext().OpenAsync();
			object _target = null;
			try
			{
				TableInfo table = await GetTableInfoAsync(type);
				object entry = _target = table.ConstructEmpty<object>();

				SqliteCommand command = ReadCommand(context, table, keyname, key);
				DbDataReader reader = await context.ReaderAsync(command);

				if (!await reader.ReadAsync())
				{
					if (createIfNone && entry is ISqlTable sqlTab)
						await sqlTab.SaveAsync();

					return default;
				}

				await ReadEntryAsync(entry, table, reader);
				OnCommandExecuted(command, 1, entry);
				return entry;
			}
			catch (Exception e)
			{
				OnException(e, context, _target);
				throw e;
			}
		}

		public T[] ReadAll<T>(object key, string keyname)
		{
			using SqliteContext context = CreateContext().Open();
			object _target = null;
			try
			{
				TableInfo table = GetTableInfo(typeof(T));
				List<T> entries = new List<T>();

				SqliteCommand command = ReadCommand(context, table, keyname, key);
				DbDataReader reader = context.Reader(command);

				while (reader.Read())
				{
					T entry = table.ConstructEmpty<T>();
					_target = entry;
					ReadEntry(entry, table, reader);
					entries.Add(entry);
					OnCommandExecuted(command, 1, entry);
				}

				return entries.ToArray();
			}
			catch (Exception e)
			{
				OnException(e, context, _target);
				throw e;
			}
		}

		public T[] ReadAll<T>(string query, Action<SqliteCommand> commandFormatter = null)
		{
			using SqliteContext context = CreateContext().Open();
			object _target = null;
			try
			{
				Type type = typeof(T);
				TableInfo table = GetTableInfo(type);
				List<T> entries = new List<T>();

				SqliteCommand command = context.CreateCommand(query);
				commandFormatter?.Invoke(command);
				DbDataReader reader = context.Reader(command);

				while (reader.Read())
				{
					T entry = table.ConstructEmpty<T>();
					_target = entry;
					ReadEntry(entry, table, reader);
					entries.Add(entry);
					OnCommandExecuted(command, 0, entry);
				}

				return entries.ToArray();
			}
			catch (Exception e)
			{
				OnException(e, context, _target);
				throw e;
			}
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
			try
			{
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
				OnCommandExecuted(command, 1, entry);
				return entry;
			}
			catch (Exception e)
			{
				OnException(e, context, entry);
				throw e;
			}
		}
	}
}
