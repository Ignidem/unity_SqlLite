using SqlLite.Wrapper.Attributes;
using SqlLite.Wrapper.Serialization;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utilities.Reflection;

namespace SqlLite.Wrapper
{
	public partial class SqliteHandler
	{
		const string addFormat = "ALTER TABLE {0} ADD COLUMN {1} {0};";
		const string removeFormat = "ALTER TABLE {0} DROP COLUMN {1};";

		internal class TableInfo
		{
			private const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			public readonly string name;

			public readonly TableMember identifier;

			public readonly TableMember[] fields;

			public string CreateQuery { get; private set; }
			public string SelectQuery { get; private set; }
			public string SaveQuery { get; private set; }

			private readonly Type type;
			private readonly ConstructorInfo emptyConstr;
			public readonly AutoIncrementAttribute idAutoIncr;
			private readonly IEnumerable<TriggerAttribute> triggers;

			public TableInfo(Type type)
			{
				this.type = type;
				name = type.Name;

				fields = LoadSerializableFields(ref identifier);

				idAutoIncr = type.GetCustomAttribute<AutoIncrementAttribute>()
					?? identifier?.Member.GetCustomAttribute<AutoIncrementAttribute>();
				idAutoIncr?.SetType(type);

				triggers = type.GetCustomAttributes<TriggerAttribute>();

				if (type.TryGetAttribute(out SqlSerializerAttribute serializer))
				{
					SqlSerializerAttribute.DefaultSerializers[type] = serializer;
				}

				SelectQuery = $"SELECT * FROM {type.Name} WHERE {{0}} = @{identifier.Name}";

				UpdateCreateTableQuery();

				UpdateSaveQuery();

				emptyConstr = type.GetConstructor(new Type[0]);
			}

			private void UpdateSaveQuery()
			{
				SaveQuery = GetSaveQuery(0);
			}

			public string GetSaveQueryColumns()
			{
				const string savefieldFormat = "'{0}'";
				return '(' + string.Format(savefieldFormat, identifier.Name) + ", " +
						JoinFields(", ", fields, f => string.Format(savefieldFormat, f.Name))
						+ ')';
			}

			private string GetSaveQueryValues(int index)
			{
				const string savevalueFormat = "@{0}";
				string GetIndexSuffix() => index > 0 ? index.ToString() : null;
				return '(' + string.Format(savevalueFormat, identifier.Name + GetIndexSuffix()) + ", " +
					JoinFields(", ", fields, f => string.Format(savevalueFormat, f.Name + GetIndexSuffix()))
					+ ')';
			}

			public string GetSaveQuery(int count)
			{
				const string saveFormat = "INSERT OR REPLACE INTO {0} {1} VALUES {2}";
				StringBuilder query = new();
				query.AppendFormat(saveFormat, type.Name, GetSaveQueryColumns(), GetSaveQueryValues(0));
				for (int i = 1; i < count; i++)
					query.Append(", ").Append(GetSaveQueryValues(i));

				return query.Append(';').ToString();
			}

			private void UpdateCreateTableQuery()
			{
				const string createFormat = "CREATE TABLE {0} ('{1}' {2} PRIMARY KEY{3});";
				const string parameterFormat = ", '{0}' {1}{2}";

				string FormatField(TableMember field)
				{
					string sqlType = SqlType(field.SerializedType);
					string details = HasIncrement(field.Member);
					return string.Format(parameterFormat, field.Name, sqlType, details);
				}

				CreateQuery = string.Format(createFormat, type.Name,
					identifier.Name, SqlType(identifier.SerializedType),
					JoinFields("", fields,  FormatField)
				);
			}

			private TableMember[] LoadSerializableFields(ref TableMember _id)
			{
				List<MemberInfo> members = new List<MemberInfo>(type.GetFields(bindingFlags));
				members.AddRange(type.GetProperties(bindingFlags));

				List<TableMember> serialized = new List<TableMember>();
				const string id_Name = nameof(ISqlTable<int>.Id);
				for (int i = 0; i < members.Count; i++)
				{
					MemberInfo info = members[i];
					if (info is FieldInfo field && field.IsBackingField())
						continue;

					if (info.GetCustomAttribute<SqlIgnoreAttribute>() != null)
						continue;

					TableMember member = info;

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

					string columnType = collumnInfo["type"].ToString().Split(' ')[0];
					string value = SqlType(type);
					if (columnType.Equals(value, StringComparison.OrdinalIgnoreCase))
					{
						data.Remove(name);
						return;
					}

					await AlterColumnType(handler, name, type);
				}

				await VerifyMember(identifier.Name, identifier.SerializedType);

				for (int i = 0; i < fields.Length; i++)
				{
					TableMember mem = fields[i];
					await VerifyMember(mem.Name, mem.SerializedType);
				}

				foreach (KeyValuePair<string, Dictionary<string, object>> column in data)
				{
					await RemoveColumn(handler, column.Key);
				}

				UpdateCreateTableQuery();
				UpdateSaveQuery();
			}
			private Task CreateTable(SqliteHandler handler) => handler.ExecuteQueryAsync(CreateQuery);
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

			public void VerifyTableSync(SqliteHandler handler)
			{
				VerifyColumnsSync(handler);

				foreach (var trigger in triggers)
				{
					trigger.UpdateTriggerSync(handler);
				}
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

				VerifyMember(identifier.Name, identifier.SerializedType);

				for (int i = 0; i < fields.Length; i++)
				{
					TableMember mem = fields[i];
					VerifyMember(mem.Name, mem.SerializedType);
				}

				foreach (KeyValuePair<string, Dictionary<string, object>> column in data)
				{
					RemoveColumnSync(handler, column.Key);
				}
			}
			private void CreateTableSync(SqliteHandler handler) => handler.ExecuteQuery(CreateQuery);
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
