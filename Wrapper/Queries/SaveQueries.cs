using Mono.Data.Sqlite;
using System;
using System.Threading.Tasks;

namespace SqlLite.Wrapper
{

	public partial class SqliteHandler
	{
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
