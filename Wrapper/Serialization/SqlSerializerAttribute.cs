using System;
using Utilities.Reflection;

namespace SqlLite.Wrapper.Serialization
{
	public class SqlSerializerAttribute : Attribute
	{
		private static readonly SerializersCache cache = new();

		public bool IsValid => serializerType != null;

		public readonly Type serializerType;

		public ISqlSerializer Serializer => IsValid ? cache[serializerType] : null;

		public SqlSerializerAttribute(Type deserializedType, Type serializedType) 
		{
			serializerType = typeof(SqlSerializer<,>).MakeGenericType(deserializedType, serializedType);
		}

		public SqlSerializerAttribute(Type serializerType)
		{
			if (!serializerType.Inherits<ISqlSerializer>()) return;

			this.serializerType = serializerType;
		}
	}
}
