using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace SqlLite.Wrapper
{
	public partial class SqliteHandler
	{
		public async Task<int> SaveManyAsync<T, I>(IEnumerable<T> entries, Action<T> onSaved = null)
			where T : ISqlTable<I>
		{
			using SqliteContext context = await CreateContext().OpenAsync();
			object _target = null;
			try
			{
				Type type = typeof(T);
				TableInfo table = await GetTableInfoAsync(type);

				int? aId = table.idAutoIncr == null ? null : await table.idAutoIncr.GetNextIndexAsync(this);

				SqliteCommand command = context.CreateCommand(table.save);
				command.Parameters.AddWithValue(table.identifier.Name, null);
				TableMember[] fields = table.fields;
				for (int i = 0; i < fields.Length; i++)
					command.Parameters.AddWithValue(fields[i].Name, null);

				int ops = 0;
				foreach (T entry in entries)
				{
					_target = entry;
					I id = entry.Id;

					if (aId is not null)
					{
						id = entry.Id = (I)(object)aId;
						aId++;
					}

					command.Parameters[table.identifier.Name].Value = id;

					for (int i = 0; i < fields.Length; i++)
					{
						TableMember field = fields[i];
						object v = await field.GetValueAsync(this, entry);
						command.Parameters[field.Name].Value = v;
					}

					int operations = await command.ExecuteNonQueryAsync();
					OnCommandExecuted(command, operations, entry);
					ops += operations;
					onSaved?.Invoke(entry);
				}

				if (table.idAutoIncr != null)
					await table.idAutoIncr.SetNextIndexAsync(this, aId.Value);

				return ops;

			}
			catch (Exception e)	
			{
				OnException(e, context, _target);
				throw e;
			}
		}

		public async Task<int> SaveAsync<T>(ISqlTable<T> entry)
		{
			using SqliteContext context = await CreateContext().OpenAsync();
			object _target = entry;
			try
			{
				Type type = entry.GetType();
				TableInfo table = await GetTableInfoAsync(type);

				if (table.idAutoIncr != null && entry.Id.Equals(default))
					await table.identifier.SetValueAsync(this, entry, await table.idAutoIncr.GetNextIndexAsync(this));

				SqliteCommand command = context.CreateCommand(table.save);
				command.Parameters.AddWithValue(table.identifier.Name, entry.Id);

				TableMember[] fields = table.fields;

				for (int i = 0; i < fields.Length; i++)
					command.Parameters.Add(await fields[i].GetParameterAsync(this, entry));

				int ops = await command.ExecuteNonQueryAsync();
				OnCommandExecuted(command, ops, _target);
				return ops;

			}
			catch (Exception e)
			{
				OnException(e, context, _target);
				throw e;
			}
		}

		public int Save<T>(ISqlTable<T> entry)
		{
			using SqliteContext context = CreateContext().Open();
			object _target = entry;
			try
			{
				Type type = entry.GetType();
				TableInfo table = GetTableInfo(type);

				if (table.idAutoIncr != null && entry.Id.Equals(default))
					table.identifier.SetValue(this, entry, table.idAutoIncr.GetNextIndex(this));

				SqliteCommand command = context.CreateCommand(table.save);
				command.Parameters.AddWithValue(table.identifier.Name, entry.Id);

				TableMember[] fields = table.fields;

				for (int i = 0; i < fields.Length; i++)
					command.Parameters.Add(fields[i].GetParameter(this, entry));

				int ops = command.ExecuteNonQuery();
				OnCommandExecuted(command, ops, _target);
				return ops;
			}
			catch (Exception e)
			{
				OnException(e, context, _target);
				throw e;
			}
		}
	}
}
