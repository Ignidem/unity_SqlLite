using System;
using System.Reflection;

namespace SqlLite.Wrapper
{
	public static partial class SqliteHandler
	{
		private class TableForeignMember : TableMember
		{
			private readonly PropertyInfo idProperty;
			private Type IdType => idProperty.PropertyType;

			public override bool IsForeign => true;

			public TableForeignMember(MemberInfo member) : base(member)
			{
				idProperty = ValueType.GetProperty(nameof(ISqlTable<int>.Id));
			}

			public override object GetValue(object instance)
			{
				object value = GetRealValue(instance);

				if (value == null)
				{
					//return default id value;
					return Activator.CreateInstance(IdType);
				}

				if (value is ISqlTable tbl)
					tbl.Save();

				return idProperty.GetValue(value);
			}

			public override void SetValue(object instance, object value)
			{
				object objValue = LoadOne<object>(ValueType, value, idProperty.Name, false);
				if (objValue == null) return;

				SetValue(instance, objValue);
			}
		}
	}
}
