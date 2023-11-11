using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace SqlLite.Wrapper
{
	public class SqliteContext : IDisposable, IAsyncDisposable
	{
		public readonly SqliteConnection connection;
		private readonly List<IDisposable> modules = new();
		private bool disposallocked;

		public SqliteContext(SqliteConnection connection)
		{
			this.connection = connection;
		}

		public T AddModule<T>(T module)
			where T : IDisposable
		{
			modules.Add(module);
			return module;
		}

		public T GetModule<T>(Predicate<T> search = null)
		{
			return (T)modules.Find(m => m is T _t && (search?.Invoke(_t) ?? true));
		}

		#region Connection
		public SqliteContext Open()
		{
			if (connection.State is System.Data.ConnectionState.Closed)
				connection.Open();
			return this;
		}

		public async Task<SqliteContext> OpenAsync()
		{
			if (connection.State is System.Data.ConnectionState.Closed)
				await connection.OpenAsync();
			return this;
		}
		#endregion

		#region Command

		public SqliteCommand CreateCommand(string query)
		{
			SqliteCommand cmd = AddModule(connection.CreateCommand());
			cmd.CommandType = System.Data.CommandType.Text;
			cmd.CommandText = query;
			return cmd;
		}

		public DbDataReader Reader(SqliteCommand cmd) => AddModule(cmd.ExecuteReader());
		public async Task<DbDataReader> ReaderAsync(SqliteCommand cmd) => AddModule(await cmd.ExecuteReaderAsync());

		public DbDataReader QueryReader(string query) => Reader(CreateCommand(query));
		public async Task<DbDataReader> QueryReaderAsync(string query) => await ReaderAsync(CreateCommand(query));

		#endregion

		#region Transaction
		public DbTransaction Begin()
		{
			SqliteTransaction module = connection.BeginTransaction();
			return AddModule(module); 
		}

		public async Task<DbTransaction> BeginAsync()
		{
			ValueTask<DbTransaction> value = connection.BeginTransactionAsync();
			return AddModule(await value.AsTask());
		}

		public Task CommitAsync()
		{
			DbTransaction trans = GetModule<DbTransaction>();
			return trans?.CommitAsync() ?? Task.CompletedTask;
		}
		public Task RollbackAsync() 
		{
			DbTransaction transaction = GetModule<DbTransaction>();
			return transaction?.RollbackAsync() ?? Task.CompletedTask;
		}

		#endregion

		#region Dispose
		public void PreventDisposal() => disposallocked = true;
		public void AllowDisposal() => disposallocked = false;

		public void Dispose()
		{
			if (disposallocked) return;

			for (int i = modules.Count - 1; i >= 0; i--)
			{
				IDisposable disposable = modules[i];
				disposable.Dispose();
			}

			modules.Clear();
			connection?.Dispose();
		}

		public ValueTask DisposeAsync()
		{
			return new ValueTask(DisposeConnectionsAsync());
		}

		private async Task DisposeConnectionsAsync()
		{
			if (disposallocked) return;

			for (int i = modules.Count - 1; i >= 0; i--)
			{
				IDisposable disposable = modules[i];
				if (disposable is IAsyncDisposable asyncDisp)
				{
					await asyncDisp.DisposeAsync().AsTask();
				}
				else
				{
					disposable.Dispose();
				}
			}

			modules.Clear();
			await connection?.DisposeAsync().AsTask();
		}
		#endregion
	}
}
