using System;
using System.Reflection;
using Utilities.Conversions;

namespace SqlLite.Wrapper
{
	public partial class SqliteHandler
	{
		private class EnumTableMember : TableMember
		{
			public override Type SerializedType { get; }

			public EnumTableMember(Type type, MemberInfo member) : base(member)
			{
				SerializedType = Enum.GetUnderlyingType(type);
			}

			public override object GetValue(SqliteHandler context, object instance)
			{
				object value = base.GetValue(context, instance);
				value.TryConvertTo(SerializedType, out value);
				return value;
			}

			public override void SetValue(SqliteHandler context, object instance, object value)
			{
				value.TryConvertTo(FieldType, out value);
				base.SetValue(context, instance, value);
			}
		}
	}
}
