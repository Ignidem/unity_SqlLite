using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace SqlLite.Wrapper
{

	public partial class SqliteHandler
	{
		public async Task<int> SaveManyAsync<T, I>(IEnumerable<T> entries)
			where T : ISqlTable<I>
		{
			Type type = typeof(T);
			TableInfo table = await GetTableInfoAsync(type);
			using SqliteContext context = await CreateContext().OpenAsync();

			int? aId = table.idAutoIncr?.GetNextIndex(this);

			SqliteCommand command = context.CreateCommand(table.save);
			command.Parameters.AddWithValue(table.identifier.Name, null);
			TableMember[] fields = table.fields;
			for (int i = 0; i < fields.Length; i++)
				command.Parameters.AddWithValue(fields[i].Name, null);

			int ops = 0;
			foreach(var entry in entries)
			{
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
					command.Parameters[field.Name].Value = field.GetValue(this, entry);
				}

				ops += await command.ExecuteNonQueryAsync();
			}

			return ops;
		}

		public async Task<int> SaveAsync<T>(ISqlTable<T> entry)
		{
			Type type = entry.GetType();
			TableInfo table = await GetTableInfoAsync(type);
			using SqliteContext context = await CreateContext().OpenAsync();
			SqliteCommand cmd = SaveCommand(context, table, entry);
			return await cmd.ExecuteNonQueryAsync();
		}

		public int Save<T>(ISqlTable<T> entry)
		{
			Type type = entry.GetType();
			TableInfo table = GetTableInfo(type); 
			using SqliteContext context = CreateContext().Open();
			using SqliteCommand cmd = SaveCommand(context, table, entry);
			return cmd.ExecuteNonQuery();
		}

		private SqliteCommand SaveCommand<T>(SqliteContext context, TableInfo table, ISqlTable<T> entry)
		{
			if (table.idAutoIncr != null)
				entry.Id = table.AutoIncrementIndex(entry, entry.Id);

			SqliteCommand command = context.CreateCommand(table.save);
			command.Parameters.Add(new SqliteParameter
			{
				ParameterName = table.identifier.Name,
				Value = entry.Id
			});

			TableMember[] fields = table.fields;

			for (int i = 0; i < fields.Length; i++)
			{
				TableMember field = fields[i];

				command.Parameters.Add(new SqliteParameter
				{
					ParameterName = field.Name,
					Value = field.GetValue(this, entry)
				});
			}

			return command;
		}
	}
}
