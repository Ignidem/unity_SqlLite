using SqlLite.Wrapper.Serialization;
using System.Reflection;

namespace SqlLite.Wrapper
{
	public static partial class SqliteHandler
	{
		private class SerializedTableMember : TableMember
		{
			private readonly SqlSerializerAttribute attribute;

			public SerializedTableMember(MemberInfo member, SqlSerializerAttribute attribute) : base(member)
			{
				this.attribute = attribute;
			}

			public override object GetValue(object instance)
			{
				ISqlSerializer serializer = attribute.Serializer;
				object value = base.GetValue(instance);
				return serializer.Serialize(value);
			}

			public override void SetValue(object instance, object value)
			{
				ISqlSerializer serializer = attribute.Serializer;
				value = serializer.Deserialize(value);
				base.SetValue(instance, value);
			}
		}
	}
}
