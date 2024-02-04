using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SqlLite.Wrapper
{
	public class MockSqliteHandler : ISqliteHandler
	{
#pragma warning disable 67
		public event CommandExecutedDelegate OnCommandExecuted;
		public event ExceptionDelegate<Exception> OnException;
#pragma warning restore 67

		public int Delete<T>(ISqlTable<T> entry)
		{
			Debug.Log("Delete " + entry);
			return 0;
		}

		public Task<int> DeleteAsync<T>(ISqlTable<T> entry)
		{
			Debug.Log("Delete " + entry);
			return Task.FromResult(0);
		}

		public void Dispose() { }

		public Task<int> ExecuteQueryAsync(string query)
		{
			Debug.Log(query);
			return Task.FromResult(0);
		}

		public T[] ReadAll<T>(object key, string keyname)
		{
			Debug.Log("Read All " + typeof(T).Name);
			return new T[0];
		}

		public T[] ReadAll<T>(string query, Action<SqliteCommand> commandFormatter = null)
		{
			Debug.Log("Read All " + typeof(T).Name);
			return new T[0];
		}

		public Task<T[]> ReadAllAsync<T>(object key, string keyname = "Id")
		{
			Debug.Log("Read All " + typeof(T).Name);
			return Task.FromResult(new T[0]);
		}

		public Task<T[]> ReadAllAsync<T>(string query, Action<SqliteCommand> commandFormatter = null)
		{
			Debug.Log("Read All " + typeof(T).Name);
			return Task.FromResult(new T[0]);
		}

		public object ReadOne(Type type, object id, string keyname, bool createIfNone = false)
		{
			Debug.Log("Read " + type.Name + " " + id);
			return default;
		}

		public T ReadOne<T>(object id, string keyname = "Id", bool createIfNone = false)
		{
			Debug.Log("Read " + typeof(T).Name + " " + id);
			return default;
		}

		public Task<T> ReadOneAsync<T>(object key, string keyname = "Id", bool createIfNone = false)
		{
			Debug.Log("Read One " + key);
			return Task.FromResult(default(T));
		}

		public Task<object> ReadOneAsync(Type type, object key, string keyname, bool createIfNone = false)
		{
			Debug.Log("Read One " + type.Name);
			return Task.FromResult<object>(null);
		}

		public int Save<T>(ISqlTable<T> entry)
		{
			Debug.Log("Save " + entry);
			return 0;
		}

		public Task<int> SaveAsync<T>(ISqlTable<T> entry)
		{
			Debug.Log("Save " + entry);
			return Task.FromResult(0);
		}

		public int SaveMany<T, I>(IEnumerable<T> entries, Action<T, int> onIterate = null) where T : ISqlTable<I>
		{
			Debug.Log("Save Many " + typeof(T).Name);
			return 0;
		}

		public Task<int> SaveManyAsync<T, I>(IEnumerable<T> entries, Action<T, int> onIterate = null) where T : ISqlTable<I>
		{
			Debug.Log("Save Many " + typeof(T).Name);
			return Task.FromResult(0);
		}

		public Task VerifyDependecies(Type type)
		{
			return Task.CompletedTask;
		}
	}
}
