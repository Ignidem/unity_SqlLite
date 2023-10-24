using System;
using System.Collections.Generic;

namespace SqlLite.Wrapper.Serialization
{
	public class SerializersCache
	{
		private readonly Dictionary<Type, ISqlSerializer> serializers = new();

		public ISqlSerializer this[Type type]
		{
			get
			{
				if (!serializers.TryGetValue(type, out ISqlSerializer serializer))
					serializers[type] = serializer = (ISqlSerializer)Activator.CreateInstance(type, new object[0]);

				return serializer;
			}
		}
	}
}
