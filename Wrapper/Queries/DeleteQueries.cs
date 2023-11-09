using Mono.Data.Sqlite;
using System;
using System.Threading.Tasks;

namespace SqlLite.Wrapper
{
	public partial class SqliteHandler
	{
		public async Task<int> DeleteAsync<T>(ISqlTable<T> entry)
		{
			Type type = entry.GetType();
			if (!await ExistsAsync(type.Name, "table")) return 0;

			using SqliteContext context = await CreateContext().OpenAsync();
			return await DeleteCommand(context, entry).ExecuteNonQueryAsync();
		}

		public int Delete<T>(ISqlTable<T> entry)
		{
			Type type = entry.GetType();
			if (!Exists(type.Name, "table")) return 0;

			using SqliteContext context = CreateContext().Open();
			return DeleteCommand(context, entry).ExecuteNonQuery();
		}

		private SqliteCommand DeleteCommand<T>(SqliteContext context, ISqlTable<T> entry)
		{
			Type type = entry.GetType();
			SqliteCommand command = context.CreateCommand($"DELETE FROM {type.Name} WHERE Id=@Id");
			command.Parameters.Add(new SqliteParameter
			{
				ParameterName = "Id",
				Value = entry.Id
			});

			return command;
		}
	}
}
