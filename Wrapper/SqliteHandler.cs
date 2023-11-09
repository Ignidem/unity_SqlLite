using Mono.Data.Sqlite;
using SqlLite.Wrapper.Serialization;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using UnityEngine;

namespace SqlLite.Wrapper
{
	public partial class SqliteHandler : IDisposable
	{
		public const string PathPrefix = "URI=file:";

		private static readonly string defaultPath = PathPrefix + Application.persistentDataPath + "/sqlite.db";
		
		private readonly Dictionary<Guid, SqliteContext> connections = new Dictionary<Guid, SqliteContext>();
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
			Debug.Log(connections.Count + " Remaining connections.");

			foreach (KeyValuePair<Guid, SqliteContext> cnt in connections)
			{
				SqliteContext connection = cnt.Value;
				connection.Dispose();
			}

			connections.Clear();
		}

		private SqliteContext CreateContext() 
		{
			if (uContext != null) return uContext;

			SqliteConnection connection = new SqliteConnection(path);
			Guid guid = Guid.NewGuid();

			connection.Disposed += DisposeConnection;
			void DisposeConnection(object sender, EventArgs e)
			{
				connections.Remove(guid);
			}

			return connections[guid] = new SqliteContext(connection);
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
	}
}
