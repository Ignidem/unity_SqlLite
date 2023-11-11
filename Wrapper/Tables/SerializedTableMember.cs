using SqlLite.Wrapper.Serialization;
using System.Reflection;
using System.Threading.Tasks;

namespace SqlLite.Wrapper
{
	public partial class SqliteHandler
	{
		private class SerializedTableMember : TableMember
		{
			private readonly SqlSerializerAttribute attribute;

			public SerializedTableMember(MemberInfo member, SqlSerializerAttribute attribute) : base(member)
			{
				this.attribute = attribute;
			}

			public override object GetValue(SqliteHandler context, object instance)
			{
				ISqlSerializer serializer = attribute.Serializer;
				object value = base.GetValue(context, instance);
				return serializer.Serialize(value);
			}

			public override async Task<object> GetValueAsync(SqliteHandler context, object instance)
			{
				ISqlSerializer serializer = attribute.Serializer;
				object value = base.GetValue(context, instance);
				return await serializer.SerializeAsync(value);
			}

			public override void SetValue(SqliteHandler context, object instance, object value)
			{
				ISqlSerializer serializer = attribute.Serializer;
				value = serializer.Deserialize(value);
				base.SetValue(context, instance, value);
			}

			public override async Task SetValueAsync(SqliteHandler context, object instance, object value)
			{
				ISqlSerializer serializer = attribute.Serializer;
				value = await serializer.DeserializeAsync(value);
				base.SetValue(context, instance, value);
			}
		}
	}
}
