using System;
using System.Collections.Generic;
using System.Reflection;

namespace SqlLite.Wrapper
{
	public static partial class SqliteHandler
	{
		private class TableInfo
		{
			public readonly string name;

			public readonly TableMember identifier;

			public readonly TableMember[] fields;

			public readonly string create;
			public readonly string select;
			public readonly string save;

			private readonly ConstructorInfo emptyConstr;

			public TableInfo(Type type)
			{
				name = type.Name;
				identifier = type.GetProperty("Id");
				fields = LoadSerializableFields(type);

				select = $"SELECT * FROM {type.Name} WHERE {{0}} = @Id";

				create = $"CREATE TABLE '{type.Name}' ("
				+ $"'Id' {SqlType(identifier.ValueType)} PRIMARY KEY {HasIncrement(type)},"
				+ JoinFields(",", fields, field => $" '{field.Name}' {SqlType(field.ValueType)}{HasIncrement(field.Member)}")
				+ ");";

				save = "INSERT or REPLACE INTO " + $"{type.Name} " +
					$"(Id{JoinFields("", fields, f => ", " + f.Name)}) " +
					$"VALUES (@Id{JoinFields("", fields, f => $", @{f.Name}")})";

				CreateTable(this);

				emptyConstr = type.GetConstructor(new Type[0]);
			}

			private TableMember[] LoadSerializableFields(Type self)
			{
				const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;

				List<MemberInfo> members = new List<MemberInfo>(self.GetFields(bindingFlags));
				members.AddRange(self.GetProperties(bindingFlags));

				List<TableMember> serialized = new List<TableMember>();

				for (int i = 0; i < members.Count; i++)
				{
					TableMember member = members[i];
					if (member.Name == "Id" || member.IsNotSerializable) continue;
					Type type = member.ValueType;
					if (type.IsPrimitive || type == typeof(string) || type == typeof(DateTime)) serialized.Add(member);
				}

				return serialized.ToArray();
			}

			private string HasIncrement(MemberInfo member)
			{
				if (member.IsDefined(typeof(AutoIncrement)))
					return "AUTOINCREMENT";
				return null;
			}

			public T ConstructEmpty<T>()
			{
				if (emptyConstr == null)
				{
					throw new Exception($"Class {this.name} requires an empty constructor.");
				}

				return (T)emptyConstr.Invoke(null);
			}
		}
	}
}
