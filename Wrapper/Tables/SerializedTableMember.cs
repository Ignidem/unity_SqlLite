using System;
using System.Reflection;
using System.Threading.Tasks;
using Utils.Serializers.CustomSerializers;

namespace SqlLite.Wrapper
{
	public partial class SqliteHandler
	{
		private class SerializedTableMember : TableMember
		{
			private readonly SerializerAttribute attribute;
			public override bool IsForeign => true;
			public override Type SerializedType => attribute.Serializer.SerializedType;

			public SerializedTableMember(MemberInfo member, SerializerAttribute attribute) : base(member)
			{
				this.attribute = attribute;
			}

			public override object GetValue(SqliteHandler context, object instance)
			{
				ISerializer serializer = attribute.Serializer;
				object value = base.GetValue(context, instance);
				return serializer.SerializeObject(value);
			}

			public override async Task<object> GetValueAsync(SqliteHandler context, object instance)
			{
				ISerializer serializer = attribute.Serializer;
				object value = base.GetValue(context, instance);
				return await serializer.SerializeObjectAsync(value);
			}

			public override void SetValue(SqliteHandler context, object instance, object value)
			{
				ISerializer serializer = attribute.Serializer;
				value = serializer.DeserializeObject(value);
				base.SetValue(context, instance, value);
			}

			public override async Task SetValueAsync(SqliteHandler context, object instance, object value)
			{
				ISerializer serializer = attribute.Serializer;
				value = await serializer.DeserializeObjectAsync(value);
				base.SetValue(context, instance, value);
			}
		}
	}
}
