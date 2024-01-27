using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Reflection;

namespace SqlLite.Wrapper.Serialization
{
	public class SqlSerializerAttribute : Attribute
	{
		public class _DefaultSerializers
		{
			public SqlSerializerAttribute this[Type type]
			{
				get
				{
					if (_serializers.TryGetValue(type, out SqlSerializerAttribute serializer))
						return serializer;

					if (type.TryGetAttribute(out serializer))
						return DefaultSerializers[type] = serializer;

					if (type.IsEnum)
					{

					}

					return null;
				}

				set
				{
					_serializers[type] = value;
				}
			}

			private Dictionary<Type, SqlSerializerAttribute> _serializers = new()
			{
				[typeof(Vector2Int)] = new SqlSerializerAttribute(typeof(Vector2IntSerializer)),
				[typeof(Vector3Int)] = new SqlSerializerAttribute(typeof(Vector3IntSerializer)),
				[typeof(Vector2)] = new SqlSerializerAttribute(typeof(Vector2Serializer)),
				[typeof(Vector3)] = new SqlSerializerAttribute(typeof(Vector3Serializer)),
				[typeof(Guid)] = new SqlSerializerAttribute(typeof(GuidSerializer)),
				[typeof(bool)] = new SqlSerializerAttribute(typeof(BoolSerializer)),
			};

			public bool TryGet(Type type, out SqlSerializerAttribute attr)
			{
				return (attr = this[type]) != null;
			}
		}

		public readonly static _DefaultSerializers DefaultSerializers = new();

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
			if (!serializerType.Inherits<ISqlSerializer>())
				throw new Exception($"Invalid Serializer " + serializerType);

			this.serializerType = serializerType;
		}
	}
}
