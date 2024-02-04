using Mono.Data.Sqlite;
using SqlLite.Wrapper.Serialization;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace SqlLite.Wrapper
{
	public delegate void CommandExecutedDelegate(SqliteCommand command, int operations, object context);
	public delegate void ExceptionDelegate<T>(T exception, SqliteContext context, object target)
		where T : Exception;

	public partial class SqliteHandler : ISqliteHandler
	{
		public const string PathPrefix = "URI=file:";

		private static readonly string defaultPath = PathPrefix + Application.persistentDataPath + "/sqlite.db";

		public event CommandExecutedDelegate OnCommandExecuted;
		public event ExceptionDelegate<Exception> OnException;

		private readonly Dictionary<Guid, SqliteContext> sharedContexts = new Dictionary<Guid, SqliteContext>();
		private readonly Dictionary<Guid, SqliteContext> contexts = new Dictionary<Guid, SqliteContext>();
		private readonly string path;
		private SqliteContext uContext;

		public SqliteHandler(string path = null)
		{
			this.path = path ?? defaultPath;
		}

		~SqliteHandler()
		{
			Dispose();
		}

		public void Dispose()
		{
			Debug.Log(contexts.Count + " Remaining connections.");

			foreach (KeyValuePair<Guid, SqliteContext> cnt in contexts)
			{
				SqliteContext connection = cnt.Value;
				connection.Dispose();
			}

			contexts.Clear();
		}

		public async Task DisposeConnectedAsync(SqliteContext context)
		{
			if (context.hasSharedConnection)
			{
				sharedContexts.Remove(context.guid);
				if (sharedContexts.Count == 0)
					await context.connection.DisposeAsync();
			}
			else
			{
				contexts.Remove(context.guid);
				await context.connection.DisposeAsync();
			}
		}

		public void DisposeConnected(SqliteContext context)
		{
			if (context.hasSharedConnection)
			{
				sharedContexts.Remove(context.guid);
				if (sharedContexts.Count == 0)
					context.connection.Dispose();
			}
			else
			{
				contexts.Remove(context.guid);
				context.connection.Dispose();
			}
		}

		private SqliteContext CreateContext(bool reuseConnections = true) 
		{
			if (uContext != null) return uContext;

			SqliteConnection connection = reuseConnections && sharedContexts.Count > 0 
				? sharedContexts.First().Value.connection
				: new SqliteConnection(path);

			Guid guid = Guid.NewGuid();

			connection.Disposed += DisposeConnection;
			void DisposeConnection(object sender, EventArgs e)
			{
				contexts.Remove(guid);
			}

			return contexts[guid] = new SqliteContext(Guid.NewGuid(), connection, this, reuseConnections);
		}

		private void ReadEntry(object entry, TableInfo table, DbDataReader reader)
		{
			Dictionary<string, object> values = GetColumnValues(reader);

			TableMember[] fields = table.fields;
			object v;
			for (int i = 0; i < fields.Length; i++)
			{
				TableMember field = fields[i];
				if (values.TryGetValue(field.Name, out v))
					field.SetValue(this, entry, v);
			}

			if (values.TryGetValue(table.identifier.Name, out v))
			{
				table.identifier.SetValue(this, entry, v);
			}

			if (entry is IOnDeserialized deserialized)
				deserialized.OnFinishRead();
		}

		private async Task ReadEntryAsync(object entry, TableInfo table, DbDataReader reader)
		{
			Dictionary<string, object> values = GetColumnValues(reader);

			TableMember[] fields = table.fields;
			if (values.TryGetValue(table.identifier.Name, out object v))
			{
				table.identifier.SetValue(this, entry, v);
			}

			for (int i = 0; i < fields.Length; i++)
			{
				TableMember field = fields[i];
				if (values.TryGetValue(field.Name, out v))
					await field.SetValueAsync(this, entry, v);
			}

			if (entry is IOnDeserialized deserialized)
				deserialized.OnFinishRead();
		}
	}
}
