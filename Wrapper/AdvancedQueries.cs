using System.Data.Common;
using System.Threading.Tasks;

namespace SqlLite.Wrapper
{
	public partial class SqliteHandler
	{
		const string existsFormat = "select name from sqlite_master where name = '{0}' and type = '{1}'";

		public async Task<int> ExecuteQueryAsync(string query)
		{
			using SqliteContext context = await CreateContext().OpenAsync();
			return await context.CreateCommand(query).ExecuteNonQueryAsync();
		}
		public async Task<bool> ExistsAsync(string name, string type)
		{
			using SqliteContext context = await CreateContext().OpenAsync();
			DbDataReader reader = await context.QueryReaderAsync(string.Format(existsFormat, name, type));
			return await reader.ReadAsync();
		}

		public int ExecuteQuery(string query)
		{
			using SqliteContext context = CreateContext().Open();
			return context.CreateCommand(query).ExecuteNonQuery();
		}
		public bool Exists(string name, string type)
		{
			using SqliteContext context = CreateContext().Open();
			DbDataReader reader = context.QueryReader(string.Format(existsFormat, name, type));
			return reader.Read();
		}
	}
}
