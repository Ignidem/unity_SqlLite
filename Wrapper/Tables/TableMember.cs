using Mono.Data.Sqlite;
using SqlLite.Wrapper.Serialization;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Utilities.Conversions;
using Utilities.Reflection;
using Utilities.Reflection.Members;

namespace SqlLite.Wrapper
{
	public partial class SqliteHandler
	{
		internal class TableMember
		{
			private static bool IsForeignReference(Type fieldType, string name, out Type table)
			{
				Type generic = typeof(ISqlTable<>);
				table = fieldType.GetInterfaces()
					.FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == generic);

				bool isSqlTable = table != null;
				if (!isSqlTable) return false;

				if (fieldType.IsInterface)
				{
					UnityEngine.Debug.LogWarning($"Interface member {name} requires a serializer.");
					return false;
				}
				else if (fieldType.IsAbstract)
				{
					UnityEngine.Debug.LogWarning($"Abstract member {name} requires a serializer.");
					return false;
				}
				else if (!IsTypeSupported(table.GetGenericArguments()[0]))
				{
					UnityEngine.Debug.LogWarning($"Unsuported Id Foreign Reference {name} requires a serializer.");
					return false;
				}

				return true;
			}

			private static bool IsTypeSupported(Type type)
			{
				return type.IsPrimitive || type == typeof(string)
					|| type == typeof(Guid);
			}

			public static implicit operator TableMember(MemberInfo member)
			{
				Type type = member switch
				{
					FieldInfo f => f.FieldType,
					PropertyInfo p => p.PropertyType ?? p.GetMethod.ReturnType,
					_ => null
				};

				if (type == null) return null;

				SqlSerializerAttribute serializerAttribute = member.GetCustomAttribute<SqlSerializerAttribute>();
				if (serializerAttribute?.IsValid ?? false)
					return new SerializedTableMember(member, serializerAttribute);

				if (SqlSerializerAttribute.DefaultSerializers.TryGet(type, out SqlSerializerAttribute seri))
					return new SerializedTableMember(member, seri);

				if (IsTypeSupported(type))
					return new TableMember(member);

				if (type.IsEnum)
					return new EnumTableMember(type, member);

				string name = member.DeclaringType.Name + '.' + member.Name;

				if (IsForeignReference(type, name, out Type table))
					return new ForeignTableMember(member, table);

				return null;
			}

			public string Name => Member.Name;
			public Type FieldType => member.ValueType();
			public virtual Type SerializedType => FieldType;
			public MemberInfo Member => member.MemberInfo;

			public bool IsNotSerializable => !CanRead || nonSerialized != null;
			public bool IsBackingField => member.MemberType == MemberTypes.Field && (Member as FieldInfo).IsBackingField();

			public bool CanRead => member.CanRead;
			public bool CanWrite => member.CanWrite;

			public virtual bool IsForeign => false;

			private readonly Member member;

			private readonly Delegate setter;
			private readonly Delegate getter;

			//Attributes
			private readonly NonSerializedAttribute nonSerialized;

			private Type Parent => member.MemberInfo.DeclaringType;

			public TableMember(MemberInfo member)
			{
				this.member = member;
				nonSerialized = member.GetCustomAttribute<NonSerializedAttribute>();
			}

			public SqliteParameter GetParameter(SqliteHandler context, object instance)
			{
				return new SqliteParameter(Name, GetValue(context, instance));
			}

			public async Task<SqliteParameter> GetParameterAsync(SqliteHandler context, object instance)
			{
				return new SqliteParameter(Name, await GetValueAsync(context, instance));
			}

			public virtual object GetValue(SqliteHandler context, object instance)
			{
				return member.Read(instance);
			}

			public virtual Task<object> GetValueAsync(SqliteHandler context, object instance)
			{
				return Task.FromResult(GetValue(context, instance));
			}

			public virtual void SetValue(SqliteHandler context, object instance, object value)
			{
				if (!CanWrite) return;

				if (value?.GetType() == typeof(DBNull))
					value = null;

				if (value != null && value.TryConvertTo(FieldType, out object v))
					value = v;

				member.Write(instance, value);
			}
			public virtual Task SetValueAsync(SqliteHandler context, object instance, object value)
			{
				SetValue(context, instance, value);
				return Task.CompletedTask;
			}

			public override string ToString()
				=> $"{Parent.Name}.{Name} ({FieldType.Name})";
		}
	}
}
