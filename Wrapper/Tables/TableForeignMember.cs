using System;
using System.Reflection;
using Utilities.Reflection;

namespace SqlLite.Wrapper
{
	public partial class SqliteHandler
	{
		private class TableForeignMember : TableMember
		{
			private const string IdFieldName = nameof(ISqlTable<int>.Id);

			private readonly Type IdType;

			public override bool IsForeign => true;

			public TableForeignMember(MemberInfo member, Type table) : base(member)
			{
				IdType = table.GetGenericArguments()[0];
			}

			public override object GetValue(SqliteHandler context, object instance)
			{
				object value = GetRealValue(instance);

				if (value == null)
				{
					//return default id value;
					return IdType.GetDefault();
				}

				if (value is ISqlTable tbl)
					tbl.SaveAsync();

				Type type = value.GetType();
				PropertyInfo idProperty = type.GetProperty(IdFieldName);
				return idProperty.GetValue(value);
			}

			public override void SetValue(SqliteHandler context, object instance, object value)
			{
				object objValue = context.ReadOne(ValueType, value, IdFieldName, false);
				if (objValue == null) return;

				SetValue(context, instance, objValue);
			}
		}
	}
}
