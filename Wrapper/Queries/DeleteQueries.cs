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

			using SqliteCommand cmd = DeleteCommand(entry);
			return cmd == null ? 0 : await cmd.ExecuteNonQueryAsync();
		}

		public int Delete<T>(ISqlTable<T> entry)
		{
			Type type = entry.GetType();
			if (!Exists(type.Name, "table")) return 0;

			using SqliteCommand cmd = DeleteCommand(entry);
			return cmd == null ? 0 : cmd.ExecuteNonQuery();
		}

		private SqliteCommand DeleteCommand<T>(ISqlTable<T> entry)
		{
			Type type = entry.GetType();
			SqliteCommand cmd = CreateQuery($"DELETE FROM {type.Name} WHERE Id=@Id");
			cmd.Parameters.Add(new SqliteParameter
			{
				ParameterName = "Id",
				Value = entry.Id
			});

			return cmd;
		}
	}
}
