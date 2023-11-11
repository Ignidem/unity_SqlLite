using SqlLite.Wrapper.Attributes;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Threading.Tasks;

namespace SqlLite.Wrapper
{
	public partial class SqliteHandler
	{
		const string addFormat = "ALTER TABLE {0} ADD COLUMN {1} {0};";
		const string removeFormat = "ALTER TABLE {0} DROP COLUMN {1};";

		private class TableInfo
		{
			private const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
			public readonly string name;

			public readonly TableMember identifier;

			public readonly TableMember[] fields;

			public readonly string create;
			public readonly string select;
			public readonly string save;

			private readonly Type type;
			private readonly ConstructorInfo emptyConstr;
			public readonly AutoIncrementAttribute idAutoIncr;
			private readonly IEnumerable<TriggerAttribute> triggers;

			public TableInfo(Type type)
			{
				this.type = type;
				name = type.Name;

				fields = LoadSerializableFields(type, ref identifier);

				idAutoIncr = type.GetCustomAttribute<AutoIncrementAttribute>()
					?? identifier?.Member.GetCustomAttribute<AutoIncrementAttribute>();
				idAutoIncr?.SetType(type);

				triggers = type.GetCustomAttributes<TriggerAttribute>();

				select = $"SELECT * FROM {type.Name} WHERE {{0}} = @{identifier.Name}";

				create = $"CREATE TABLE '{type.Name}' ("
				+ $"'{identifier.Name}' {SqlType(identifier.ValueType)} PRIMARY KEY,"
				+ JoinFields(",", fields, field => $" '{field.Name}' {SqlType(field.ValueType)}{HasIncrement(field.Member)}")
				+ ");";

				save = "INSERT or REPLACE INTO " + $"{type.Name} " +
					$"(Id{JoinFields("", fields, f => $", '{f.Name}'")}) " +
					$"VALUES (@{identifier.Name}{JoinFields("", fields, f => $", @{f.Name}")})";

				emptyConstr = type.GetConstructor(new Type[0]);
			}

			private TableMember[] LoadSerializableFields(Type self, ref TableMember _id)
			{
				List<MemberInfo> members = new List<MemberInfo>(self.GetFields(bindingFlags));
				members.AddRange(self.GetProperties(bindingFlags));

				List<TableMember> serialized = new List<TableMember>();
				const string id_Name = nameof(ISqlTable<int>.Id);
				for (int i = 0; i < members.Count; i++)
				{
					TableMember member = members[i];

					if (member == null || member.IsNotSerializable)
						continue;

					if (member.Name == id_Name) 
					{
						_id = member;
						continue;
					}

					serialized.Add(member);
				}

				return serialized.ToArray();
			}

			private string HasIncrement(MemberInfo member)
			{
				if (member.IsDefined(typeof(AutoIncrementAttribute)))
					return " AUTOINCREMENT";
				return null;
			}

			public T ConstructEmpty<T>()
			{
				if (emptyConstr == null)
				{
					throw new Exception($"Class {name} requires an empty constructor.");
				}

				return (T)emptyConstr.Invoke(null);
			}

			public async Task VerifyTable(SqliteHandler handler)
			{
				await VerifyColumns(handler);

				foreach(var trigger in triggers)
				{
					await trigger.UpdateTrigger(handler);
				}
			}

			public void VerifyTableSync(SqliteHandler handler)
			{
				VerifyColumnsSync(handler);

				foreach (var trigger in triggers)
				{
					trigger.UpdateTriggerSync(handler);
				}
			}

			private async Task VerifyColumns(SqliteHandler handler)
			{
				using SqliteContext context = await handler.CreateContext().OpenAsync();
				DbDataReader reader = await context.QueryReaderAsync($"PRAGMA table_info({name})");

				if (!reader.Read())
				{
					await CreateTable(handler);
					return;
				}

				Dictionary<string, Dictionary<string, object>> data = new Dictionary<string, Dictionary<string, object>>();
				do
				{
					Dictionary<string, object> values = GetColumnValues(reader);
					data.Add(values["name"].ToString(), values);
				}
				while (reader.Read());

				async Task VerifyMember(string name, Type type)
				{
					if (!data.TryGetValue(name, out Dictionary<string, object> collumnInfo))
					{
						await AddColumn(handler, name, type);
						return;
					}

					string columnType = collumnInfo["type"].ToString();
					if (columnType.Equals(SqlType(type), StringComparison.OrdinalIgnoreCase))
					{
						data.Remove(name);
						return;
					}

					await AlterColumnType(handler, name, type);
				}

				await VerifyMember(identifier.Name, identifier.ValueType);

				for (int i = 0; i < fields.Length; i++)
				{
					TableMember mem = fields[i];
					await VerifyMember(mem.Name, mem.ValueType);
				}

				foreach (KeyValuePair<string, Dictionary<string, object>> column in data)
				{
					await RemoveColumn(handler, column.Key);
				}
			}
			private Task CreateTable(SqliteHandler handler) => handler.ExecuteQueryAsync(create);
			private async Task AlterColumnType(SqliteHandler handler, string name, Type type)
			{
				await RemoveColumn(handler, name);
				await AddColumn(handler, name, type);
			}
			private Task AddColumn(SqliteHandler handler, string name, Type type)
			{
				return handler.ExecuteQueryAsync(string.Format(addFormat, this.name, name, SqlType(type)));
			}
			private Task RemoveColumn(SqliteHandler handler, string name)
			{
				return handler.ExecuteQueryAsync(string.Format(removeFormat, this.name, name));
			}

			private void VerifyColumnsSync(SqliteHandler handler)
			{
				using SqliteContext context = handler.CreateContext().Open();
				DbDataReader reader = context.QueryReader($"PRAGMA table_info({name})");

				if (!reader.Read())
				{
					CreateTableSync(handler);
					return;
				}

				Dictionary<string, Dictionary<string, object>> data = new Dictionary<string, Dictionary<string, object>>();
				do
				{
					Dictionary<string, object> values = GetColumnValues(reader);
					data.Add(values["name"].ToString(), values);
				}
				while (reader.Read());

				void VerifyMember(string name, Type type)
				{
					if (!data.TryGetValue(name, out Dictionary<string, object> collumnInfo))
					{
						AddColumnSync(handler, name, type);
						return;
					}

					string columnType = collumnInfo["type"].ToString();
					if (columnType.Equals(SqlType(type), StringComparison.OrdinalIgnoreCase))
					{
						data.Remove(name);
						return;
					}

					AlterColumnTypeSync(handler, name, type);
				}

				VerifyMember(identifier.Name, identifier.ValueType);

				for (int i = 0; i < fields.Length; i++)
				{
					TableMember mem = fields[i];
					VerifyMember(mem.Name, mem.ValueType);
				}

				foreach (KeyValuePair<string, Dictionary<string, object>> column in data)
				{
					RemoveColumnSync(handler, column.Key);
				}
			}
			private void CreateTableSync(SqliteHandler handler) => handler.ExecuteQuery(create);
			private void AlterColumnTypeSync(SqliteHandler handler, string name, Type type)
			{
				RemoveColumnSync(handler, name);
				AddColumnSync(handler, name, type);
			}
			private void AddColumnSync(SqliteHandler handler, string name, Type type)
			{
				handler.ExecuteQuery(string.Format(addFormat, this.name, name, SqlType(type)));
			}
			private void RemoveColumnSync(SqliteHandler handler, string name)
			{
				handler.ExecuteQuery(string.Format(removeFormat, this.name, name));
			}
		}
	}
}
