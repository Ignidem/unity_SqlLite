using System;
using System.Reflection;
using System.Threading.Tasks;
using Utilities.Conversions;
using Utilities.Reflection;

namespace SqlLite.Wrapper
{
	public partial class SqliteHandler
	{
		private class ForeignTableMember : TableMember
		{
			private const string IdFieldName = nameof(ISqlTable<int>.Id);
			private const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.FlattenHierarchy 
				| BindingFlags.Public | BindingFlags.NonPublic;
			private readonly Type IdType;

			public override bool IsForeign => true;
			public override Type SerializedType => IdType;

			public ForeignTableMember(MemberInfo member, Type table) : base(member)
			{
				IdType = table.GetGenericArguments()[0];
			}

			public override object GetValue(SqliteHandler context, object instance)
			{
				object value = base.GetValue(context, instance);

				if (value == null) 
					return IdType.GetDefault();

				if (value is ISqlTable tbl)
					tbl.Save();

				Type type = value.GetType();
				PropertyInfo idProperty = type.GetProperty(IdFieldName);
				return idProperty.GetValue(value);
			}

			public override async Task<object> GetValueAsync(SqliteHandler context, object instance)
			{
				object value = base.GetValue(context, instance);

				if (value == null)
					return IdType.GetDefault();

				if (value is ISqlTable tbl)
					await tbl.SaveAsync();

				Type type = value.GetType();
				PropertyInfo idProperty = type.GetProperty(IdFieldName, bindingFlags);
				return idProperty.GetValue(value);
			}

			public override void SetValue(SqliteHandler context, object instance, object value)
			{
				object objValue = context.ReadOne(FieldType, value, IdFieldName, false);
				if (objValue == null) return;
				base.SetValue(context, instance, objValue);
			}

			public override async Task SetValueAsync(SqliteHandler context, object instance, object value)
			{
				object objValue = await context.ReadOneAsync(FieldType, value, IdFieldName, false);
				if (objValue == null) return;
				base.SetValue(context, instance, objValue);
			}
		}
	}
}
