using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlLite.Wrapper
{
	public static partial class SqliteHandler
	{
		private class TableInfo
		{
			private const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
			public readonly string name;

			public readonly TableMember identifier;

			public readonly TableMember[] fields;

			public readonly string create;
			public readonly string select;
			public readonly string save;

			public readonly AutoIncrementAttribute idAutoIncr;
			private readonly Type type;
			private readonly ConstructorInfo emptyConstr;

			public TableInfo(Type type)
			{
				this.type = type;
				name = type.Name;

				fields = LoadSerializableFields(type, ref identifier);

				idAutoIncr = type.GetCustomAttribute<AutoIncrementAttribute>()
					?? identifier?.Member.GetCustomAttribute<AutoIncrementAttribute>();

				select = $"SELECT * FROM {type.Name} WHERE {{0}} = @{identifier.Name}";

				create = $"CREATE TABLE '{type.Name}' ("
				+ $"'{identifier.Name}' {SqlType(identifier.ValueType)} PRIMARY KEY,"
				+ JoinFields(",", fields, field => $" '{field.Name}' {SqlType(field.ValueType)}{HasIncrement(field.Member)}")
				+ ");";

				save = "INSERT or REPLACE INTO " + $"{type.Name} " +
					$"(Id{JoinFields("", fields, f => $", '{f.Name}'")}) " +
					$"VALUES (@{identifier.Name}{JoinFields("", fields, f => $", @{f.Name}")})";

				CreateTable(this);

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

			public I AutoIncrementIndex<I>(object entry, I id)
			{
				if (id is not int i)
				{
					throw new Exception("Cannot auto increment non integer Id for " + type.Name);
				}

				//id was already previously set;
				if (i != 0) return id;
				
				object index = idAutoIncr.GetNextIndex(type);
				identifier.SetValue(entry, index);
				return (I)index;
			}
		}
	}
}
