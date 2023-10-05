using System;
using System.Linq;
using System.Reflection;
using Utilities.Conversions;

namespace SqlLite.Wrapper
{
	public static partial class SqliteHandler
	{
		private class TableMember
		{
			private static bool IsForeignReference(Type type)
			{
				Type generic = typeof(ISqlTable<>);
				Type table = type.GetInterfaces()
					.FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == generic);
				return table != null;
			}

			private static bool IsTypeSupported(Type type)
			{
				if (type.IsEnum)
					type = Enum.GetUnderlyingType(type);

				return type.IsPrimitive || type == typeof(string)
					|| type == typeof(DateTime) || type == typeof(Guid)
					;
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

				if (IsTypeSupported(type))
					return new TableMember(member);

				if (IsForeignReference(type))
					return new TableForeignMember(member);

				return null;
			}

			public string Name => isField ? field.Name : prop.Name;
			public Type ValueType => isField ? field.FieldType : prop.PropertyType;
			public MemberInfo Member => isField ? field : prop;

			public bool IsNotSerializable => !CanRead || nonSerialized != null;

			public bool CanRead => isField || (prop.GetMethod?.IsPublic ?? false);
			public bool CanWrite => isField || (prop.SetMethod?.IsPublic ?? false);

			public virtual bool IsForeign => false;

			private readonly bool isField;
			private readonly FieldInfo field;
			private readonly PropertyInfo prop;

			//Attributes
			private readonly NonSerializedAttribute nonSerialized;

			private Type Parent => isField ? field.DeclaringType : prop.DeclaringType;

			public TableMember(MemberInfo member)
			{
				if (member is FieldInfo field)
				{
					isField = true;
					this.field = field;
				}
				else
				{
					prop = (PropertyInfo)member;
					isField = false;
				}

				nonSerialized = member.GetCustomAttribute<NonSerializedAttribute>();
			}

			public virtual object GetValue(object instance)
			{
				object value = GetRealValue(instance);

				return value;
			}

			protected object GetRealValue(object instance)
			{
				return isField ? field.GetValue(instance) : prop.GetValue(instance);
			}

			public virtual void SetValue(object instance, object value)
			{
				if (!CanWrite) return;

				if (value.GetType() == typeof(DBNull))
					value = null;

				value.TryConvertTo(ValueType, out object v);
				value = v;

				if (isField) field.SetValue(instance, value);
				else prop.SetValue(instance, value);
			}

			public override string ToString()
			 => $"{Parent.Name}.{Name} ({ValueType.Name})";
		}
	}
}
