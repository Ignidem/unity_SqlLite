using Mono.Data.Sqlite;
using System;
using System.Threading.Tasks;

namespace SqlLite.Wrapper
{
	public partial class SqliteHandler
	{
		public async Task<int> DeleteAsync<T>(ISqlTable<T> entry)
		{
			using SqliteContext context = await CreateContext().OpenAsync();
			object _target = entry;
			try
			{
				Type type = entry.GetType();
				if (!await ExistsAsync(type.Name, "table")) return 0;

				SqliteCommand command = DeleteCommand(context, entry);
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

		public int Delete<T>(ISqlTable<T> entry)
		{
			using SqliteContext context = CreateContext().Open();
			object _target = null;
			try
			{
				Type type = entry.GetType();
				if (!Exists(type.Name, "table")) return 0;

				SqliteCommand command = DeleteCommand(context, entry);
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
