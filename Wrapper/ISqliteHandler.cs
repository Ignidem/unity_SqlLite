using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SqlLite.Wrapper
{
	public interface ISqliteHandler : IDisposable
	{
		event CommandExecutedDelegate OnCommandExecuted;
		event ExceptionDelegate<Exception> OnException;

		Task VerifyDependecies(Type type);

		Task<int> ExecuteQueryAsync(string query);

		#region Save
		Task<int> SaveAsync<T>(ISqlTable<T> entry);
		int Save<T>(ISqlTable<T> entry);
		int SaveMany<T, I>(IEnumerable<T> entries, Action<T, int> onIterate = null) where T : ISqlTable<I>;
		Task<int> SaveManyAsync<T, I>(IEnumerable<T> entries, Action<T, int> onIterate = null) where T : ISqlTable<I>;
		#endregion

		#region Read
		object ReadOne(Type type, object id, string keyname, bool createIfNone = false);
		T ReadOne<T>(object id, string keyname = "Id", bool createIfNone = false);
		Task<T> ReadOneAsync<T>(object key, string keyname = "Id", bool createIfNone = false);
		Task<object> ReadOneAsync(Type type, object key, string keyname, bool createIfNone = false);
		T[] ReadAll<T>(object key, string keyname);
		Task<T[]> ReadAllAsync<T>(object key, string keyname = "Id");
		Task<T[]> ReadAllAsync<T>(string query, Action<SqliteCommand> commandFormatter = null);
		T[] ReadAll<T>(string query, Action<SqliteCommand> commandFormatter = null);
		#endregion

		#region Delete
		Task<int> DeleteAsync<T>(ISqlTable<T> entry);
		int Delete<T>(ISqlTable<T> entry);
		#endregion
	}
}
