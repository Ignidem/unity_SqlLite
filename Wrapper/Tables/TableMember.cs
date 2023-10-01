using System;
using System.Reflection;

namespace SqlLite.Wrapper
{
	public class TableMember
	{
		public static implicit operator TableMember(MemberInfo member)
			=> new TableMember(member);

		public string Name => isField ? field.Name : prop.Name;
		public Type ValueType => isField ? field.FieldType : prop.PropertyType;
		public MemberInfo Member => isField ? field : prop;

		public bool IsNotSerializable => !CanRead || nonSerialized != null;

		public bool CanRead => isField || (prop.GetMethod?.IsPublic ?? false);
		public bool CanWrite => isField || (prop.SetMethod?.IsPublic ?? false);

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

		public object GetValue(object instance)
			=> isField ? field.GetValue(instance) : prop.GetValue(instance);

		public void SetValue(object instance, object value)
		{
			if (!CanWrite) return;

			if (value.GetType() == typeof(DBNull))
				value = null;

			if (ValueType == typeof(int))
			{
				value = Convert.ToInt32((long)value);
			}

			else if (ValueType == typeof(short))
			{
				value = Convert.ToInt16((long)value);
			}

			if (isField) field.SetValue(instance, value);
			else prop.SetValue(instance, value);
		}

		public override string ToString()
		 => $"{Parent.Name}.{Name} ({ValueType.Name})";
	}
}
