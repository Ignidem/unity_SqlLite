using Mono.Data.Sqlite;
using System;
using UnityEngine;

namespace SqlLite.Wrapper
{
	public partial class SqliteHandler
	{
		private static readonly string defaultPath = "URI=file:" + Application.persistentDataPath + "/ql_dtbs.db";

		private readonly string path;

		public SqliteHandler(string path = null)
		{
			this.path = path ?? defaultPath;
		}

		private SqliteCommand CreateQuery(string query)
		{
			SqliteConnection connection = new SqliteConnection(path);

			void DisposeConnection(object sender, EventArgs e)
			{
				connection.Dispose();
			}

			connection.Open();
			SqliteCommand cmd = connection.CreateCommand();
			cmd.CommandType = System.Data.CommandType.Text;
			cmd.CommandText = query;
			cmd.Disposed += DisposeConnection;
			return cmd;
		}
	}
}
