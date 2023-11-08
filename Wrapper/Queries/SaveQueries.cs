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
			using SqliteCommand cmd = SaveCommand(table, entry);
			return await cmd.ExecuteNonQueryAsync();
		}

		public int Save<T>(ISqlTable<T> entry)
		{
			Type type = entry.GetType();
			TableInfo table = GetTableInfo(type);
			using SqliteCommand cmd = SaveCommand(table, entry);
			return cmd.ExecuteNonQuery();
		}

		private SqliteCommand SaveCommand<T>(TableInfo table, ISqlTable<T> entry)
		{
			if (table.idAutoIncr != null)
				entry.Id = table.AutoIncrementIndex(entry, entry.Id);

			SqliteCommand cmd = CreateQuery(table.save);

			cmd.Parameters.Add(new SqliteParameter
			{
				ParameterName = table.identifier.Name,
				Value = entry.Id
			});

			TableMember[] fields = table.fields;

			for (int i = 0; i < fields.Length; i++)
			{
				TableMember field = fields[i];

				cmd.Parameters.Add(new SqliteParameter
				{
					ParameterName = field.Name,
					Value = field.GetValue(this, entry)
				});
			}

			return cmd;
		}
	}
}
